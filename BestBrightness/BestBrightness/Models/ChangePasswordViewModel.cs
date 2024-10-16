using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
