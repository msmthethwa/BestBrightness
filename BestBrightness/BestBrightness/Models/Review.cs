using System;
using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public string Content { get; set; }  // User review content

        [Required]
        [Range(1, 5)]
        public decimal Rating { get; set; }  // Rating value (1-5)

        [Required]
        public DateTime DateSubmitted { get; set; }  // When the review was submitted
    }
}
