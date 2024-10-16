using BestBrightness.Models;

namespace BestBrightness.ViewModels
{
    public class SearchViewModel
    {
        public IEnumerable<StockItem> StockItemResults { get; set; }
        public List<Stock> StockResults { get; set; }
        public List<Supplier> SupplierResults { get; set; }
    }
}
