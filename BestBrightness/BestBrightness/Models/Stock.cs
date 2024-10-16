using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class Stock
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        [Display(Name = "Original Quantity")]
        public int OriginalQuantity { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Supplier")]
        public string Supplier { get; set; }

        [Display(Name = "Received Date")]
        public DateTime ReceivedDate { get; set; }

        [Display(Name = "Product Image")]
        public string? ImageBase64 { get; set; }

        public string? Description { get; set; }

        [DataType(DataType.Currency)]
        public decimal? Price { get; set; }

        public string? Category { get; set; }
    }
}
