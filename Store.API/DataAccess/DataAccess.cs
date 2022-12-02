using Microsoft.IdentityModel.Tokens;
using Store.API.Models;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Store.API.DataAccess
{
    public class DataAccess : IDataAccess
    {
        private readonly IConfiguration configuration;
        private readonly string dbconnection;
        private readonly string dateformat;

        public DataAccess(IConfiguration configuration)
        {
            this.configuration = configuration;
            dbconnection = this.configuration["ConnectionStrings:DB"];
            dateformat = this.configuration["Constant:DateFormat"];
        }

        public Cart GetActiveCartOfUser(int userId)
        {
            var cart = new Cart();

            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand cmd = new()
                {
                    Connection = connection
                };
                connection.Open();

                string query = "SELECT COUNT(*) From Carts WHERE UserId=" + userId + " AND Ordered='false';";
                cmd.CommandText = query;

                int count = (int)cmd.ExecuteScalar();

                if(count == 0)
                {
                    return cart;
                }

                query = "SELECT CartId From Carts WHERE UserId=" + userId + " AND Ordered='false';";
                cmd.CommandText = query;

                int cartId = (int)cmd.ExecuteScalar();

                query = "SELECT * FROM CartItems WHERE CartId=" + cartId + ";";
                cmd.CommandText = query;

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    CartItem item = new()
                    {
                        Id = (int)reader["CartItemId"],
                        Product = GetProduct((int)reader["ProductId"])
                    };
                    cart.CartItems.Add(item);
                }

                cart.Id = cartId;
                cart.User = GetUser(userId);
                cart.Ordered = false;
                cart.OrderedOn = "";
            }

            return cart;
        }

        public List<Cart> GetAllPreviousCartOfUser(int userId)
        {
            var carts = new List<Cart>();

            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand cmd = new()
                {
                    Connection = connection,
                };

                string query = "SELECT CartId FROM Carts Where UserId=" + userId + " AND Ordered='true';";

                cmd.CommandText = query;

                connection.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var cartId = (int)reader["CartId"];
                    carts.Add(GetCart(cartId));
                }
            }

            return carts;
        }

        public Cart GetCart(int cartId)
        {
            var cart = new Cart();

            using(SqlConnection connection = new(dbconnection))
            {
                SqlCommand cmd = new()
                {
                    Connection=connection,
                };
                connection.Open();

                string query = "SELECT * FROM CartItems WHERE CartId=" + cartId + ";";
                cmd.CommandText = query;

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    CartItem item = new()
                    {
                        Id = (int)reader["CartItemId"],
                        Product = GetProduct((int)reader["ProductId"])
                    };

                    cart.CartItems.Add(item);
                }
                reader.Close();

                query = "SELECT * FROM Carts WHERE CartId=" + cartId + ";";
                cmd.CommandText= query;
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cart.Id = cartId;
                    cart.User = GetUser((int)reader["UserId"]);
                    cart.Ordered = bool.Parse((string)reader["Ordered"]);
                    cart.OrderedOn = (string)reader["OrderedOn"];
                }
                reader.Close();
            }

            return cart;
        }

        public Offer GetOffer(int id)
        {
            var offer = new Offer();

            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand cmd = new()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM Offers WHERE OfferId=" + id + ";";
                cmd.CommandText = query;

                connection.Open();
                SqlDataReader r = cmd.ExecuteReader();

                while (r.Read())
                {
                    offer.Id = (int)r["OfferId"];
                    offer.Title = (string)r["Title"];
                    offer.Discount = (int)r["Discount"];
                }
            }

            return offer;
        }

        public List<PaymentMethod> GetPaymentMethod()
        {
            var result = new List<PaymentMethod>();

            using(SqlConnection connection = new(dbconnection))
            {
                SqlCommand cmd = new()
                {
                    Connection= connection
                };

                string query = "SELECT * FROM PaymentMethods;";
                cmd.CommandText = query;

                connection.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    PaymentMethod paymentMethod = new()
                    {
                        Id = (int)reader["PaymentMethodId"],
                        Type = (string)reader["Type"],
                        Provider = (string)reader["Provider"],
                        Available = bool.Parse((string)reader["Available"]),
                        Reason = (string)reader["Reason"]
                    };
                    result.Add(paymentMethod);
                }
            }

            return result;
        }

        public Product GetProduct(int id)
        {
            var product = new Product();

            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand cmd = new()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM Products WHERE ProductId=" + id + ";";
                cmd.CommandText = query;

                connection.Open();
                SqlDataReader r = cmd.ExecuteReader();

                while (r.Read())
                {
                    product.Id = (int)r["ProductId"];
                    product.Title = (string)r["Title"];
                    product.Description = (string)r["Description"];
                    product.Price = (double)r["Price"];
                    product.Quantity = (int)r["Quantity"];
                    product.ImageName = (string)r["ImageName"];

                    var categoryId = (int)r["CategoryId"];
                    product.ProductCategory = GetProductCategory(categoryId);

                    var offerId = (int)r["OfferId"];
                    product.Offer = GetOffer(offerId);
                }
            }

            return product;
        }

        public List<ProductCategory> GetProductCategories()
        {
            var productCategories = new List<ProductCategory>();

            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection,
                };

                string query = "SELECT * FROM ProductCategories;";

                command.CommandText = query;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var category = new ProductCategory()
                    {
                        Id = (int)reader["CategoryId"],
                        Category = (string)reader["Category"],
                        SubCategory = (string)reader["SubCategory"]
                    };
                    productCategories.Add(category);
                }
            }
            return productCategories;
        }

        public ProductCategory GetProductCategory(int id)
        {
            var productCategory = new ProductCategory();

            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand cmd = new()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM ProductCategories WHERE CategoryId=" + id + ";";
                cmd.CommandText = query;

                connection.Open();
                SqlDataReader r = cmd.ExecuteReader();

                while (r.Read())
                {
                    productCategory.Id = (int)r["CategoryId"];
                    productCategory.Category = (string)r["Category"];
                    productCategory.SubCategory = (string)r["SubCategory"];
                }
            }

            return productCategory;
        }

        public List<Review> GetProductReviews(int productId)
        {
            var reviews = new List<Review>();   

            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand cmd = new()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM Reviews WHERE ProductId=" + productId + ";";
                cmd.CommandText = query;

                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var review = new Review()
                    {
                        Id = (int)reader["ReviewId"],
                        Value = (string)reader["Review"],
                        CreatedAt = (string)reader["CreatedAt"]
                    };

                    var userId = (int)reader["UserId"];
                    review.User = GetUser(userId);

                    var prodId = (int)reader["ProductId"];
                    review.Product = GetProduct(prodId);

                    reviews.Add(review);
                }
            }

            return reviews;
        }

        public List<Product> GetProducts(string category, string subCategory, int count)
        {
            var products = new List<Product>();

            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand cmd = new()
                {
                    Connection = connection
                };

                string query = "SELECT TOP " + count + " * FROM Products WHERE CategoryId=(SELECT CategoryId FROM ProductCategories WHERE Category=@c AND SubCategory=@s) ORDER BY newid();";
                cmd.CommandText = query;
                cmd.Parameters.Add("@c", System.Data.SqlDbType.NVarChar).Value = category;
                cmd.Parameters.Add("@s", System.Data.SqlDbType.NVarChar).Value = subCategory;

                connection.Open();

                SqlDataReader r = cmd.ExecuteReader();

                while (r.Read())
                {
                    var product = new Product()
                    {
                        Id = (int)r["ProductId"],
                        Title = (string)r["Title"],
                        Description = (string)r["Description"],
                        Price = (double)r["Price"],
                        Quantity = (int)r["Quantity"],
                        ImageName = (string)r["ImageName"]
                    };

                    var categoryId = (int)r["CategoryId"];
                    product.ProductCategory = GetProductCategory(categoryId);

                    var offerId = (int)r["OfferId"];
                    product.Offer = GetOffer(offerId);

                    products.Add(product);
                }
            }

            return products;
        }

        public User GetUser(int id)
        {
            var user = new User();

            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand cmd = new()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM Users WHERE UserId=" + id + ";";
                cmd.CommandText = query;

                connection.Open();
                SqlDataReader r = cmd.ExecuteReader();

                while (r.Read())
                {
                    user.Id = (int)r["UserId"];
                    user.FirstName = (string)r["FirstName"];
                    user.LastName = (string)r["LastName"];
                    user.Email = (string)r["Email"];
                    user.Address = (string)r["Address"];
                    user.Mobile = (string)r["Mobile"];
                    user.Password = (string)r["Password"];
                    user.CreatedAt = (string)r["CreatedAt"];
                    user.ModifiedAt = (string)r["ModifiedAt"];
                }
            }

            return user;
        }

        public bool InsertCartItem(int userId, int productId)
        {
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand cmd = new()
                {
                    Connection = connection,
                };

                connection.Open();

                string query = "SELECT COUNT(*) FROM Carts WHERE UserId=" + userId + " AND Ordered='false';";

                cmd.CommandText = query;
                int count = (int)cmd.ExecuteScalar();

                if (count == 0)
                {
                    query = "INSERT INTO Carts (UserId, Ordered, OrderedOn) VALUES (" + userId + ", 'false', '');";

                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }

                query = "SELECT CartId FROM Carts WHERE UserId=" + userId + " AND Ordered='false';";
                cmd.CommandText= query;
                int cartId = (int)cmd.ExecuteScalar();

                query = "INSERT INTO CartItems (CartId, ProductId) VALUES (" + cartId + ", " + productId + ");";
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();

                return true;
            }
        }

        public int InsertOrder(Order order)
        {
            int value = 0;
            using (SqlConnection sqlConnection = new(dbconnection))
            {
                SqlCommand cmd = new()
                {
                    Connection = sqlConnection,
                };

                string query = @"INSERT INTO Orders (UserId, CartId, PaymentId, CreatedAt)
                            VALUES (@uid, @cid, @pid, @cat);";

                cmd.CommandText = query;
                cmd.Parameters.Add("@uid", System.Data.SqlDbType.Int).Value = order.User.Id;
                cmd.Parameters.Add("@cid", System.Data.SqlDbType.Int).Value = order.Cart.Id;
                cmd.Parameters.Add("@pid", System.Data.SqlDbType.Int).Value = order.Payment.Id;
                cmd.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = order.CreatedAt;

                sqlConnection.Open();
                value = cmd.ExecuteNonQuery();

                if (value > 0)
                {
                    query = "UPDATE Carts SET Ordered='true', OrderedOn='" + DateTime.Now.ToString(dateformat) + "' WHERE CartId=" + order.Cart.Id + ";";
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();

                    query = "SELECT TOP 1 Id FROM Orders ORDER BY Id DESC;";
                    cmd.CommandText = query;
                    value = (int)cmd.ExecuteScalar();
                }
                else
                {
                    value = 0;
                }
            }

            return value;
        }

        public int InsertPayment(Payment payment)
        {
            int value = 0;
            using (SqlConnection sqlConnection = new(dbconnection))
            {
                SqlCommand cmd = new()
                {
                    Connection = sqlConnection,
                };

                string query = @"INSERT INTO Payments (PaymentMethodId, UserId, TotalAmount, ShippingCharges, AmountReduced, AmountPaid, CreatedAt)
                            VALUES (@pmid, @uid, @ta, @sc, @ar, @ap, @cat);";

                cmd.CommandText = query;
                cmd.Parameters.Add("@pmid", System.Data.SqlDbType.Int).Value = payment.PaymentMethod.Id;
                cmd.Parameters.Add("@uid", System.Data.SqlDbType.Int).Value = payment.User.Id;
                cmd.Parameters.Add("@ta", System.Data.SqlDbType.NVarChar).Value = payment.TotalAmount;
                cmd.Parameters.Add("@sc", System.Data.SqlDbType.NVarChar).Value = payment.ShipingCharges;
                cmd.Parameters.Add("@ar", System.Data.SqlDbType.NVarChar).Value = payment.AmountReduced;
                cmd.Parameters.Add("@ap", System.Data.SqlDbType.NVarChar).Value = payment.AmountPaid;
                cmd.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = payment.CreatedAt;

                sqlConnection.Open();
                value = cmd.ExecuteNonQuery();

                if (value > 0)
                {
                    query = "SELECT TOP 1 Id FROM Payments ORDER BY Id DESC;";
                    cmd.CommandText = query;
                    value = (int)cmd.ExecuteScalar();
                }
                else
                {
                    value = 0;
                }
            }

            return value;
        }

        public void InsertReview(Review review)
        {
            using SqlConnection connection = new(dbconnection);

            SqlCommand cmd = new()
            {
                Connection = connection,
            };

            string query = "INSERT INTO Reviews (UserId, ProductId, Review, CreatedAt) VALUES (@uid, @pid, @rv, @cat);";
            cmd.CommandText = query;
            cmd.Parameters.Add("@uid", System.Data.SqlDbType.Int).Value = review.User.Id;
            cmd.Parameters.Add("@pid", System.Data.SqlDbType.Int).Value = review.Product.Id;
            cmd.Parameters.Add("@rv", System.Data.SqlDbType.NVarChar).Value = review.Value;
            cmd.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = review.CreatedAt;

            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public bool InsertUser(User user)
        {
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand cmd = new()
                {
                    Connection = connection,
                };

                connection.Open();

                string query = "SELECT COUNT(*) FROM Users WHERE Email='" + user.Email + "';";
                cmd.CommandText = query;

                int count = (int)cmd.ExecuteScalar();

                if (count > 0)
                {
                    connection.Close();

                    return false;
                }

                query = "INSERT INTO Users (FirstName, LastName, Address, Mobile, Email, Password, CreatedAt, ModifiedAt) values (@fn, @ln, @add, @mb, @em, @pwd, @cat, @mat);";

                cmd.CommandText = query;
                cmd.Parameters.Add("@fn", System.Data.SqlDbType.NVarChar).Value = user.FirstName;
                cmd.Parameters.Add("@ln", System.Data.SqlDbType.NVarChar).Value = user.LastName;
                cmd.Parameters.Add("@add", System.Data.SqlDbType.NVarChar).Value = user.Address;
                cmd.Parameters.Add("@mb", System.Data.SqlDbType.NVarChar).Value = user.Mobile;
                cmd.Parameters.Add("@em", System.Data.SqlDbType.NVarChar).Value = user.Email;
                cmd.Parameters.Add("@pwd", System.Data.SqlDbType.NVarChar).Value = user.Password;
                cmd.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = user.CreatedAt;
                cmd.Parameters.Add("@mat", System.Data.SqlDbType.NVarChar).Value = user.ModifiedAt;

                cmd.ExecuteNonQuery();
            }

            return true;
        }

        public string IsUserPresent(string email, string password)
        {
            User user = new();

            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand cmd = new()
                {
                    Connection = connection,
                };

                connection.Open();

                string query = "SELECT COUNT(*) FROM Users WHERE Email='" + email + "' AND Password='" + password + "';";
                
                cmd.CommandText = query;
                int count = (int)cmd.ExecuteScalar();
                if (count == 0)
                {
                    connection.Close();
                    return "";
                }

                query = "SELECT * FROM Users WHERE Email='" + email + "' AND Password='" + password + "';";
                cmd.CommandText = query;

                SqlDataReader r = cmd.ExecuteReader();

                while (r.Read())
                {
                    user.Id = (int)r["UserId"];
                    user.FirstName = (string)r["FirstName"];
                    user.LastName = (string)r["LastName"];
                    user.Email = (string)r["Email"];
                    user.Address = (string)r["Address"];
                    user.Mobile = (string)r["Mobile"];
                    user.Password = (string)r["Password"];
                    user.CreatedAt = (string)r["CreatedAt"];
                    user.ModifiedAt = (string)r["ModifiedAt"];
                }

                string key = "MNU66iBl3T5rh6H52i69";
                string duration = "60";
                var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("firstName", user.FirstName),
                    new Claim("lastName", user.LastName),
                    new Claim("address", user.Address),
                    new Claim("mobile", user.Mobile),
                    new Claim("email", user.Email),
                    new Claim("createdAt", user.CreatedAt),
                    new Claim("modifiedAt", user.ModifiedAt),
                };

                var jwtToken = new JwtSecurityToken(
                    issuer: "localhost",
                    audience: "localhost",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(Int32.Parse(duration)),
                    signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }

            return "";
        }
    }
}
