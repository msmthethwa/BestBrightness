using System;
using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class Users
    {
        [Key]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; }  // e.g., 'Admin', 'Salesperson'

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? SalespersonId { get; set; }

        [StringLength(255)]
        public string? ProfilePicturePath { get; set; }
    }
}

