using System.ComponentModel.DataAnnotations;

namespace BestBrightness.ViewModels
{
    public class RegisterSalespersonViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "First Names")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Key]
        [Display(Name = "Salesperson ID")]
        public string SalespersonId { get; set; }

        [Display(Name = "Default Password")]
        public string DefaultPassword { get; set; }
    }
}
