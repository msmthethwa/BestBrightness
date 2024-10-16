using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class Pickup
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Product Id")]
        public string ProductId { get; set; }
        public int Quantity { get; set; }

        [Display(Name = "Pickup Date")]
        public DateTime PickupDate { get; set; }
        public string Status { get; set; }

        [Display(Name = "Decline Reason")]
        public string DeclineReason { get; set; } // Optional, for declines
    }
}
