namespace BestBrightness.Models
{
    public class StockItem
    {
        public int StockItemId { get; set; }
        public string ProductName { get; set; }
        public int QuantityOrdered { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Status { get; set; }
        public string? DeclineReason { get; set; }

    }
}