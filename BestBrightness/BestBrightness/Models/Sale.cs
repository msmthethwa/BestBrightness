using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BestBrightness.Models
{
    public class Sale
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Products Product { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public decimal PricePerUnit { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        public DateTime SaleDate { get; set; }

        [Required]
        public string SalespersonId { get; set; }
    }
}
