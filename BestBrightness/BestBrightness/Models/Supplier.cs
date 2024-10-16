using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [Display(Name = "Contact Info")]
        public string ContactInfo { get; set; }

        public string Address { get; set; }
    }
}
