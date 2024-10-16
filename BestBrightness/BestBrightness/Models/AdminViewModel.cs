using System.Collections.Generic;

namespace BestBrightness.Models
{
    public class AdminViewModel
    {
        public IEnumerable<Customer> Customers { get; set; }
        public IEnumerable<Products> Products { get; set; }
        public int DamagedProductsCount { get; set; } // Damaged Products Count
        public int QuotationsWithoutFeedbackCount { get; set; }
        public int OrdersWithoutDeliveryDateCount { get; set; }
        public decimal TotalEarnings { get; set; }
        public IEnumerable<Products> FilteredProducts { get; set; }
        public IEnumerable<Customer> FilteredCustomers { get; set; }
        public IEnumerable<Products> DamagedProducts { get; set; }
        public string SearchQuery { get; set; }
        public string ProfilePicturePath { get; set; }

    }
}
