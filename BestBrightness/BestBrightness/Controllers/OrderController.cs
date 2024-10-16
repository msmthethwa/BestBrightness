using Microsoft.AspNetCore.Mvc;
using BestBrightness.Models;
using BestBrightness.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using BestBrightness.Data;
using System.Security.Claims;

namespace BestBrightness.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> OrderReview()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "CustomerAccount");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == userEmail);

            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);

            if (cart == null)
            {
                return NotFound("Cart not found");
            }

            var model = new OrderReviewViewModel
            {
                Subtotal = cart.Subtotal,
                Tax = cart.Tax,
                Total = cart.Total,
                OrderCarts = cart.Items.Select(item => new OrderCart
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    ProductImage = item.ProductImage // Use ProductImage instead of ImagePath
                }).ToList(),
                Customer = customer,
                ShippingAddress = customer.Address
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(OrderReviewViewModel model)
        {
            if (ModelState.IsValid)
            {
                var order = new Order
                {
                    CustomerId = model.Customer.CustomerId,
                    Subtotal = model.Subtotal,
                    Tax = model.Tax,
                    Total = model.Total,
                    ShippingAddress = model.ShippingAddress,
                    OrderDate = DateTime.Now,
                    OrderItems = model.OrderCarts.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }).ToList()
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                return RedirectToAction("OrderConfirmation");
            }

            return View("OrderReview", model);
        }

        public IActionResult OrderConfirmation()
        {
            return View();
        }
    }
}
