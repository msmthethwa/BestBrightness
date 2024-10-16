using System;
using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class Products
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock level cannot be negative.")]
        public int StockLevel { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required]
        public DateTime LastUpdatedDate { get; set; }

        public string LastUpdatedBy { get; set; }

        public string SalespersonId { get; set; }

        [Required]
        public string Category { get; set; }

        [Range(0, 5)]
        public decimal Rating { get; set; }

        [Range(0, int.MaxValue)]
        public int ReviewCount { get; set; }

        [Range(0, 100)]
        public decimal? Discount { get; set; }

        // New property to store the product image as a byte array
        public byte[] ProductImage { get; set; }

        // New properties for indicating damaged items
        public bool? IsDamaged { get; set; }
        public bool? IsMarkedAsDamaged { get; set; }

        public int? DamagedStockLevel { get; set; }
        public int? UndamagedStockLevel { get; set; }

        public decimal? LossAmount
        {
            get
            {
                return DamagedStockLevel * Price;
            }
        }
    }
}