using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class WarehouseOrder
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Product Id")]
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Supplier Id")]
        public int SupplierId { get; set; }

        [Required]
        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; }

        public string Status { get; set; }  // Created, Sent, Updated, Canceled

        // Navigation property to Supplier
        public string Supplier { get; set; } 
    }
}
