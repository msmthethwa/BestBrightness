using System.Collections.Generic;

namespace BestBrightness.ViewModels
{
    public class StockOrderViewModel
    {
        public List<StockOrderItemViewModel> StockOrderItems { get; set; } = new List<StockOrderItemViewModel>();
    }

    public class StockOrderItemViewModel
    {
        public string ProductName { get; set; }
        public int QuantityOrdered { get; set; }
        public decimal UnitPrice { get; set; }
    }
}