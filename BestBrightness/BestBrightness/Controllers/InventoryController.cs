using Microsoft.AspNetCore.Mvc;
using BestBrightness.Data;
using BestBrightness.ViewModels;
using BestBrightness.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;

namespace BestBrightness.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var lowStockProducts = new List<string>();
            var viewModel = _context.Products.Select(p => new InventoryViewModel
            {
                Id = p.Id,
                ProductName = p.Name,
                ProductDescription = p.Description,
                StockLevel = p.StockLevel,
                Price = p.Price,
                LastUpdatedDate = p.LastUpdatedDate,
                LastUpdatedBy = p.LastUpdatedBy,
                SalespersonId = p.SalespersonId,
                ProductImage = p.ProductImage, 
                LowStockThreshold = 10 // Set the threshold here or get it from a configuration setting
            }).ToList();

            foreach (var item in viewModel)
            {
                if (item.StockLevel < item.LowStockThreshold)
                {
                    lowStockProducts.Add(item.ProductName);
                }
            }

            if (lowStockProducts.Any())
            {
                TempData["LowStockAlert"] = "The following products are low in stock: " + string.Join(", ", lowStockProducts);
            }

            return View(viewModel);
        }

        public async Task<IActionResult> UpdateStock(int id)
        {
            var item = await _context.Products.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            var viewModel = new InventoryViewModel
            {
                Id = item.Id,
                ProductName = item.Name,
                ProductDescription = item.Description,
                StockLevel = item.StockLevel,
                Price = item.Price,
                SalespersonId = "", // Initialize SalespersonId
                ProductImage = item.ProductImage,
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock(InventoryViewModel model)
        {
            var loggedInUsername = User.Identity.Name;
            var loggedInUser = await _context.Users.SingleOrDefaultAsync(u => u.Username == loggedInUsername);
            if (loggedInUser == null || loggedInUser.SalespersonId != model.SalespersonId)
            {
                ModelState.AddModelError("SalespersonId", "Invalid Salesperson ID.");
                return View(model);
            }

            var item = await _context.Products.FindAsync(model.Id);
            if (item == null)
            {
                return NotFound();
            }

            item.StockLevel = model.StockLevel;
            item.LastUpdatedBy = loggedInUser.FullName;
            item.LastUpdatedDate = DateTime.Now;

            _context.Update(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Stock updated successfully.";
            return RedirectToAction("Index");
        }
    }
}
