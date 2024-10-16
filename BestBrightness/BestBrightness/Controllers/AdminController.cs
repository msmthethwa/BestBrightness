using Microsoft.AspNetCore.Mvc;
using BestBrightness.ViewModels;
using BestBrightness.Data;
using BestBrightness.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;

namespace BestBrightness.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private static List<InventoryViewModel> _inventory = new List<InventoryViewModel>();
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Profile
        public async Task<IActionResult> UpdateProfile()
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
            if (currentUser == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Username = currentUser.Username,
                FullName = currentUser.FullName,
                Role = currentUser.Role,
                CreatedAt = currentUser.CreatedAt,
                ProfilePicturePath = currentUser.ProfilePicturePath 
            };

            return View(model);
        }

        // POST: Admin/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>UpdateProfile(ProfileViewModel model)
        {
            // Clear any existing ModelState errors for NewPassword and ConfirmPassword
            ModelState.Remove("NewPassword");
            ModelState.Remove("ConfirmPassword");

            // Only validate passwords if NewPassword is provided
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "The new password and confirmation password do not match.");
                }
            }

            if (ModelState.IsValid)
            {
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
                if (currentUser == null)
                {
                    TempData["ProfileUpdateMessage"] = "User not found.";
                    return RedirectToAction("UpdateProfile");
                }

                currentUser.FullName = model.FullName;

                // Only update the password if it has been provided
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    currentUser.Password = model.NewPassword; // Consider hashing the password
                }

                // Handle profile picture upload
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var fileName = Path.GetFileName(model.ProfilePicture.FileName);
                    var filePath = Path.Combine("wwwroot/images/profile_pictures", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(stream);
                    }

                    currentUser.ProfilePicturePath = "/images/profile_pictures/" + fileName; // Save the image path
                }

                try
                {
                    _context.Users.Update(currentUser);
                    await _context.SaveChangesAsync();
                    TempData["ProfileUpdateMessage"] = "Profile updated successfully.";
                }
                catch (Exception ex)
                {
                    TempData["ProfileUpdateMessage"] = $"Error updating profile: {ex.Message}";
                }

                return RedirectToAction("UpdateProfile");
            }

            return View(model);
        }


        // GET: Admin/RegisterSalesperson
        public IActionResult RegisterSalesperson()
        {
            var viewModel = new RegisterSalespersonViewModel
            {
                SalespersonId = GenerateSalespersonId(),
                DefaultPassword = GenerateDefaultPassword()
            };

            return View(viewModel);
        }

        // POST: Admin/RegisterSalesperson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterSalesperson(RegisterSalespersonViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the username (email) is already registered
                if (await _context.Users.AnyAsync(u => u.Username == model.Email))
                {
                    ModelState.AddModelError(string.Empty, "Email address is already registered. Please use a different email address.");
                    return View(model);
                }

                var user = new Users
                {
                    Username = model.Email,
                    Password = model.DefaultPassword,
                    FullName = $"{model.FirstName} {model.LastName}", // Store the full name
                    Role = "Salesperson",
                    SalespersonId = model.SalespersonId
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Redirect to a page displaying the salesperson ID and default password
                return RedirectToAction("SalespersonDetails", new { salespersonId = user.SalespersonId, defaultPassword = model.DefaultPassword });
            }

            return View(model);
        }

        // GET: Admin/EditSalesperson/{id}
        public IActionResult EditSalesperson(string id)
        {
            var user = _context.Users.FirstOrDefault(u => u.SalespersonId == id && u.Role == "Salesperson");
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new RegisterSalespersonViewModel
            {
                SalespersonId = user.SalespersonId,
                FirstName = GetFirstNames(user.FullName),
                LastName = GetLastName(user.FullName),
                Email = user.Username,
                DefaultPassword = user.Password // Populate password for display/edit
                                                // You can add more properties here if needed
            };

            return View(viewModel);
        }

        // POST: Admin/EditSalesperson/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSalesperson(string id, RegisterSalespersonViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.SalespersonId == id && u.Role == "Salesperson");
                if (user == null)
                {
                    return NotFound();
                }

                // Update the user's information
                user.Username = model.Email;
                user.FullName = $"{model.FirstName} {model.LastName}";
                user.Password = model.DefaultPassword; // Update password if edited

                // Save changes to the database
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Redirect to the salesperson details page or any other appropriate page
                TempData["EditSuccessMessage"] = "Salesperson details updated successfully."; // Store message in TempData
                return RedirectToAction("SalespersonDetails", new { salespersonId = user.SalespersonId, defaultPassword = user.Password });
            }

            return View(model);
        }


        // GET: Admin/SalespersonDetails/{salespersonId}
        public IActionResult SalespersonDetails(string salespersonId, string defaultPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.SalespersonId == salespersonId);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new SalespersonDetailsViewModel
            {
                SalespersonId = user.SalespersonId,
                FirstNames = GetFirstNames(user.FullName),
                LastName = GetLastName(user.FullName),
                Email = user.Username,
                DefaultPassword = defaultPassword // Pass the default password
            };

            return View(viewModel);
        }

        // GET: Admin/ViewSalespersons
        public IActionResult ViewSalespersons()
        {
            var salespersons = _context.Users
                .Where(u => u.Role == "Salesperson")
                .Select(u => new SalespersonDetailsViewModel
                {
                    SalespersonId = u.SalespersonId,
                    FirstNames = GetFirstNames(u.FullName),
                    LastName = GetLastName(u.FullName),
                    Email = u.Username,
                    DefaultPassword = u.Password
                })
                .ToList();

            return View(salespersons);
        }

        // Static helper methods to get first names and last name
        private static string GetFirstNames(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return string.Empty;

            var names = fullName.Split(' ');
            return names.Length > 1 ? string.Join(" ", names.Take(names.Length - 1)) : names[0];
        }

        private static string GetLastName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return string.Empty;

            var names = fullName.Split(' ');
            return names.Length > 1 ? names.Last() : string.Empty;
        }

        // GET: Admin/DeleteSalesperson/{id}
        public async Task<IActionResult> DeleteSalesperson(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.SalespersonId == id && u.Role == "Salesperson");
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("ViewSalespersons");
        }


        // Method to generate a unique salesperson ID
        private string GenerateSalespersonId()
        {
            var random = new Random();
            string id;
            do
            {
                id = random.Next(100000, 999999).ToString();
            } while (_context.Users.Any(u => u.SalespersonId == id));

            return id;
        }

        // Method to generate a default password
        private string GenerateDefaultPassword()
        {
            // Generate a default password (you can customize the logic as needed)
            return Guid.NewGuid().ToString().Substring(0, 8); // Example logic for generating a default password
        }

        // Method to compute SHA256 hash (if needed)
        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // GET: Admin/UploadProduct
        public async Task<IActionResult> UploadProduct()
        {
            var availableWarehouseStock = await _context.Stocks
                .Where(p => p.Quantity > 0)
                .ToListAsync();

            ViewBag.StockItems = availableWarehouseStock;

            ViewBag.Categories = await _context.Stocks
                .Select(s => s.Category)
                .Distinct()
                .ToListAsync();

            return View(new ProductUploadViewModel());
        }

        // POST: Admin/UploadProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProduct(ProductUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Fetch the stock item from the database
                var stockItem = await _context.Stocks.FirstOrDefaultAsync(s => s.Id == model.StockId);
                if (stockItem == null)
                {
                    TempData["ProductUploadMessage"] = "Stock item not found.";
                    return View(model);
                }

                // Ensure stock level is sufficient for the upload
                if (stockItem.Quantity < model.StockLevel)
                {
                    TempData["ProductUploadMessage"] = "Insufficient stock available.";
                    return View(model);
                }

                // Deduct the stock level from the stock item
                stockItem.Quantity -= model.StockLevel;

                // Start a transaction
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Save the stock deduction
                        _context.Stocks.Update(stockItem);
                        await _context.SaveChangesAsync();

                        // Handle the product image upload
                        byte[] productImageData = null;

                        if (model.ProductImage != null)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await model.ProductImage.CopyToAsync(memoryStream);
                                productImageData = memoryStream.ToArray();
                            }
                        }

                        // Check if the product already exists based on StockId or ProductName
                        var existingProduct = await _context.Products
                            .FirstOrDefaultAsync(p => p.Id == model.StockId || p.Name == model.ProductName);

                        if (existingProduct != null)
                        {
                            // If the product exists, update its stock level and other relevant details
                            existingProduct.StockLevel += model.StockLevel;
                            existingProduct.Price = model.Price; // Optionally update the price if needed
                            existingProduct.LastUpdatedDate = DateTime.Now;
                            existingProduct.ProductImage = productImageData ?? existingProduct.ProductImage; // Update image data if provided
                            existingProduct.Category = model.Category;
                            existingProduct.Rating = model.Rating;
                            existingProduct.ReviewCount = model.ReviewCount;

                            _context.Products.Update(existingProduct);
                        }
                        else
                        {
                            // Create a new product if it does not exist
                            var newProduct = new Products
                            {
                                Name = model.ProductName,
                                Description = model.ProductDescription,
                                StockLevel = model.StockLevel,
                                Price = model.Price,
                                LastUpdatedBy = "Admin",
                                LastUpdatedDate = DateTime.Now,
                                SalespersonId = "default_salesperson_id", // Adjust as necessary
                                ProductImage = productImageData,
                                Category = model.Category,
                                Rating = model.Rating,
                                ReviewCount = model.ReviewCount,
                            };

                            _context.Products.Add(newProduct);
                        }

                        await _context.SaveChangesAsync(); // Save changes

                        // Commit the transaction
                        await transaction.CommitAsync();

                        TempData["ProductUploadMessage"] = "Product uploaded successfully, and stock updated.";
                        return RedirectToAction("Admin", "Admin");
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction if any error occurs
                        await transaction.RollbackAsync();
                        TempData["ProductUploadMessage"] = $"Error uploading product or updating stock: {ex.Message}";
                        return View(model);
                    }
                }
            }

            var availableWarehouseStock = await _context.Stocks
                .Where(p => p.Quantity > 0)
                .ToListAsync();

            ViewBag.StockItems = availableWarehouseStock;

            ViewBag.Categories = await _context.Stocks
                .Select(s => s.Category)
                .Distinct()
                .ToListAsync();

            return View(model);
        }

        // GET: Admin/UpdateProduct/{id}
        public async Task<IActionResult> UpdateProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ProductUpdateMessage"] = "Product not found.";
                return RedirectToAction("AdminPage");
            }

            var model = new ProductUploadViewModel
            {
                ProductName = product.Name,
                ProductDescription = product.Description,
                StockLevel = product.StockLevel,
                Price = product.Price,
                Category = product.Category,

            };

            ViewBag.ProductId = id;
            return View(model);
        }

        // POST: Admin/UpdateProduct/{id}
        // POST: Admin/UpdateProduct/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProduct(int id, ProductUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    TempData["ProductUpdateMessage"] = "Product not found.";
                    return RedirectToAction("AdminPage");
                }

                // Update product details
                product.Name = model.ProductName;
                product.Description = model.ProductDescription;
                product.StockLevel = model.StockLevel;
                product.Price = model.Price;
                product.Category = model.Category;
                product.LastUpdatedBy = "Admin"; // or current user
                product.LastUpdatedDate = DateTime.Now;

                // Handle image file update if a new image is provided
                if (model.ProductImage != null)
                {
                    try
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await model.ProductImage.CopyToAsync(memoryStream);
                            product.ProductImage = memoryStream.ToArray(); // Save image as byte array
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["ProductUpdateMessage"] = $"Error uploading image: {ex.Message}";
                        return View(model);
                    }
                }

                try
                {
                    _context.Products.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["ProductUpdateMessage"] = "Product updated successfully.";
                }
                catch (DbUpdateException dbEx)
                {
                    TempData["ProductUpdateMessage"] = $"Database update error: {dbEx.Message}";
                }
                catch (Exception ex)
                {
                    TempData["ProductUpdateMessage"] = $"Error updating product: {ex.Message}";
                }

                return RedirectToAction("AdminPage");
            }

            // If validation fails, return to the view with the current model
            ViewBag.ProductId = id;
            return View(model);
        }

        // GET: Admin/AdminPage
        public IActionResult AdminPage()
        {
            var products = _context.Products
                .Select(p => new InventoryViewModel
                {
                    Id = p.Id,
                    ProductName = p.Name,
                    ProductDescription = p.Description,
                    StockLevel = p.StockLevel,
                    Price = p.Price,
                    LastUpdatedBy = p.LastUpdatedBy,
                    LastUpdatedDate = p.LastUpdatedDate,
                    ProductImage = p.ProductImage, // Include the image as a byte array
                    Category = p.Category,
                    Rating = p.Rating,
                    ReviewCount = p.ReviewCount,
                    Discount = p.Discount
                })
                .ToList();

            return View(products);
        }

        // GET: Admin/DeleteProduct/{id}
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ProductDeleteMessage"] = "Product not found.";
                return RedirectToAction("AdminPage");
            }

            // Optionally return a view to confirm deletion
            return View(product);
        }

        // POST: Admin/DeleteProduct/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id, IFormCollection form)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["ProductDeleteMessage"] = "Product not found.";
                return RedirectToAction("AdminPage");
            }

            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["ProductDeleteMessage"] = "Product deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ProductDeleteMessage"] = $"Error deleting product: {ex.Message}";
            }

            return RedirectToAction("AdminPage");
        }

        // View customer orders along with order items
        public async Task<IActionResult> ViewCustomerOrdersItems()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ToListAsync();
            return View(orders);
        }

        // Update order status, handle delivery date, and save feedback
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string action, string feedback, DateTime? deliveryDate)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order != null)
            {
                // Update order status based on the action
                if (action == "confirm")
                {
                    order.IsConfirmed = true;
                    order.IsCanceled = false; // Ensure the order is not marked as canceled when confirmed
                    order.DeliveryDate = deliveryDate; // Store the delivery date
                }
                else if (action == "reject")
                {
                    order.IsConfirmed = false;
                    order.IsCanceled = false; // Ensure the order is not marked as canceled when rejected
                }
                else if (action == "cancel")
                {
                    order.IsCanceled = true;
                    order.IsConfirmed = false; // Ensure the order is not marked as confirmed when canceled
                }

                // Save feedback
                order.Feedback = feedback;

                // Update the order in the database
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ViewCustomerOrdersItems");
        }

        // Separate method to confirm an order with delivery date
        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(int orderId, DateTime? deliveryDate, string feedback)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order != null)
            {
                // Confirm the order
                order.IsConfirmed = true;
                order.DeliveryDate = deliveryDate; // Store the delivery date
                order.Feedback = feedback;

                // Update the order in the database
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ViewCustomerOrdersItems");
        }

        // Action to display the Quotation page
        public async Task<IActionResult> Quotation()
        {
            var quotations = await _context.Quotations.ToListAsync();
            return View(quotations);
        }

        // Action to update feedback for a quotation
        [HttpPost]
        public async Task<IActionResult> UpdateFeedback(int quotationId, string feedback)
        {
            var quotation = await _context.Quotations.FindAsync(quotationId);
            if (quotation != null)
            {
                quotation.Feedback = feedback;
                _context.Update(quotation);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Quotation");
        }

        // Action to update product discount
        [HttpPost]
        public async Task<IActionResult> UpdateProductDiscount(int id, decimal discount)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.Discount = discount;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(AdminPage));
        }

        // View employees with details
        public IActionResult ViewEmployees()
        {
            var employee = _context.Users
                .Where(u => u.Role == "Manager")
                .Select(u => new EmployeeDetailsViewModel
                {
                    EmployeeId = u.SalespersonId,
                    FirstNames = GetFirstNames(u.FullName),
                    LastName = GetLastName(u.FullName),
                    Email = u.Username,
                    DefaultPassword = u.Password
                })
                .ToList();

            return View(employee);
        }

        // POST: Admin/RegisterEmployee
        [HttpGet]
        public IActionResult RegisterEmployee()
        {
            var viewModel = new RegisterEmployeeViewModel
            {
                EmployeeId = GenerateSalespersonId(),
                DefaultPassword = GenerateDefaultPassword()
            };

            return View(viewModel);
        }
        [Route("Admin/RegisterEmployee")]
        [HttpPost]
        public async Task<IActionResult> RegisterEmployee(RegisterEmployeeViewModel model)

        {
            if (ModelState.IsValid)
            {
                // Check if the username (email) is already registered
                if (await _context.Users.AnyAsync(u => u.Username == model.Email))
                {
                    ModelState.AddModelError(string.Empty, "Email address is already registered. Please use a different email address.");
                    return View(model);
                }

                var user = new Users
                {
                    Username = model.Email,
                    Password = model.DefaultPassword,
                    FullName = $"{model.FirstNames} {model.LastName}", // Store the full name
                    Role = "Manager",
                    SalespersonId = model.EmployeeId
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Redirect to a page displaying the salesperson ID and default password
                return RedirectToAction("EmployeeDetails", new { EmployeeId = user.SalespersonId, defaultPassword = model.DefaultPassword });
            }

            return View(model);
        }

        public IActionResult EmployeeDetails(string EmployeeId, string defaultPassword)
        {

            var user = _context.Users.FirstOrDefault(u => u.SalespersonId == EmployeeId);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new EmployeeDetailsViewModel
            {
                EmployeeId = user.SalespersonId,
                FirstNames = GetFirstNames(user.FullName),
                LastName = GetLastName(user.FullName),
                Email = user.Username,
                DefaultPassword = defaultPassword // Pass the default password
            };

            return View(viewModel);
        }

        public IActionResult EditEmployee(string id)
        {
            var user = _context.Users.FirstOrDefault(u => u.SalespersonId == id && u.Role == "Manager");
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new RegisterEmployeeViewModel
            {
                EmployeeId = user.SalespersonId,
                FirstNames = GetFirstNames(user.FullName),
                LastName = GetLastName(user.FullName),
                Email = user.Username,
                DefaultPassword = user.Password // Populate password for display/edit
                                                // You can add more properties here if needed
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(string id, RegisterEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.SalespersonId == id && u.Role == "Manager");
                if (user == null)
                {
                    return NotFound();
                }

                // Update the user's information
                user.Username = model.Email;
                user.FullName = $"{model.FirstNames} {model.LastName}";
                user.Password = model.DefaultPassword; // Update password if edited

                // Save changes to the database
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Redirect to the salesperson details page or any other appropriate page
                TempData["EditSuccessMessage"] = "Employee details updated successfully."; // Store message in TempData
                return RedirectToAction("SalespersonDetails", new { EmployeeId = user.SalespersonId, defaultPassword = user.Password });
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStockOrder(StockOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                foreach (var item in model.StockOrderItems)
                {
                    var stockItem = new StockItem
                    {
                        ProductName = item.ProductName,
                        QuantityOrdered = item.QuantityOrdered,
                        UnitPrice = item.UnitPrice
                    };
                    _context.StockItems.Add(stockItem);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Display"); // Redirect to a relevant page after saving


            }

            // If validation fails, return to the form
            return View(model);
        }



        public IActionResult CreateStockOrder()
        {
            var viewModel = new StockOrderViewModel
            {
                StockOrderItems = new List<StockOrderItemViewModel>
         {
             new StockOrderItemViewModel()
         }
            };
            return View(viewModel);
        }

        public IActionResult Display()
        {
            var orders = _context.StockItems.ToList();  // Fetching orders

            foreach (var order in orders)
            {
                if (order.Status == null)
                {
                    Console.WriteLine($"StockItem {order.StockItemId} has NULL Status.");
                }

                if (order.DeclineReason == null && order.Status == "Declined")
                {
                    Console.WriteLine($"StockItem {order.StockItemId} has NULL DeclineReason.");
                }
            }

            return View(orders);
        }

        // GET: Admin/EditStock/5
        public IActionResult EditStock(int id)
        {
            var stockItem = _context.StockItems.Find(id);
            if (stockItem == null)
            {
                return NotFound();
            }
            return View(stockItem);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            // Logic to find the stock item by id and remove it from the database
            var stockItem = _context.StockItems.Find(id);
            if (stockItem != null)
            {
                _context.StockItems.Remove(stockItem);
                _context.SaveChanges();
            }

            return RedirectToAction("Display"); // or wherever you want to redirect
        }


        // POST: Admin/EditStock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditStock(int id, StockItem stockItem)
        {
            if (id != stockItem.StockItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stockItem);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.StockItems.Any(e => e.StockItemId == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Display));
            }
            return View(stockItem);
        }

        public IActionResult Admin(string searchQuery = null)
        {
            // Fetch the current logged-in user
            var currentUser = _context.Users.FirstOrDefault(u => u.Username == User.Identity.Name);

            // Fetch all customers and products
            var customers = _context.Customers.ToList();
            var products = _context.Products.ToList();

            // Fetch damaged products
            var damagedProducts = products.Where(p => p.IsDamaged == true).ToList(); // Updated line

            // Apply search filtering if a search query is provided
            if (!string.IsNullOrEmpty(searchQuery))
            {
                customers = customers.Where(c =>
                    (c.FirstName + " " + c.LastName).Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    c.Email.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

                products = products.Where(p =>
                    p.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Calculate total amount from Orders and Sales
            var totalOrdersAmount = _context.Orders.Sum(o => o.Total);
            var totalSalesAmount = _context.Sales.Sum(s => s.TotalPrice);
            var totalEarnings = (decimal)totalOrdersAmount + totalSalesAmount;

            var viewModel = new AdminViewModel
            {
                Customers = customers,
                Products = products,
                DamagedProducts = damagedProducts, // Assign the damaged products
                QuotationsWithoutFeedbackCount = _context.Quotations.Count(q => string.IsNullOrEmpty(q.Feedback)),
                OrdersWithoutDeliveryDateCount = _context.Orders.Count(o => o.DeliveryDate == null && !o.IsCanceled),
                TotalEarnings = totalEarnings,
                SearchQuery = searchQuery, // Add search query to the ViewModel
                FilteredCustomers = customers, // Assign filtered customers
                FilteredProducts = products, // Assign filtered products
                ProfilePicturePath = currentUser?.ProfilePicturePath // Include the user's profile picture path
            };

            return View(viewModel);
        }

    }
}