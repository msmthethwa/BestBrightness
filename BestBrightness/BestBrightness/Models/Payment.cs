using System;
using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        [MaxLength(50)]
        public string CardName { get; set; }

        [Required]
        [MaxLength(16)]
        public string CardNumber { get; set; }

        [Required]
        [MaxLength(5)]
        public string ExpiryDate { get; set; }

        [Required]
        [MaxLength(3)]
        public string CVV { get; set; }

        [Required]
        public string BillingAddress { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;
    }
}
