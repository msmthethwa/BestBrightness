using System;
using System.ComponentModel.DataAnnotations;

namespace BestBrightness.ViewModels
{
    public class ProfileViewModel
    {
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        public string Role { get; set; }

        public DateTime CreatedAt { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile ProfilePicture { get; set; } // File upload property

        public string? ProfilePicturePath { get; set; }
    }
}
