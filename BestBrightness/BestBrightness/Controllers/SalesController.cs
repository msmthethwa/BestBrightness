using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BestBrightness.ViewModels;
using BestBrightness.Models;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BestBrightness.Data;
using System.Collections.Generic;
using System.Text.Json;

namespace BestBrightness.Controllers
{
    public class SalesController : Controller
    {
        private readonly ILogger<SalesController> _logger;
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public SalesController(ILogger<SalesController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Displaying sales data.");

            var salesGroupedBySalesperson = _context.Sales
                .Include(s => s.Product)
                .GroupBy(s => s.SalespersonId)
                .Select(group => new SalesBySalespersonViewModel
                {
                    SalespersonId = group.Key,
                    SalespersonName = _context.Users
                        .Where(u => u.SalespersonId == group.Key)
                        .Select(u => u.FullName)
                        .FirstOrDefault(),
                    Sales = group.Select(s => new SaleViewModel
                    {
                        ProductId = s.ProductId,
                        Quantity = s.Quantity,
                        PricePerUnit = s.PricePerUnit,
                        TotalPrice = s.TotalPrice,
                        SaleDate = s.SaleDate,
                        SalespersonId = s.SalespersonId,
                        Products = _context.Products.Select(p => new ProductViewModel
                        {
                            Id = p.Id,
                            ProductName = p.Name,
                            Price = p.Price
                        }).ToList()
                    }).ToList()
                }).ToList();

            return View(salesGroupedBySalesperson);
        }

        [HttpGet]
        public IActionResult RecordSale()
        {
            _logger.LogInformation("Fetching products for sale.");
            var products = _context.Products.Select(p => new ProductViewModel
            {
                Id = p.Id,
                ProductName = p.Name,
                Price = p.Price
            }).ToList();

            var model = new SaleViewModel
            {
                Products = products,
                SaleDate = DateTime.Now
            };

            if (User.Identity.IsAuthenticated && User.IsInRole("Salesperson"))
            {
                var username = User.Identity.Name;
                var loggedInUser = _context.Users.SingleOrDefault(u => u.Username == username);
                model.SalespersonId = loggedInUser?.SalespersonId;
            }

            ViewData["CartItems"] = GetCart();

            return View(model);
        }

        [HttpPost]
        public IActionResult AddToCart(SaleViewModel model)
        {
            _logger.LogInformation("Adding sale to cart.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid.");
                return RedirectToAction("RecordSale");
            }

            var product = _context.Products.SingleOrDefault(p => p.Id == model.ProductId);
            if (product == null)
            {
                _logger.LogError("Product not found.");
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction("RecordSale");
            }

            var cart = GetCart();
            var cartItem = cart.SingleOrDefault(item => item.ProductId == model.ProductId);

