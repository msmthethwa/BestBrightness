using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class CustomerLogin
    {
        [Key]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
