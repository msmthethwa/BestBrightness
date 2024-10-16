namespace BestBrightness.Models
{
    public class QuotationRequestViewModel
    {
        public string ProductDetails { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        // A property to hold the list of quotations
        public List<Quotation> Quotations { get; set; } = new List<Quotation>();
    }
}
