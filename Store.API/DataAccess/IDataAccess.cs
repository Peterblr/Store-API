using Store.API.Models;

namespace Store.API.DataAccess
{
    public interface IDataAccess
    {
        List<ProductCategory> GetProductCategories();
        ProductCategory GetProductCategory(int id);
        Offer GetOffer(int id);
        List<Product> GetProducts(string category, string subCategory, int count);
        Product GetProduct(int id);
        bool InsertUser(User user);
        string IsUserPresent(string email, string password);  
        void InsertReview(Review review);
        List<Review> GetProductReviews(int productId);
        User GetUser(int id);
        bool InsertCartItem(int userId, int productId);
        Cart GetActiveCartOfUser(int userId);   
        Cart GetCart(int cartId);
        List<Cart> GetAllPreviousCartOfUser(int userId);
        List<PaymentMethod> GetPaymentMethod();
        int InsertPayment(Payment payment);
        int InsertOrder(Order order); 
    }
}
