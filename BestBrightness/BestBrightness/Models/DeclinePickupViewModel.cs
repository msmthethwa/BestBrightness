using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class DeclinePickupViewModel
    {
        [Display(Name = "Pickup Id")]
        [Required]
        public string PickupId { get; set; }

        [Required]
        [Display(Name = "Reason for Decline")]
        public string DeclineReason { get; set; }
    }
}
