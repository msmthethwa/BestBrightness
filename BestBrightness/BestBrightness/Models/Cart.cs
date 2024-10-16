using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace BestBrightness.Models
{
    public class CartItems
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CartUserId { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public byte[] ProductImage { get; set; }
        public double Price { get; set; }  // Discounted price
        public int Quantity { get; set; }
        public double Discount { get; set; }  // Discount percentage
        public double Total => Price * Quantity;
    }

    public class Cart
    {
        [Key]
        public string UserId { get; set; } = Guid.NewGuid().ToString(); // Automatically generate a UserId
        public int CustomerId { get; set; }  // Use CustomerId from the Customer model
        public List<CartItems> Items { get; set; } = new List<CartItems>();
        public double Subtotal => CalculateSubtotal();
        public decimal Discount { get; set; }
        public double Tax => Subtotal * 0.10; // Example tax rate of 10%
        public double Total => Subtotal + Tax;

        private double CalculateSubtotal()
        {
            double subtotal = 0;
            foreach (var item in Items)
            {
                subtotal += item.Total;
            }
            return subtotal;
        }
    }
}