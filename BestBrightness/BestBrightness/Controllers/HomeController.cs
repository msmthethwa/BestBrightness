using BestBrightness.Data;
using BestBrightness.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BestBrightness.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user != null)
            {
                ViewData["FullName"] = user.FullName;
            }
            else
            {
                ViewData["FullName"] = username; // Fallback to username if full name is not found
            }

            // Check if current user is a salesperson
            ViewData["IsSalesperson"] = user.Role == "Salesperson";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TermsService()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize(Roles = "Salesperson")]
        public async Task<IActionResult> Profile()
        {
            var username = User.Identity.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Salesperson")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile ProfilePicture)
        {
            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                // Ensure the user directory exists
                var username = User.Identity.Name;
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles", username);
                Directory.CreateDirectory(uploadsFolder);

                // Save the file with a unique name
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePicture.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePicture.CopyToAsync(stream);
                }

                // Save the file path in the database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user != null)
                {
                    user.ProfilePicturePath = $"/images/profiles/{username}/{fileName}";
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Profile picture uploaded successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Please select a valid image file.";
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        [Authorize(Roles = "Salesperson")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["ErrorMessage"] = "New Password and Confirm Password do not match.";
                var username = User.Identity.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    return NotFound();
                }

                return View("Profile", user);
            }

            if (ModelState.IsValid)
            {
                var username = User.Identity.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    return NotFound();
                }

                user.Password = model.NewPassword;
                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Password changed successfully.";
                return RedirectToAction("Profile");
            }

            return View("Profile", model);
        }

        public IActionResult Home()
        {
            return View("Home");
        }
    }
}
