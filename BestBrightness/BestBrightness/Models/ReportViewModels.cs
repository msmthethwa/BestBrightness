using System.Collections.Generic;

namespace BestBrightness.ViewModels
{
    public class ReportViewModel
    {
        public List<DailySalesReportViewModel> DailySalesReport { get; set; }
        public List<InventoryReportViewModel> InventoryReport { get; set; }
    }

    public class DailySalesReportViewModel
    {
        public int InvoiceId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Change { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string FormattedInvoiceDate { get; set; } 
        public string SalespersonName { get; set; }
        public int? SalespersonId { get; set; }
    }

    public class InventoryReportViewModel
    {
        public string ProductName { get; set; }
        public int StockLevel { get; set; }
        public bool NeedsRestocking { get; set; }
    }
}
