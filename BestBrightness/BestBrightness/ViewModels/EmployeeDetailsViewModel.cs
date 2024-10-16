using System.ComponentModel.DataAnnotations;

namespace BestBrightness.ViewModels
{
    public class EmployeeDetailsViewModel
    {
        [Key]
        public string? EmployeeId { get; set; }
        public string? FirstNames { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? DefaultPassword { get; set; }
    }
}