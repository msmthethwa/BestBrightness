using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class PaymentViewModel
    {
        [Required]
        [Display(Name = "Name on Card")]
        public string CardName { get; set; }

        [Required]
        [RegularExpression(@"\d{16}", ErrorMessage = "Card number must be exactly 16 digits.")]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }

        [Required]
        [RegularExpression(@"\d{2}/\d{2}", ErrorMessage = "Expiry date must be in MM/YY format.")]
        [Display(Name = "Expiry Date (MM/YY)")]
        public string ExpiryDate { get; set; }

        [Required]
        [RegularExpression(@"\d{3}", ErrorMessage = "CVV must be exactly 3 digits.")]
        [Display(Name = "CVV")]
        public string CVV { get; set; }

        [Display(Name = "Billing Address")]
        public string BillingAddress { get; set; }
    }
}
