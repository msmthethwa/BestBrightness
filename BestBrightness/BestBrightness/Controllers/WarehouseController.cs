using BestBrightness.Models;
using BestBrightness.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Net;
using Rotativa.AspNetCore;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout;
using iText.IO.Image;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections;
using BestBrightness.Data;
using System.Security.Claims;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authentication;


namespace Warehouse.Controllers
{
    public class WarehouseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WarehouseController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: NewProduct
        public ActionResult NewProduct()
        {
            ViewBag.Categories = _context.Categories.ToList(); // Get existing categories
            var suppliers = _context.Suppliers.ToList();
            ViewBag.Suppliers = suppliers; // Get existing suppliers
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProducts(
            IFormFile[] images,
            string[] names,
            int[] quantities,
            string[] suppliers,
            DateTime[] receivedDates,
            string[] descriptions,
            decimal[] prices,
            string[] categories) // Categories passed from the form
        {
            if (ModelState.IsValid &&
                images.Length == names.Length &&
                images.Length == quantities.Length &&
                images.Length == suppliers.Length &&
                images.Length == receivedDates.Length &&
                images.Length == descriptions.Length &&
                images.Length == prices.Length &&
                images.Length == categories.Length)
            {
                for (int i = 0; i < images.Length; i++)
                {
                    // Convert the image to a Base64 string
                    string imageBase64 = string.Empty;
                    if (images[i] != null && images[i].Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await images[i].CopyToAsync(ms);
                            imageBase64 = Convert.ToBase64String(ms.ToArray());
                        }
                    }

                    // Create a new Stock object
                    var newStock = new Stock
                    {
                        Name = names[i],
                        Quantity = quantities[i],
                        Supplier = suppliers[i],
                        ReceivedDate = receivedDates[i],
                        OriginalQuantity = quantities[i],
                        ImageBase64 = imageBase64,
                        Description = descriptions[i],
                        Price = prices[i],
                        Category = categories[i] // Save the selected category
                    };

                    _context.Stocks.Add(newStock);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("StockList");
            }

            ViewBag.Categories = _context.Categories.ToList(); // Repopulate categories in case of an error
            ViewBag.Suppliers = _context.Suppliers.ToList(); // Repopulate suppliers
            return View("NewProduct");
        }



        public async Task<IActionResult> PrintDeliverySlip(string id)
        {
            var product = await _context.Stocks.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                document.Add(new Paragraph("Delivery Slip").SetFontSize(24));
                document.Add(new Paragraph($"Product Name: {product.Name}"));
                document.Add(new Paragraph($"Quantity: {product.Quantity}"));
                document.Add(new Paragraph($"Supplier: {product.Supplier}"));
                document.Add(new Paragraph($"Received Date: {product.ReceivedDate:yyyy-MM-dd}"));


        
        if (!string.IsNullOrEmpty(product.ImageBase64))
                {
                    var imageBytes = Convert.FromBase64String(product.ImageBase64);
                    using (var imageStream = new MemoryStream(imageBytes))
                    {
                        var image = new Image(ImageDataFactory.Create(imageStream.ToArray()));
                        image.SetAutoScale(true);
                        document.Add(image);
                    }
                }

                document.Close();

                return File(stream.ToArray(), "application/pdf", "DeliverySlip.pdf");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PrintAllDeliverySlips()
        {
            var allProducts = await _context.Stocks.ToListAsync();

            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Set document properties
                document.Add(new Paragraph("All Delivery Slips")
                    .SetFontSize(24)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBold()
                    .SetMarginBottom(20));

                foreach (var product in allProducts)
                {
                    document.Add(new LineSeparator(new SolidLine()).SetMarginBottom(10));

                    var productTable = new Table(2).UseAllAvailableWidth();
                    productTable.SetMarginBottom(20);

                    // Add product details
                    productTable.AddCell(new Cell().Add(new Paragraph("Product Name:").SetBold()));
                    productTable.AddCell(new Cell().Add(new Paragraph(product.Name)));

                    productTable.AddCell(new Cell().Add(new Paragraph("Quantity:").SetBold()));
                    productTable.AddCell(new Cell().Add(new Paragraph(product.Quantity.ToString())));

                    productTable.AddCell(new Cell().Add(new Paragraph("Supplier:").SetBold()));
                    productTable.AddCell(new Cell().Add(new Paragraph(product.Supplier)));

                    productTable.AddCell(new Cell().Add(new Paragraph("Received Date:").SetBold()));
                    productTable.AddCell(new Cell().Add(new Paragraph(product.ReceivedDate.ToString("yyyy-MM-dd"))));

                    // Add Description, Price, and Category
                    if (!string.IsNullOrEmpty(product.Description))
                    {
                        productTable.AddCell(new Cell().Add(new Paragraph("Description:").SetBold()));
                        productTable.AddCell(new Cell().Add(new Paragraph(product.Description)));
                    }

                    if (product.Price.HasValue)
                    {
                        productTable.AddCell(new Cell().Add(new Paragraph("Price:").SetBold()));
                        productTable.AddCell(new Cell().Add(new Paragraph(product.Price.Value.ToString("C"))));  // Format as currency
                    }

                    if (!string.IsNullOrEmpty(product.Category))
                    {
                        productTable.AddCell(new Cell().Add(new Paragraph("Category:").SetBold()));
                        productTable.AddCell(new Cell().Add(new Paragraph(product.Category)));
                    }

                    document.Add(productTable);
                }

                document.Close();

                return File(stream.ToArray(), "application/pdf", "AllDeliverySlips.pdf");
            }
        }



        // GET: Warehouse/CreateSupplier
        public IActionResult CreateSupplier()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSupplier([Bind("Name,ContactInfo,Address")] Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                // No need to set Id if it is an identity column
                // The database will generate the Id automatically
                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();

                // Redirect to the SupplierList page
                return RedirectToAction("SupplierList");
            }

            // If model state is invalid, return the form with validation errors
            return View(supplier); // Return the supplier object to preserve user input
        }



