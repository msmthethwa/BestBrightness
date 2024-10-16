using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BestBrightness.Models;
using BestBrightness.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BestBrightness.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customer/Profile
        public async Task<IActionResult> Profile()
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

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(Customer model, IFormFile profilePicture)
        {
            if (!ModelState.IsValid)
            {
                TempData["UpdateStatus"] = "Failed to update profile. Please check your inputs.";
                return View("Profile", model);
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["UpdateStatus"] = "Unable to verify your email address.";
                return RedirectToAction("CustomerIndex", "CustomerAccount");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == userEmail);

            if (customer == null)
            {
                TempData["UpdateStatus"] = "Profile not found.";
                return RedirectToAction("Profile");
            }

            // Update customer details
            customer.FirstName = model.FirstName;
            customer.LastName = model.LastName;
            customer.Password = model.Password;
            customer.Phone = model.Phone;
            customer.Address = model.Address;
            customer.DateOfBirth = model.DateOfBirth;
            customer.Gender = model.Gender;

            // Handle profile picture upload
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profile-pictures", profilePicture.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }

                customer.ProfilePicturePath = $"/images/profile-pictures/{profilePicture.FileName}";
            }

            try
            {
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
                TempData["UpdateStatus"] = "Profile updated successfully.";
            }
            catch
            {
                TempData["UpdateStatus"] = "An error occurred while updating the profile.";
            }

            return RedirectToAction("Profile");
        }

        // GET: Customer/Payment
        public IActionResult Payment()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var customer = _context.Customers.FirstOrDefault(c => c.Email == userEmail);

            if (customer == null)
            {
                return RedirectToAction("CustomerIndex", "CustomerAccount");
            }

            var model = new PaymentViewModel
            {
                BillingAddress = customer.Address
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ConfirmOrder(PaymentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Process the payment
                return RedirectToAction("PaymentSuccess");
            }

            return View("Payment", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitPayment(PaymentViewModel model)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("CustomerIndex", "CustomerAccount");
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);
            if (customer == null)
            {
                return NotFound();
            }

            // Retrieve the user's cart
            var cart = await _context.Carts.Include(c => c.Items)
                                           .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId);
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Cart", "Cart");
            }

            // Create a new order
            var order = new Order
            {
                CustomerId = customer.CustomerId,
                ShippingAddress = customer.Address,
                OrderDate = DateTime.Now,
                Subtotal = cart.Subtotal,
                Tax = cart.Tax,
                Total = cart.Total,
                IsConfirmed = true,
                OrderItems = cart.Items.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Quantity = item.Quantity
                }).ToList()
            };

            // Save the order to the database
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Clear the cart for the user
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            // Redirect to a payment success page
            return RedirectToAction("PaymentSuccess");
        }

        // GET: Customer/PaymentSuccess
        public IActionResult PaymentSuccess()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> QuotationRequest()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);

            if (customer == null)
            {
                return RedirectToAction("CustomerIndex", "CustomerAccount");
            }

            var viewModel = new QuotationRequestViewModel
            {
                Name = customer.FirstName + " " + customer.LastName,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                Quotations = await _context.Quotations
                    .Where(q => q.CustomerEmail == customer.Email)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuotationRequest(QuotationRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("QuotationRequest", model);
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);

            if (customer == null)
            {
                TempData["StatusMessage"] = "User details not found.";
                TempData["StatusClass"] = "alert-danger";
                return RedirectToAction("QuotationRequest");
            }

            // Create and save the quotation request
            var quotation = new Quotation
            {
                ProductDetails = model.ProductDetails,
                Quantity = model.Quantity,
                CustomerName = customer.FirstName + " " + customer.LastName,
                CustomerEmail = customer.Email,
                CustomerPhone = customer.Phone,
                CustomerAddress = customer.Address,
                RequestDate = DateTime.Now
            };

            _context.Quotations.Add(quotation);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Quotation request submitted successfully. Our team will contact you shortly.";
            TempData["StatusClass"] = "alert-success";

            return RedirectToAction("QuotationRequest");
        }


        // GET: Customer/Order
        public async Task<IActionResult> Order()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("CustomerIndex", "CustomerAccount");
            }

            var customer = await _context.Customers
                .Include(c => c.Orders) // Include orders
                .ThenInclude(o => o.OrderItems) // Include order items
                .FirstOrDefaultAsync(c => c.Email == userEmail);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer.Orders); // Pass orders to the view
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.IsCanceled)
            {
                return BadRequest("Order is already cancelled.");
            }

            // Set the order status to cancelled
            order.IsCanceled = true;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            // Redirect to the order history page or show a success message
            return RedirectToAction(nameof(Order));
        }

    }
}
