using BestBrightness.Data;
using BestBrightness.Models;
using BestBrightness.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BestBrightness.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string reportType = "daily", string searchQuery = "")
        {
            var invoices = new List<Invoice>();

            if (reportType == "daily")
            {
                invoices = await _context.Invoices
                    .Include(i => i.Sales)
                    .Where(i => i.InvoiceDate.Date == DateTime.Today)
                    .ToListAsync();
            }
            else if (reportType == "all")
            {
                invoices = await _context.Invoices
                    .Include(i => i.Sales)
                    .ToListAsync();

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    if (int.TryParse(searchQuery, out var invoiceId))
                    {
                        invoices = invoices
                            .Where(i => i.InvoiceId == invoiceId)
                            .ToList();
                    }
                }
            }

            var users = await _context.Users.ToListAsync();

            var dailySalesReport = invoices.Select(i => new DailySalesReportViewModel
            {
                InvoiceId = i.InvoiceId,
                TotalAmount = i.TotalAmount,
                AmountPaid = i.AmountPaid,
                Change = i.Change,
                InvoiceDate = i.InvoiceDate,
                FormattedInvoiceDate = i.InvoiceDate.ToString("yyyy-MM-dd HH:mm:ss"),
                SalespersonName = users.FirstOrDefault(u => u.SalespersonId == i.SalespersonId.ToString())?.FullName ?? "Unknown",
                SalespersonId = i.SalespersonId
            }).ToList();

            var inventoryReport = await _context.Products
                .Select(p => new InventoryReportViewModel
                {
                    ProductName = p.Name,
                    StockLevel = p.StockLevel,
                    NeedsRestocking = p.StockLevel < 10 // Arbitrary restocking threshold
                })
                .ToListAsync();

            var model = new ReportViewModel
            {
                DailySalesReport = dailySalesReport,
                InventoryReport = inventoryReport
            };

            ViewData["ReportType"] = reportType;
            ViewData["SearchQuery"] = searchQuery;

            return View(model);
        }

        public async Task<IActionResult> DownloadReport(string reportType = "daily", string searchQuery = "")
        {
            var invoices = new List<Invoice>();

            if (reportType == "daily")
            {
                invoices = await _context.Invoices
                    .Include(i => i.Sales)
                    .Where(i => i.InvoiceDate.Date == DateTime.Today)
                    .ToListAsync();
            }
            else if (reportType == "all")
            {
                invoices = await _context.Invoices
                    .Include(i => i.Sales)
                    .ToListAsync();

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    invoices = invoices
                        .Where(i => i.InvoiceId.ToString().Contains(searchQuery) ||
                                    i.SalespersonName.Contains(searchQuery) ||
                                    i.TotalAmount.ToString().Contains(searchQuery) ||
                                    i.AmountPaid.ToString().Contains(searchQuery) ||
                                    i.Change.ToString().Contains(searchQuery))
                        .ToList();
                }
            }

            var users = await _context.Users.ToListAsync();

            var dailySalesReport = invoices.Select(i => new DailySalesReportViewModel
            {
                InvoiceId = i.InvoiceId,
                TotalAmount = i.TotalAmount,
                AmountPaid = i.AmountPaid,
                Change = i.Change,
                InvoiceDate = i.InvoiceDate,
                FormattedInvoiceDate = i.InvoiceDate.ToString("yyyy-MM-dd HH:mm:ss"), // Format the date with time
                SalespersonName = users.FirstOrDefault(u => u.SalespersonId == i.SalespersonId.ToString())?.FullName ?? "Unknown",
                SalespersonId = i.SalespersonId
            }).ToList();

            var inventoryReport = await _context.Products
                .Select(p => new InventoryReportViewModel
                {
                    ProductName = p.Name,
                    StockLevel = p.StockLevel,
                    NeedsRestocking = p.StockLevel < 10 // Arbitrary restocking threshold
                })
                .ToListAsync();

            var reportData = new ReportViewModel
            {
                DailySalesReport = dailySalesReport,
                InventoryReport = inventoryReport
            };

            // Generate CSV file
            var csv = GenerateCsv(reportData);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            var output = new MemoryStream(bytes);

            return File(output, "text/csv", "report.csv");
        }

        private string GenerateCsv(ReportViewModel reportData)
        {
            var csv = new StringWriter();

            // Write Daily Sales Report
            csv.WriteLine("Daily Sales Report");
            csv.WriteLine("Invoice ID,Total Amount,Amount Paid,Change,Invoice Date,Salesperson Name,Salesperson ID");

            foreach (var invoice in reportData.DailySalesReport)
            {
                csv.WriteLine($"{invoice.InvoiceId},{invoice.TotalAmount},{invoice.AmountPaid},{invoice.Change},{invoice.FormattedInvoiceDate},{invoice.SalespersonName},{invoice.SalespersonId}");
            }

            csv.WriteLine();

            // Write Inventory Report
            csv.WriteLine("Inventory Report");
            csv.WriteLine("Product,Stock Level,Needs Restocking");

            foreach (var product in reportData.InventoryReport)
            {
                csv.WriteLine($"{product.ProductName},{product.StockLevel},{(product.NeedsRestocking ? "Yes" : "No")}");
            }

            return csv.ToString();
        }
    }
}
