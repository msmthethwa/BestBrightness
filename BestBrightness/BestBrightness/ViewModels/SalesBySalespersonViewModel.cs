using System.Collections.Generic;

namespace BestBrightness.ViewModels
{
    public class SalesBySalespersonViewModel
    {
        public string SalespersonId { get; set; }
        public string SalespersonName { get; set; }
        public List<SaleViewModel> Sales { get; set; }
        public decimal TotalSalesPrice => Sales.Sum(s => s.TotalPrice);
    }

}