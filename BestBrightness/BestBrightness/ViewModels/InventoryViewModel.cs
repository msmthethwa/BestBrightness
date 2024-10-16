using BestBrightness.Models;
using System;
using System.Collections.Generic; // Add this using directive
using System.ComponentModel.DataAnnotations;

namespace BestBrightness.ViewModels
{
    public class InventoryViewModel
    {
        // List of Products
        public IEnumerable<Products> Products { get; set; }

        // List of Damaged Products
        public IEnumerable<DamagedProducts> DamagedProducts { get; set; }

        public int DamagedProductsCount { get; set; } // Count of damaged products

        public int Id { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Display(Name = "Description")]
        public string ProductDescription { get; set; }

        [Display(Name = "Stock Level")]
        public int StockLevel { get; set; }

        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public string Category { get; set; } // New property
        public decimal Rating { get; set; } // New property
        public int ReviewCount { get; set; } // New property

        [Display(Name = "Last Updated Date")]
        public DateTime LastUpdatedDate { get; set; }

        [Display(Name = "Last Updated By")]
        public string LastUpdatedBy { get; set; }

        [Display(Name = "Salesperson ID")]
        public string SalespersonId { get; set; }

        [Display(Name = "Product Image")]
        public byte[] ProductImage { get; set; }

        public decimal? Discount { get; set; }

        // Constructor to initialize default values if needed
        public InventoryViewModel()
        {
            LastUpdatedDate = DateTime.Now; // Initialize LastUpdatedDate to current date/time
            DamagedProducts = new List<DamagedProducts>(); // Initialize the damaged products list
            Products = new List<Products>(); // Initialize the products list
        }

        // Predefined threshold for low stock (if used in view or processing logic)
        public int LowStockThreshold { get; set; } = 10;
    }
}