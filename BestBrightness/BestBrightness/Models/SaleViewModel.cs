using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BestBrightness.ViewModels
{
    public class SaleViewModel
    {
        [Key]
        [Required(ErrorMessage = "Product is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero")]
        public int Quantity { get; set; }

        public decimal PricePerUnit { get; set; }

        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Sale date is required")]
        public DateTime SaleDate { get; set; }

        [Required(ErrorMessage = "Salesperson ID is required")]
        public string SalespersonId { get; set; }

        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
    }

    public class ProductViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
    }

}
