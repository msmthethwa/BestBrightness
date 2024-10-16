namespace BestBrightness.Models
{
    public class QuotationRequest
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Message { get; set; }

        // You might also want to include a list of products if you're using a dropdown
        public IEnumerable<Products> Products { get; set; }
    }
}
