namespace BestBrightness.Models
{
    public class PickupViewModel
    {
        public string PickupId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime PickupDate { get; set; }
        public string Status { get; set; }
    }
}
