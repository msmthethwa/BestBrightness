using System;
using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class Quotation
    {
        public int QuotationId { get; set; }
        public string ProductDetails { get; set; }
        public int Quantity { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public DateTime RequestDate { get; set; }

        [MaxLength(500, ErrorMessage = "Feedback must be 500 characters or less.")]
        public string? Feedback { get; set; }

    }
}