        // Action for Supplier List
        public async Task<IActionResult> SupplierList(string searchTerm)
        {
            var suppliers = _context.Suppliers.AsQueryable();

            // If there is a search term, filter the suppliers
            if (!string.IsNullOrEmpty(searchTerm))
            {
                suppliers = suppliers.Where(s => s.Name.Contains(searchTerm) || s.ContactInfo.Contains(searchTerm));
            }

            // Fetch the filtered list asynchronously
            var supplierList = await suppliers.ToListAsync();
            return View(supplierList);
        }



        private bool SupplierExists(int id)
        {
            return _context.Suppliers.Any(e => e.Id == id);
        }



        // GET: Warehouse/EditSupplier/{id}
        [HttpGet]
        [Route("EditSupplier/{id}")]
        public async Task<IActionResult> EditSupplier(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }
            return View(supplier);
        }


        // POST: Warehouse/UpdateSupplier
        [HttpPost]
        [Route("UpdateSupplier")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSupplier(Supplier supplier)
        {
            if (!ModelState.IsValid)
            {
                return View(supplier);
            }

            var existingSupplier = await _context.Suppliers.FindAsync(supplier.Id);
            if (existingSupplier == null)
            {
                return NotFound();
            }

            existingSupplier.Name = supplier.Name;
            existingSupplier.ContactInfo = supplier.ContactInfo;
            existingSupplier.Address = supplier.Address;

            _context.Suppliers.Update(existingSupplier);
            await _context.SaveChangesAsync();

            return RedirectToAction("SupplierList");
        }

        [HttpGet]
        [Route("DeleteOrder/{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.WarehouseOrders
                .Include(o => o.Supplier) // Assuming you have a navigation property for Supplier
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers.FindAsync(order.SupplierId);
            ViewBag.SupplierName = supplier?.Name; // Add supplier name to the ViewBag

            return View(order);
        }

        // GET: Warehouse/DeleteSupplier/3
        public async Task<IActionResult> DeleteSupplier(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }


        // POST: Warehouse/RemoveSupplier
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSupplier(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }
            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return RedirectToAction("SupplierList");
        }

        // Action for Stock List
        public async Task<IActionResult> StockList(string searchTerm)
        {
            var stocks = _context.Stocks.AsQueryable();

            // If there is a search term, filter the stocks
            if (!string.IsNullOrEmpty(searchTerm))
            {
                stocks = stocks.Where(s => s.Name.Contains(searchTerm));
            }

            // Convert to a list asynchronously
            var stockList = await stocks.ToListAsync();
            return View(stockList);
        }

        [HttpGet]
        [Route("Pickup/{id}")]
        public async Task<IActionResult> PickupStock(string id)
        {
            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null)
            {
                return NotFound();
            }

            // For example, remove the stock from the database or mark it as picked up
            return RedirectToAction(nameof(StockList));
        }

        private bool StockExists(int id)
        {
            // Convert 'id' to string to compare with 'Stock.Id', assuming 'Stock.Id' is a string
            return _context.Stocks.Any(e => e.Id == id);
        }

        [HttpGet]
        [Route("PickupDetails/{id}")]
        public async Task<IActionResult> PickupDetails(string id)
        {
            var pickup = await _context.Pickups.FindAsync(id);
            if (pickup == null)
            {
                return NotFound();
            }
            return View(pickup);
        }


        [HttpGet]
        public async Task<IActionResult> CreateOrder()
        {
            ViewBag.Stocks = await _context.Stocks.ToListAsync();
            ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(int productId, int quantity, int supplierId, DateTime orderDate)
        {
            if (ModelState.IsValid)
            {
                // Ensure that the supplier exists in the database
                var supplier = await _context.Suppliers.FindAsync(supplierId);
                if (supplier == null)
                {
                    ModelState.AddModelError("SupplierId", "Invalid Supplier selected.");
                    ViewBag.Stocks = await _context.Stocks.ToListAsync();
                    ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
                    return View();
                }

                var newOrder = new WarehouseOrder
                {
                    ProductId = productId,
                    Quantity = quantity,
                    SupplierId = supplierId,
                    OrderDate = orderDate,
                    Status = "Created",
                    Supplier = supplier.Name // Set to supplier's name or appropriate value
                };

                _context.WarehouseOrders.Add(newOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction("OrderList");
            }

            ViewBag.Stocks = await _context.Stocks.ToListAsync();
            ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
            return View();
        }



        // GET: Warehouse/OrderList
        public async Task<IActionResult> OrderList()
        {
            var orders = await _context.WarehouseOrders.ToListAsync();
            return View(orders);
        }


        // GET: Warehouse/DeleteOrder/{id}
        [HttpGet]
        [Route("DeleteOrder/{id}")]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var order = await _context.WarehouseOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Warehouse/EditOrder/{id}
        [HttpGet]
        [Route("EditOrder/{id}")]
        public async Task<IActionResult> EditOrder(int id)
        {
            var order = await _context.WarehouseOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Products = new SelectList(await _context.Stocks.ToListAsync(), "Id", "Name");
            ViewBag.Suppliers = new SelectList(await _context.Suppliers.ToListAsync(), "Id", "Name");

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrder(int id, int productId, int quantity, int supplierId, DateTime orderDate, string status)
        {
            if (ModelState.IsValid)
            {
                var existingOrder = await _context.WarehouseOrders.FindAsync(id);
                if (existingOrder == null)
                {
                    return NotFound();
                }

                existingOrder.ProductId = productId;
                existingOrder.Quantity = quantity;
                existingOrder.SupplierId = supplierId;
                existingOrder.OrderDate = orderDate;
                existingOrder.Status = status;

                _context.WarehouseOrders.Update(existingOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction("OrderList");
            }

            ViewBag.Stocks = await _context.Stocks.ToListAsync();
            ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
            return View();
        }


        // GET: Warehouse/EditStock/5
        public async Task<IActionResult> EditStock(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null)
            {
                return NotFound();
            }
            return View(stock);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStock(int id, [Bind("Id,Name,OriginalQuantity,Quantity,Supplier,ReceivedDate,Description,Price,Category")] Stock stock, IFormFile ImageBase64)
        {
            if (id != stock.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingStock = await _context.Stocks.FindAsync(id);

                    if (existingStock == null)
                    {
                        return NotFound();
                    }

                    existingStock.Name = stock.Name;
                    existingStock.OriginalQuantity = stock.OriginalQuantity;
                    existingStock.Quantity = stock.Quantity;
                    existingStock.Supplier = stock.Supplier;
                    existingStock.ReceivedDate = stock.ReceivedDate;
                    existingStock.Description = stock.Description;
                    existingStock.Price = stock.Price;
                    existingStock.Category = stock.Category;

                    if (ImageBase64 != null && ImageBase64.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await ImageBase64.CopyToAsync(memoryStream);
                            existingStock.ImageBase64 = Convert.ToBase64String(memoryStream.ToArray());
                        }
                    }

                    _context.Update(existingStock);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StockExists(stock.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(StockList));
            }
            return View(stock);
        }




        // POST: Warehouse/RemoveOrder
        [HttpPost]
        [Route("RemoveOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveOrder(int id)
        {
            var order = await _context.WarehouseOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.WarehouseOrders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction("OrderList");
        }

        [HttpGet]
        [Route("DeleteStock/{id}")]
        public async Task<IActionResult> DeleteStock(int id)  // Change to int
        {
            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null)
            {
                return NotFound();
            }

            return View(stock);
        }

        [HttpPost]
        [Route("RemoveStock")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStock(int id)  // Change to int
        {
            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null)
            {
                return NotFound();
            }
            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();
            return RedirectToAction("StockList");
        }


        public IActionResult CreatePickup()
        {
            return View();
        }

        // Action for Pickup List
        public async Task<IActionResult> PickupList(string searchTerm)
        {
            var pickups = _context.StockItems.AsQueryable();

            // If there is a search term, filter the pickups
            if (!string.IsNullOrEmpty(searchTerm))
            {
                pickups = pickups.Where(s => s.ProductName.Contains(searchTerm));
            }

            // Fetch the filtered list asynchronously
            var pickupList = await pickups.ToListAsync();
            return View(pickupList);
        }



        // GET: Warehouse/UpdatePickup
        public ActionResult UpdatePickup(string id)
        {

            return View();
        }

        public ActionResult Dashboard()
        {
            if (HttpContext.Session.TryGetValue("loggedInUser", out var emailBytes))
            {
                var email = System.Text.Encoding.UTF8.GetString(emailBytes);
                var user = _context.WarehouseUsers.FirstOrDefault(u => u.Email == email);

                if (user != null && user.IsAdmin) // Check if the user is an admin
                {
                    ViewBag.UserName = user.Name; // Pass the user name to the view

                    // Assume this is the list of all stock items in the database
                    var stockItems = _context.Stocks.ToList();

                    // Calculate total stock
                    int totalStock = stockItems.Sum(s => s.Quantity);

                    // Set a low stock threshold (e.g., 100 units)
                    int lowStockThreshold = 100;
                    bool isStockLow = totalStock <= lowStockThreshold;

                    // Pass the total stock and low stock warning to the view
                    ViewBag.TotalStock = totalStock;
                    ViewBag.IsStockLow = isStockLow;
                    ViewBag.LowStockThreshold = lowStockThreshold;

                    return View();
                }
            }

            return RedirectToAction("Login", "Warehouse"); // Redirect to login if not authenticated
        }


        [HttpGet]
        public IActionResult Login()
        {
            // Return an empty model to the view
            return View(new WarehouseLoginViewModel());
        }

        [HttpPost]
        public IActionResult Login(WarehouseLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.WarehouseUsers.FirstOrDefault(u => u.Email == model.Email);

                if (user != null)
                {
                    bool passwordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
                    if (passwordValid)
                    {
                        HttpContext.Session.SetString("loggedInUser", user.Email);
                        return RedirectToAction("Dashboard", "Warehouse");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid login attempt.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid login attempt.";
                }
            }
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Register(WarehouseRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.WarehouseUsers.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("", "This email is already registered.");
                    return View(model);
                }

                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("", "Passwords do not match.");
                    return View(model);
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password, 11);

                var warehouseUser = new WarehouseUser
                {
                    Name = model.Name,
                    Email = model.Email,
                    PasswordHash = hashedPassword,
                    IsAdmin = true
                };


                // Save user to database
                _context.WarehouseUsers.Add(warehouseUser);
                    await _context.SaveChangesAsync();

                    // Redirect to login after successful registration
                    return RedirectToAction("Login", "Warehouse");                              
            }
            return View(model);

        }



        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

       

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Warehouse");
        }


        // GET: View Orders
        public IActionResult ViewOrders()
        {
            var orders = _context.WarehouseOrders.Where(o => o.Status == "Created").ToList();
            return View(orders);
        }

        // POST: Process Order
        [HttpPost]
        public IActionResult ProcessOrder(int orderId)
        {
            var order = _context.WarehouseOrders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = "Processed";
                _context.SaveChanges();
            }

            return RedirectToAction("ViewOrders");
        }

        public IActionResult ViewPickupSlip(string id)
        {
            var pickup = _context.Pickups.Find(id);
            if (pickup == null)
            {
                return NotFound();
            }

            var stock = _context.Stocks.Find(pickup.ProductId);
            if (stock == null)
            {
                return NotFound();
            }

            var viewModel = new PickupSlipViewModel
            {
                Pickup = pickup,
                Stock = stock
            };

            return View("PickupSlip", viewModel); // Ensure the view name matches the .cshtml file name
        }

        [HttpPost]
        public ActionResult UpdatePickupStatus(int id, string action, string reason)
        {
            var stockItem = _context.StockItems.SingleOrDefault(s => s.StockItemId == id);
            if (stockItem == null)
            {
                return HttpNotFound();
            }

            if (action == "accept")
            {
                stockItem.Status = "Accepted";
                stockItem.DeclineReason = string.Empty; // Ensure non-null value
            }
            else if (action == "decline")
            {
                if (string.IsNullOrWhiteSpace(reason))
                {
                    ModelState.AddModelError("reason", "Reason for decline is required.");
                    return View("PickupList", _context.StockItems.ToList());
                }
                stockItem.Status = "Declined";
                stockItem.DeclineReason = reason;
            }

            _context.SaveChanges();

            return RedirectToAction("PickupList");
        }


        private ActionResult HttpNotFound()
        {
            throw new NotImplementedException();
        }

       
        // Dummy in-memory data store for demonstration
        private static readonly List<StockItem> _stockItems = new List<StockItem>
        {
            new StockItem { StockItemId = 1, ProductName = "Cleaner", QuantityOrdered = 100, UnitPrice = 19.99m },
            new StockItem { StockItemId = 2, ProductName = "Detergent", QuantityOrdered = 50, UnitPrice = 9.99m }
        };

        // GET: /Warehouse/StockItemList
        public IActionResult StockItemList()
        {
            return View(_stockItems);
        }

        // GET: /Warehouse/StockItem/Details/5
        public IActionResult Details(int id)
        {
            var stockItem = _stockItems.FirstOrDefault(s => s.StockItemId == id);
            if (stockItem == null)
            {
                return NotFound();
            }
            return View(stockItem);
        }

        // GET: /Warehouse/StockItem/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Warehouse/StockItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("StockItemId,ProductName,QuantityOrdered,UnitPrice")] StockItem stockItem)
        {
            if (ModelState.IsValid)
            {
                _stockItems.Add(stockItem);
                return RedirectToAction(nameof(StockItemList));
            }
            return View(stockItem);
        }

        // GET: /Warehouse/StockItem/Edit/5
        public IActionResult Edit(int id)
        {
            var stockItem = _stockItems.FirstOrDefault(s => s.StockItemId == id);
            if (stockItem == null)
            {
                return NotFound();
            }
            return View(stockItem);
        }

        // POST: /Warehouse/StockItem/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("StockItemId,ProductName,QuantityOrdered,UnitPrice")] StockItem stockItem)
        {
            if (id != stockItem.StockItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var item = _stockItems.FirstOrDefault(s => s.StockItemId == id);
                if (item != null)
                {
                    item.ProductName = stockItem.ProductName;
                    item.QuantityOrdered = stockItem.QuantityOrdered;
                    item.UnitPrice = stockItem.UnitPrice;
                }
                return RedirectToAction(nameof(StockItemList));
            }
            return View(stockItem);
        }

        // GET: /Warehouse/StockItem/Delete/5
        public IActionResult Delete(int id)
        {
            var stockItem = _stockItems.FirstOrDefault(s => s.StockItemId == id);
            if (stockItem == null)
            {
                return NotFound();
            }
            return View(stockItem);
        }

        // POST: /Warehouse/StockItem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var stockItem = _stockItems.FirstOrDefault(s => s.StockItemId == id);
            if (stockItem != null)
            {
                _stockItems.Remove(stockItem);
            }
            return RedirectToAction(nameof(StockItemList));
        }


        public IActionResult SearchResults(string searchTerm)
        {
            var model = new SearchViewModel
            {
                StockItemResults = _context.StockItems
                    .Where(s => s.ProductName.Contains(searchTerm))
                    .ToList(),
                StockResults = _context.Stocks
                    .Where(s => s.Name.Contains(searchTerm))
                    .ToList(),
                SupplierResults = _context.Suppliers
                    .Where(s => s.Name.Contains(searchTerm) || s.ContactInfo.Contains(searchTerm))
                    .ToList()
            };

            return View(model);
        }


        // Action for Category Management
        public IActionResult CategoryManagement()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        [HttpPost]
        public IActionResult AddCategory(string newCategory)
        {
            if (!string.IsNullOrWhiteSpace(newCategory))
            {
                // Check if the category already exists by comparing in a case-insensitive manner
                if (!_context.Categories.Any(c => c.Name.ToLower() == newCategory.ToLower()))
                {
                    // If the category does not exist, add it to the database
                    _context.Categories.Add(new Category { Name = newCategory });
                    _context.SaveChanges();
                }
            }

            return RedirectToAction("CategoryManagement");
        }

    }

}
