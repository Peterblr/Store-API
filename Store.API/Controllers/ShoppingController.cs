using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Store.API.DataAccess;
using Store.API.Models;

namespace Store.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingController : ControllerBase
    {
        private readonly IDataAccess dataAccess;
        private readonly string DateFormat;

        public ShoppingController(IDataAccess dataAccess, IConfiguration configuration)
        {
            this.dataAccess = dataAccess;
            DateFormat = configuration["Constants:DatrFormst"];
        }

        [HttpGet("GetCategoryList")]
        public IActionResult GetCategoryList()
        {
            var result = dataAccess.GetProductCategories();
            return Ok(result);
        }

        [HttpGet("GetProducts")]
        public IActionResult GetProducts(string category, string subcategory, int count)
        {
            var result = dataAccess.GetProducts(category, subcategory, count);

            return Ok(result);
        }

        [HttpGet("GetProduct/{id}")]
        public IActionResult GetProduct(int id)
        {
            var product = dataAccess.GetProduct(id);

            return Ok(product);
        }

        [HttpPost("RegisterUser")]
        public IActionResult RegisterUser([FromBody] User user)
        {
            user.CreatedAt = DateTime.Now.ToString(DateFormat);
            user.ModifiedAt = DateTime.Now.ToString(DateFormat);

            var result = dataAccess.InsertUser(user);

            string? message;

            if (result)
            {
                message = "inserted";
            } else
            {
                message = "email not available";
            }

            return Ok(message);
        }

        [HttpPost("LoginUser")]
        public IActionResult LoginUser([FromBody] User user)
        {
            var token = dataAccess.IsUserPresent(user.Email, user.Password);

            if (token == "") token = "invalid";

            return Ok(token);
        }

        [HttpPost("InsertReview")]
        public IActionResult InsertReview([FromBody] Review review)
        {
            review.CreatedAt = DateTime.Now.ToString(DateFormat);
            dataAccess.InsertReview(review);
            return Ok("inserted");
        }

        [HttpGet("GetProductReviews/{productId}")]
        public IActionResult GetProductReviews(int productId)
        {
            var result = dataAccess.GetProductReviews(productId);
            return Ok(result);
        }

        [HttpPost("InsertCartItem/{userId}/{productId}")]
        public IActionResult InsertCartItem(int userId, int productId)
        {
            var result = dataAccess.InsertCartItem(userId, productId);
            return Ok(result ? "inserted" : "not inserted");
        }

        [HttpGet("GetActiveCartOfUser/{id}")]
        public IActionResult GetActiveCartOfUser(int id)
        {
            var result = dataAccess.GetActiveCartOfUser(id);
            return Ok(result);
        }
        [HttpGet("GetAllPreviousCartOfUser/{id}")]
        public IActionResult GetAllPreviousCartOfUser(int id)
        {
            var result = dataAccess.GetAllPreviousCartOfUser(id);
            return Ok(result);
        }
        [HttpGet("GetPaymentMethod")]
        public IActionResult GetPaymentMethod()
        {
            var result = dataAccess.GetPaymentMethod();

            return Ok(result);
        }

        [HttpPost("InsertPayment")]
        public IActionResult InsertPayment(Payment payment)
        {
            payment.CreatedAt = DateTime.Now.ToString();
            var id = dataAccess.InsertPayment(payment);
            return Ok(id.ToString());
        }

        [HttpPost("InsertOrder")]
        public IActionResult InsertOrder(Order order)
        {
            order.CreatedAt = DateTime.Now.ToString();
            var id = dataAccess.InsertOrder(order);
            return Ok(id.ToString());
        }

    }
}
