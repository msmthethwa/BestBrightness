using BestBrightness.Models;
using System.Collections.Generic;

namespace BestBrightness.ViewModels
{
    public class SearchResultsViewModel
    {
        public string Query { get; set; }  // The search query entered by the user
        public List<Products> Products { get; set; }  // List of products matching the query
        public List<Customer> Customers { get; set; }  // List of customers matching the query

        public SearchResultsViewModel()
        {
            Products = new List<Products>();
            Customers = new List<Customer>();
        }
    }
}
