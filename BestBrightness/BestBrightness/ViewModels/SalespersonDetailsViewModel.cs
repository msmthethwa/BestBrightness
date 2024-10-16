using System.ComponentModel.DataAnnotations;

namespace BestBrightness.ViewModels
{
    public class SalespersonDetailsViewModel
    {
        [Key]
        public string SalespersonId { get; set; }
        public string FirstNames{ get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DefaultPassword { get; set; }
    }
}
