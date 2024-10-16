using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class WarehouseProduct
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Supplier { get; set; }
        public DateTime ReceivedDate { get; set; }
        public string ImageBase64 { get; set; }
    }
}
