using Microsoft.AspNetCore.Mvc;
using BestBrightness.Models;
using System.Linq;
using BestBrightness.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks; // Import this for async/await

namespace BestBrightness.Controllers
{
    [Authorize]
    public class CustomerHomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Injecting the database context
        public CustomerHomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index() // Mark the method as async and return Task<IActionResult>
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("CustomerIndex", "CustomerAccount");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == userEmail);

            if (customer == null)
            {
                return NotFound();
            }

            // Fetching products from the database
            var products = await _context.Products.ToListAsync(); // Use await with async method

            // Passing the products list to the view
            ViewData["Customer"] = customer; // Passing the customer object to the view
            return View(products); // Returning the view with products
        }

        public async Task<IActionResult> Search(string query)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("CustomerIndex", "CustomerAccount");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == userEmail);

            if (customer == null)
            {
                return NotFound();
            }

            // Convert query and product names to lowercase for case-insensitive comparison
            var lowerQuery = query?.ToLower();

            var products = await _context.Products
                .Where(p => p.Name.ToLower().Contains(lowerQuery))
                .ToListAsync();

            ViewData["Customer"] = customer; // Passing the customer object to the view
            return View("Index", products); // Returning the view with filtered products
        }
    }
}
