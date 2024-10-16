using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public DateTime OrderDate { get; set; }

        public double Subtotal { get; set; }

        public double Tax { get; set; }

        public double Total { get; set; }

        public double TotalAmount { get; set; }

        [Required]
        [StringLength(200)]
        public string ShippingAddress { get; set; }

        // Foreign Key to Customer
        public int CustomerId { get; set; }

        // Navigation property to Customer
        public Customer Customer { get; set; }

        // Collection of Order Items
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // property for order confirmation status
        public bool IsConfirmed { get; set; }

        public bool IsCanceled { get; set; }

        // property for customer feedback
        public string? Feedback { get; set; }

        // property to store the delivery date
        public DateTime? DeliveryDate { get; set; }
    }

    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int OrderId { get; set; } // Foreign Key to Order

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public double Price { get; set; }

        public int Quantity { get; set; }

        // Navigation property to Order
        public Order Order { get; set; }
    }
}
