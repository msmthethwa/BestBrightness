using BestBrightness.Models;
using System.Collections.Generic;

namespace BestBrightness.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; } // Added discount property
        public int Quantity { get; set; }

        // Total price calculated in the view
        public decimal TotalPrice => Quantity * (Price - (Price * (Discount / 100 ?? 0)));
    }
}