            if (cartItem != null)
            {
                cartItem.Quantity += model.Quantity;
                cartItem.TotalPrice = cartItem.Price * (1 - (cartItem.Discount ?? 0) / 100) * cartItem.Quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Discount = product.Discount, // Set the discount from the product
                    Quantity = model.Quantity,
                    TotalPrice = product.Price * (1 - (product.Discount ?? 0) / 100) * model.Quantity // Calculate total price considering discount
                });
            }

            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));

            TempData["SuccessMessage"] = "Sale added to cart.";
            return RedirectToAction("Cart");
        }

        [HttpGet]
        public IActionResult Cart()
        {
            var cart = GetCart();
            var model = new CartViewModel
            {
                Items = cart
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult RecordCartSale(decimal amountPaid)
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Cart is empty.";
                return RedirectToAction("Cart");
            }

            // Calculate total amount
            var totalAmount = cart.Sum(item => item.TotalPrice);

            // Check for sufficient payment
            if (amountPaid < totalAmount)
            {
                TempData["ErrorMessage"] = "Amount paid is less than the total amount.";
                return RedirectToAction("Cart");
            }

            var sales = new List<Sale>();
            string salespersonName = null;
            string salespersonEmail = null;
            int? salespersonId = null;

            // Process sale
            foreach (var item in cart)
            {
                var product = _context.Products.SingleOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    if (product.StockLevel < item.Quantity)
                    {
                        TempData["ErrorMessage"] = $"Insufficient stock for product {item.ProductName}. Current stock level: {product.StockLevel}";
                        return RedirectToAction("Cart");
                    }

                    product.StockLevel -= item.Quantity;
                    var sale = new Sale
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        PricePerUnit = item.Price * (1 - (item.Discount ?? 0) / 100), // Use discounted price
                        TotalPrice = item.TotalPrice,
                        SaleDate = DateTime.Now,
                        SalespersonId = User.IsInRole("Salesperson") ? _context.Users.SingleOrDefault(u => u.Username == User.Identity.Name)?.SalespersonId : null
                    };

                    _context.Sales.Add(sale);
                    sales.Add(sale);

                    // Get Salesperson details if applicable
                    if (sale.SalespersonId != null)
                    {
                        var salespersonIdString = sale.SalespersonId.ToString(); // Convert SalespersonId to string
                        var salesperson = _context.Users.SingleOrDefault(u => u.SalespersonId == salespersonIdString);
                        if (salesperson != null)
                        {
                            salespersonName = salesperson.FullName;
                            salespersonEmail = salesperson.Username;
                            salespersonId = int.TryParse(salespersonIdString, out int id) ? id : (int?)null;
                        }
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = $"Product {item.ProductName} not found.";
                    return RedirectToAction("Cart");
                }
            }

            // Create invoice
            var invoice = new Invoice
            {
                InvoiceDate = DateTime.Now,
                AmountPaid = amountPaid,
                TotalAmount = totalAmount,
                Change = amountPaid - totalAmount,
                Sales = sales,
                SalespersonName = salespersonName,
                SalespersonEmail = salespersonEmail,
                SalespersonId = salespersonId
            };

            try
            {
                _context.Invoices.Add(invoice);
                _context.Products.UpdateRange(_context.Products.ToList());
                _context.SaveChanges();
                HttpContext.Session.Remove(CartSessionKey);

                // Create model for invoice
                var model = new InvoiceViewModel
                {
                    InvoiceId = invoice.InvoiceId, // Pass the InvoiceId to the view model
                    Items = cart,
                    AmountPaid = amountPaid,
                    Change = amountPaid - totalAmount,
                    SalespersonName = salespersonName,
                    SalespersonEmail = salespersonEmail,
                    SalespersonId = salespersonId
                };

                return View("Invoice", model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving changes to database: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while recording the sales. Please try again.";
                return RedirectToAction("Cart");
            }
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            _logger.LogInformation("Removing item from cart.");

            var cart = GetCart();
            var cartItem = cart.SingleOrDefault(item => item.ProductId == productId);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
                HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
                TempData["SuccessMessage"] = "Item removed from cart.";
            }
            else
            {
                TempData["ErrorMessage"] = "Item not found in cart.";
            }

            return RedirectToAction("Cart");
        }

        [HttpGet]
        public IActionResult GetProductDetails(int id)
        {
            var product = _context.Products
                .Where(p => p.Id == id)
                .Select(p => new { price = p.Price, discount = p.Discount })
                .FirstOrDefault();

            if (product == null)
            {
                return NotFound();
            }

            return Json(product);
        }

        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            return cartJson == null ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartJson);
        }

        [HttpPost]
        public IActionResult TestViewModelAccess()
        {
            TempData["TestMessage"] = "Accessing SaleViewModel";
            return RedirectToAction("RecordSale");
        }

        [HttpGet]
        public IActionResult GetProductStock(int id)
        {
            var stock = _context.Products
                .Where(p => p.Id == id)
                .Select(p => p.StockLevel)
                .FirstOrDefault();

            if (stock == null)
            {
                return NotFound();
            }

            return Json(stock);
        }
    }
}
