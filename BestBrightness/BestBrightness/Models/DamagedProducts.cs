using System;

namespace BestBrightness.Models
{
    public class DamagedProducts
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal LossAmount { get; set; }
        public DateTime DateReported { get; set; }
        public DateTime DamageDate { get; set; }

        public Products Product { get; set; } // Navigation property
    }
}
