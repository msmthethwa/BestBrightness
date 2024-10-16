using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        // Navigation property for Orders
        public ICollection<Order>? Orders { get; set; }

        [StringLength(200)]
        public string? ProfilePicturePath { get; set; }

    }
}
