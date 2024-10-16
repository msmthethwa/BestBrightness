using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class WarehouseLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

    }
}
