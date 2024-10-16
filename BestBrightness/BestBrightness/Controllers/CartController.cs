using BestBrightness.Data;
using BestBrightness.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;

namespace BestBrightness.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? GetCustomerIdByEmail(string email)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Email == email);
            return customer?.CustomerId;
        }

        public IActionResult Cart()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var customerId = GetCustomerIdByEmail(email);

            if (customerId == null)
            {
                return RedirectToAction("CustomerIndex", "CustomerAccount");
            }

            var cart = _context.Carts
                .Include(c => c.Items)
                .FirstOrDefault(c => c.CustomerId == customerId);

            if (cart == null)
            {
                cart = new Cart { CustomerId = customerId.Value };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, string productName, double price, byte[] productImage)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var customerId = GetCustomerIdByEmail(email);

            if (customerId == null)
            {
                return RedirectToAction("CustomerIndex", "CustomerAccount");
            }

            var cart = _context.Carts
                .Include(c => c.Items)
                .FirstOrDefault(c => c.CustomerId == customerId);

            if (cart == null)
            {
                cart = new Cart { CustomerId = customerId.Value };
                _context.Carts.Add(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            var product = _context.Products.Find(productId);

            if (product == null)
            {
                return RedirectToAction("Error", "Home"); // Handle error appropriately
            }

            if (product.StockLevel <= 0)
            {
                return RedirectToAction("OutOfStock", "Home"); // Handle out-of-stock appropriately
            }

            decimal disP = product.Discount ?? 0;
            double discountPercentage = (double)disP;
            double discountedPrice = price - (price * (discountPercentage / 100));

            if (existingItem != null)
            {
                if (existingItem.Quantity + 1 > product.StockLevel)
                {
                    return RedirectToAction("StockLimitExceeded", "Home"); // Handle stock limit exceeded
                }
                existingItem.Quantity++;
                existingItem.Price = discountedPrice; // Store the discounted price
                existingItem.Discount = discountPercentage; // Store the discount percentage
            }
            else
            {
                if (1 > product.StockLevel)
                {
                    return RedirectToAction("StockLimitExceeded", "Home"); // Handle stock limit exceeded
                }
                cart.Items.Add(new CartItems
                {
                    ProductId = productId,
                    ProductName = productName,
                    Price = discountedPrice, // Set the discounted price
                    Quantity = 1,
                    ProductImage = productImage, // Use ProductImage instead of ImagePath
                    Discount = discountPercentage // Store the discount percentage
                });
                product.StockLevel -= 1;
            }

            _context.Products.Update(product);
            _context.SaveChanges();

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult RemoveFromCart([FromBody] CartRemoveModel model)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var customerId = GetCustomerIdByEmail(email);

            if (customerId == null)
            {
                return RedirectToAction("CustomerIndex", "CustomerAccount");
            }

            var cart = _context.Carts
                .Include(c => c.Items)
                .FirstOrDefault(c => c.CustomerId == customerId);

            if (cart != null)
            {
                var itemToRemove = cart.Items.FirstOrDefault(i => i.ProductId == model.ProductId);
                if (itemToRemove != null)
                {
                    var product = _context.Products.Find(model.ProductId);
                    if (product != null)
                    {
                        product.StockLevel += itemToRemove.Quantity; // Add the quantity back to the stock
                        _context.Products.Update(product);
                        _context.SaveChanges();
                    }

                    cart.Items.Remove(itemToRemove);
                    _context.SaveChanges();
                }
            }

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult UpdateQuantity([FromBody] CartUpdateModel model)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var customerId = GetCustomerIdByEmail(email);

            if (customerId == null)
            {
                return Json(new { success = false, message = "Customer not found" });
            }

            var cart = _context.Carts
                .Include(c => c.Items)
                .FirstOrDefault(c => c.CustomerId == customerId);

            if (cart != null)
            {
                var itemToUpdate = cart.Items.FirstOrDefault(i => i.ProductId == model.ProductId);
                if (itemToUpdate != null)
                {
                    var product = _context.Products.FirstOrDefault(p => p.Id == model.ProductId);
                    if (product == null)
                    {
                        return Json(new { success = false, message = "Product not found" });
                    }

                    var previousQuantity = itemToUpdate.Quantity;
                    if (model.Quantity > product.StockLevel + previousQuantity)
                    {
                        return Json(new { success = false, message = "The requested quantity exceeds available stock." });
                    }

                    itemToUpdate.Quantity = model.Quantity;
                    product.StockLevel = product.StockLevel + previousQuantity - model.Quantity; // Update stock level accordingly
                    _context.Products.Update(product);
                    _context.SaveChanges();
                    return Json(new { success = true, newSubtotal = cart.Subtotal, newTotal = cart.Total });
                }
            }

            return Json(new { success = false, message = "Cart item not found" });
        }

        [HttpPost]
        public IActionResult GetProductStockLevel([FromBody] ProductStockRequestModel model)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == model.ProductId);

            if (product == null)
            {
                return Json(new { success = false });
            }

            return Json(new { success = true, stockLevel = product.StockLevel });
        }
    }
}