using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class InventoryItem
    {
        [Key]
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int StockLevel { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
    }
}
