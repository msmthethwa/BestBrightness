using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BestBrightness.ViewModels
{
    public class InvoiceViewModel
    {
        [Key]
        public int InvoiceId { get; set; }
        public List<CartItem> Items { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Change { get; set; }

        // Add Salesperson details
        public string SalespersonName { get; set; }
        public string SalespersonEmail { get; set; }
        public int? SalespersonId { get; set; }
    }
}
