using System.ComponentModel.DataAnnotations;

namespace BestBrightness.Models
{
    public class WarehouseUser
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool IsAdmin { get; set; }
    }
}
