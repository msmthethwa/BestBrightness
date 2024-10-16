using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BestBrightness.Models;
using BestBrightness.Data;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BestBrightness.Controllers
{
    public class CustomerAccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerAccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CustomerAccount/CustomerIndex
        public IActionResult CustomerIndex()
        {
            var viewModel = new CustomerAccountViewModel
            {
                CustomerRegister = new CustomerRegister(),
                CustomerLogin = new CustomerLogin()
            };
            return View(viewModel);
        }

        // POST: CustomerAccount/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(CustomerRegister model)
        {
            if (ModelState.IsValid)
            {
                var existingCustomer = await _context.Customers
                    .Where(c => c.Email == model.Email)
                    .FirstOrDefaultAsync();

                if (existingCustomer != null)
                {
                    TempData["EmailError"] = "This email is already registered.";
                    var viewModel = new CustomerAccountViewModel { CustomerRegister = model };
                    return View("CustomerIndex", viewModel);
                }

                var customer = new Customer
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = HashPassword(model.Password),
                    RegistrationDate = DateTime.Now
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                return RedirectToAction("CustomerIndex");
            }

            var vm = new CustomerAccountViewModel { CustomerRegister = model };
            return View("CustomerIndex", vm);
        }

        // POST: CustomerAccount/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(CustomerLogin model)
        {
            if (ModelState.IsValid)
            {
                var customer = await _context.Customers
                    .Where(c => c.Email == model.Email)
                    .FirstOrDefaultAsync();

                if (customer == null || !VerifyPassword(model.Password, customer.Password))
                {
                    ModelState.AddModelError("Email", "Invalid email or password.");
                    var viewModel = new CustomerAccountViewModel { CustomerLogin = model };
                    return View("CustomerIndex", viewModel);
                }

                // Create claims for the user (email and name, for example)
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, customer.FirstName),
            new Claim(ClaimTypes.Email, customer.Email)
        };

                // Create the claims identity
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Sign in the user and issue the authentication cookie
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                // Redirect to a logged-in area (e.g., CustomerHome)
                return RedirectToAction("Index", "CustomerHome");
            }

            var vm = new CustomerAccountViewModel { CustomerLogin = model };
            return View("CustomerIndex", vm);
        }

        private string HashPassword(string password)
        {
            // Implement your password hashing logic here
            // For example, use BCrypt or any other hashing method
            return password; // Placeholder
        }

        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            // Implement your password verification logic here
            // For example, compare hashed passwords
            return enteredPassword == storedPassword; // Placeholder
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("CustomerIndex", "CustomerAccount");
        }
    }
}
