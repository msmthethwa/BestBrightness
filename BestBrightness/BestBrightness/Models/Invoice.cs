using System;
using System.Collections.Generic;

namespace BestBrightness.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Change { get; set; }
        public ICollection<Sale> Sales { get; set; }

        // Salesperson details
        public string SalespersonName { get; set; }
        public string SalespersonEmail { get; set; }
        public int? SalespersonId { get; set; }
    }
}
