using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class OrderReviewViewModel
    {
        [Key]
        public int OrderId { get; set; }
        public double Subtotal { get; set; }
        public double Tax { get; set; }
        public double Total { get; set; }
        public IEnumerable<OrderCart> OrderCarts { get; set; }
        public Customer Customer { get; set; }
        public string ShippingAddress { get; set; }
    }

    public class OrderCart
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public byte[] ProductImage { get; set; }
    }
}
