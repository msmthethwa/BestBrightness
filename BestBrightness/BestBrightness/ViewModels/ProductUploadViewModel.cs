using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BestBrightness.ViewModels
{
    public class ProductUploadViewModel
    {
        [Key]
        public int Id { get; set; }

        // Stock ID to select a stock item from the warehouse
        [Required]
        [Display(Name = "Select Product from Warehouse")]
        public int StockId { get; set; }


        [Required]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Display(Name = "Product Description")]
        public string ProductDescription { get; set; }

        // The quantity of stock being deducted/uploaded
        [Required]
        [Display(Name = "Stock Level to Deduct")]
        [Range(1, int.MaxValue, ErrorMessage = "Stock level must be greater than zero")]
        public int StockLevel { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        // Mark IFormFile as NotMapped to avoid EF Core trying to map this field
        [NotMapped]
        [Display(Name = "Product Image")]
        public IFormFile ProductImage { get; set; }

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; }

        // These fields can be initialized by default but are optional
        public decimal Rating { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;
    }
}