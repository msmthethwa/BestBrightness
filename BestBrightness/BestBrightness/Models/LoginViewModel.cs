using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class LoginViewModel
    {
        [Key]
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
