using Microsoft.AspNetCore.Mvc;
using BestBrightness.Data;
using BestBrightness.Models;
using System.Linq;
using System;
using BestBrightness.ViewModels;

namespace BestBrightness.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Product(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Fetch reviews for the product
            var reviews = _context.Reviews.Where(r => r.ProductId == id).ToList();
            ViewBag.Reviews = reviews;

            return View(product);
        }

        public IActionResult AdminPage()
        {
            var products = _context.Products.ToList();
            var damagedProducts = _context.DamagedProducts.ToList(); // Get list of damaged products
            var damagedProductsCount = damagedProducts.Count(); // Get count of damaged products

            var viewModel = new InventoryViewModel
            {
                Products = products,
                DamagedProducts = damagedProducts, // Set the damaged products
                DamagedProductsCount = damagedProductsCount // Set the count
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SubmitReview(int productId, string content, decimal rating)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);

            if (product == null)
            {
                return NotFound();
            }

            // Create new review entry
            var review = new Review
            {
                ProductId = productId,
                Content = content,
                Rating = rating,
                DateSubmitted = DateTime.Now
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            // Update product rating
            product.ReviewCount += 1;
            product.Rating = (_context.Reviews.Where(r => r.ProductId == productId).Average(r => r.Rating));

            _context.Products.Update(product);
            _context.SaveChanges();

            TempData["Message"] = "Thank you for your review!";
            return RedirectToAction("Product", new { id = productId });
        }

        [HttpPost]
        public IActionResult MarkAsDamaged(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                product.IsDamaged = true;
                product.DamagedStockLevel += 1; // Increment damaged stock level
                product.UndamagedStockLevel -= 1; // Decrement undamaged stock level
                _context.SaveChanges();
            }
            return RedirectToAction("ManageDamagedProducts");
        }

        [HttpPost]
        public IActionResult MarkAsUndamaged(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                product.IsDamaged = false;
                product.DamagedStockLevel -= 1; // Decrement damaged stock level
                product.UndamagedStockLevel += 1; // Increment undamaged stock level
                _context.SaveChanges();
            }
            return RedirectToAction("ManageDamagedProducts");
        }

        public IActionResult ManageDamagedProducts()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        [HttpPost]
        public IActionResult UpdateDamagedStockLevel(int id, int damagedStockLevel)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                // Ensure the updated value is valid
                if (damagedStockLevel >= 0 && damagedStockLevel <= product.StockLevel)
                {
                    product.DamagedStockLevel = damagedStockLevel;
                    product.UndamagedStockLevel = product.StockLevel - damagedStockLevel; // Adjust undamaged stock level

                    // Calculate LossAmount (if it's stored, otherwise just use the property)
                    // If you need to store LossAmount in the database, uncomment the following line:
                    // product.LossAmount = product.DamagedStockLevel * product.Price;

                    // Alternatively, if you want to keep LossAmount as a calculated property,
                    // you can comment out the above line and leave it as is.

                    _context.SaveChanges();
                }
            }

            return RedirectToAction("ManageDamagedProducts");
        }

    }
}
