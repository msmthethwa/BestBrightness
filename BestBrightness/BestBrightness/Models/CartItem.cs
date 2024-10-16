using System;
using System.ComponentModel.DataAnnotations;

namespace BestBrightness.ViewModels
{
    public class CartItem
    {
        [Key]
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal Price { get; set; }

        public decimal? Discount { get; set; } // Added Discount property

        public int Quantity { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
