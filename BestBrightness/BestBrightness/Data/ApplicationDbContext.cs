using BestBrightness.Models;
using BestBrightness.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BestBrightness.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Users> Users { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<InventoryItem> InventoryItem { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<CustomerRegister> CustomerRegisters { get; set; }
        public DbSet<CustomerLogin> CustomerLogins { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItems> CartItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationRequest> QuotationRequests { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<StockItem> StockItems { get; set; }
        public DbSet<DamagedProducts> DamagedProducts { get; set; }
        public DbSet<WarehouseUser> WarehouseUsers { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<WarehouseOrder> WarehouseOrders { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Pickup> Pickups { get; set; }
        public DbSet<WarehouseProduct> WarehouseProducts { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<InvoiceViewModel> InvoiceViewModels { get; set; }

    }
}
