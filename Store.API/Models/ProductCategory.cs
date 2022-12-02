namespace Store.API.Models
{
    public class ProductCategory
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
    }
}
