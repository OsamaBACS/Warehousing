using System.Security.Cryptography;
using System.Text;
using Warehousing.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;


namespace Warehousing.Data.Context
{
    public class WarehousingContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public WarehousingContext(DbContextOptions<WarehousingContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed data here
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    NameEn = "Cement & Concrete",
                    NameAr = "إسمنت وخرسانة",
                    Description = "Bags of cement, concrete mixes"
                },
                new Category
                {
                    Id = 2,
                    NameEn = "Steel & Rebar",
                    NameAr = "حديد وتسليح",
                    Description = "Steel bars, mesh, structural steel"
                },
                new Category
                {
                    Id = 3,
                    NameEn = "Bricks & Blocks",
                    NameAr = "طوب وبلوك",
                    Description = "Clay bricks, concrete blocks"
                },
                new Category
                {
                    Id = 4,
                    NameEn = "Plumbing & Pipes",
                    NameAr = "السباكة والأنابيب",
                    Description = "PVC pipes, copper pipes, fittings"
                },
                new Category
                {
                    Id = 5,
                    NameEn = "Electrical Materials",
                    NameAr = "مواد كهربائية",
                    Description = "Wires, switches, circuit breakers"
                },
                new Category
                {
                    Id = 6,
                    NameEn = "Paint & Coatings",
                    NameAr = "دهانات وألوان",
                    Description = "Interior/exterior paints, primers"
                },
                new Category
                {
                    Id = 7,
                    NameEn = "Sanitary Ware",
                    NameAr = "أدوات صحية",
                    Description = "Toilets, sinks, taps, showers"
                },
                new Category
                {
                    Id = 8,
                    NameEn = "Tiles & Flooring",
                    NameAr = "بلاط وأرضيات",
                    Description = "Ceramic tiles, marble, vinyl flooring"
                },
                new Category
                {
                    Id = 9,
                    NameEn = "Doors & Windows",
                    NameAr = "أبواب ونوافذ",
                    Description = "Wooden doors, aluminum windows"
                },
                new Category
                {
                    Id = 10,
                    NameEn = "Tools & Equipment",
                    NameAr = "أدوات ومعدات",
                    Description = "Hand tools, power tools, construction machines"
                }
            );

            modelBuilder.Entity<SubCategory>().HasData(
                // CEMENT
                new SubCategory { Id = 1, NameEn = "White Cement", NameAr = "أسمنت أبيض", Description = "Used for decorative works", CategoryId = 1 },
                new SubCategory { Id = 2, NameEn = "Portland Cement", NameAr = "أسمنت بورتلاندي", Description = "General construction use", CategoryId = 1 },

                // STEEL
                new SubCategory { Id = 3, NameEn = "Rebar", NameAr = "حديد تسليح", Description = "Reinforcing steel bars", CategoryId = 2 },
                new SubCategory { Id = 4, NameEn = "Steel Mesh", NameAr = "شبك فولاذي", Description = "For concrete reinforcement", CategoryId = 2 },

                // BRICKS
                new SubCategory { Id = 5, NameEn = "Clay Bricks", NameAr = "طوب طيني", Description = "Traditional building material", CategoryId = 3 },
                new SubCategory { Id = 6, NameEn = "Concrete Blocks", NameAr = "بلوك خرساني", Description = "Hollow or solid blocks", CategoryId = 3 },

                // PLUMBING
                new SubCategory { Id = 7, NameEn = "PVC Pipes", NameAr = "أنابيب PVC", Description = "For drainage and water systems", CategoryId = 4 },
                new SubCategory { Id = 8, NameEn = "Copper Pipes", NameAr = "أنابيب نحاسية", Description = "Used for water supply", CategoryId = 4 },

                // ELECTRICAL
                new SubCategory { Id = 9, NameEn = "Electrical Wires", NameAr = "أسلاك كهربائية", Description = "Insulated copper wires", CategoryId = 5 },
                new SubCategory { Id = 10, NameEn = "Switches & Sockets", NameAr = "مفاتيح ومقابس", Description = "Power control devices", CategoryId = 5 },

                // PAINT
                new SubCategory { Id = 11, NameEn = "Interior Paint", NameAr = "دهان داخلي", Description = "For indoor walls", CategoryId = 6 },
                new SubCategory { Id = 12, NameEn = "Exterior Paint", NameAr = "دهان خارجي", Description = "Weather-resistant paint", CategoryId = 6 },

                // SANITARY
                new SubCategory { Id = 13, NameEn = "Toilets", NameAr = "مراحيض", Description = "Standard and smart toilets", CategoryId = 7 },
                new SubCategory { Id = 14, NameEn = "Bathroom Faucets", NameAr = "حنفيات حمام", Description = "Taps and mixers", CategoryId = 7 },

                // TILES
                new SubCategory { Id = 15, NameEn = "Ceramic Tiles", NameAr = "بلاط سيراميك", Description = "Wall and floor tiles", CategoryId = 8 },
                new SubCategory { Id = 16, NameEn = "Marble Flooring", NameAr = "أرضيات رخام", Description = "Luxury floor option", CategoryId = 8 },

                // DOORS
                new SubCategory { Id = 17, NameEn = "Wooden Doors", NameAr = "أبواب خشبية", Description = "Interior and exterior use", CategoryId = 9 },
                new SubCategory { Id = 18, NameEn = "Aluminum Windows", NameAr = "نوافذ ألمنيوم", Description = "Durable and modern", CategoryId = 9 },

                // TOOLS
                new SubCategory { Id = 19, NameEn = "Hand Tools", NameAr = "أدوات يدوية", Description = "Saws, hammers, screwdrivers", CategoryId = 10 },
                new SubCategory { Id = 20, NameEn = "Power Tools", NameAr = "أدوات كهربائية", Description = "Drills, grinders, etc.", CategoryId = 10 }
            );

            // Seed data for Statuses
            modelBuilder.Entity<Status>().HasData(
                new Status
                {
                    Id = 1,
                    Code = "PENDING",
                    NameEn = "Pending",
                    NameAr = "قيد الانتظار",
                    Description = "Order is created but not processed yet"
                },
                new Status
                {
                    Id = 2,
                    Code = "PROCESSING",
                    NameEn = "Processing",
                    NameAr = "جاري المعالجة",
                    Description = "Order is being prepared or reviewed"
                },
                new Status
                {
                    Id = 3,
                    Code = "CONFIRMED",
                    NameEn = "Confirmed",
                    NameAr = "مؤكد",
                    Description = "Order has been confirmed by the supplier/customer"
                },
                new Status
                {
                    Id = 4,
                    Code = "SHIPPED",
                    NameEn = "Shipped",
                    NameAr = "تم الشحن",
                    Description = "Goods have been dispatched"
                },
                new Status
                {
                    Id = 5,
                    Code = "DELIVERED",
                    NameEn = "Delivered",
                    NameAr = "تم التسليم",
                    Description = "Goods have been successfully delivered"
                },
                new Status
                {
                    Id = 6,
                    Code = "CANCELLED",
                    NameEn = "Cancelled",
                    NameAr = "تم الإلغاء",
                    Description = "Order was cancelled"
                },
                new Status
                {
                    Id = 7,
                    Code = "RETURNED",
                    NameEn = "Returned",
                    NameAr = "تم الإرجاع",
                    Description = "Goods were returned after delivery"
                },
                new Status
                {
                    Id = 8,
                    Code = "COMPLETED",
                    NameEn = "Completed",
                    NameAr = "مكتمل",
                    Description = "Order completed successfully"
                },
                new Status
                {
                    Id = 9,
                    Code = "ONHOLD",
                    NameEn = "On Hold",
                    NameAr = "معلق",
                    Description = "Order temporarily paused"
                },
                new Status
                {
                    Id = 10,
                    Code = "FAILED",
                    NameEn = "Failed",
                    NameAr = "فشل",
                    Description = "Order failed due to payment or stock issue"
                }
            );

            // Seed data for TransactionTypes
            modelBuilder.Entity<TransactionType>().HasData(
                new TransactionType
                {
                    Id = 1,
                    Code = "PURCHASE",
                    NameEn = "Purchase Receipt",
                    NameAr = "استلام مشتريات",
                    Description = "Product received from a supplier"
                },
                new TransactionType
                {
                    Id = 2,
                    Code = "SALE",
                    NameEn = "Sale Dispatch",
                    NameAr = "صرف مبيعات",
                    Description = "Product dispatched to a customer"
                },
                new TransactionType
                {
                    Id = 3,
                    Code = "RETURN_IN",
                    NameEn = "Supplier Return",
                    NameAr = "إرجاع للمورد",
                    Description = "Product returned to the supplier"
                },
                new TransactionType
                {
                    Id = 4,
                    Code = "RETURN_OUT",
                    NameEn = "Customer Return",
                    NameAr = "إرجاع من العميل",
                    Description = "Product returned by a customer"
                },
                new TransactionType
                {
                    Id = 5,
                    Code = "ADJUSTMENT_PLUS",
                    NameEn = "Inventory Adjustment (+)",
                    NameAr = "تعديل مخزون (+)",
                    Description = "Increase stock manually (e.g., audit correction)"
                },
                new TransactionType
                {
                    Id = 6,
                    Code = "ADJUSTMENT_MINUS",
                    NameEn = "Inventory Adjustment (-)",
                    NameAr = "تعديل مخزون (-)",
                    Description = "Decrease stock manually"
                },
                new TransactionType
                {
                    Id = 7,
                    Code = "TRANSFER_IN",
                    NameEn = "Transfer In",
                    NameAr = "تحويل وارد",
                    Description = "Stock transferred into this warehouse/location"
                },
                new TransactionType
                {
                    Id = 8,
                    Code = "TRANSFER_OUT",
                    NameEn = "Transfer Out",
                    NameAr = "تحويل صادر",
                    Description = "Stock transferred out of this warehouse/location"
                },
                new TransactionType
                {
                    Id = 9,
                    Code = "DAMAGE_LOSS",
                    NameEn = "Damage / Loss",
                    NameAr = "تالف أو ضائع",
                    Description = "Stock marked as damaged or lost"
                },
                new TransactionType
                {
                    Id = 10,
                    Code = "SAMPLE",
                    NameEn = "Sample Issue",
                    NameAr = "إصدار نموذج",
                    Description = "Issued for sample or demo purposes"
                }
            );

            // Seed data for Units
            modelBuilder.Entity<Unit>().HasData(
                new Unit
                {
                    Id = 1,
                    Code = "EA",
                    NameEn = "Each",
                    NameAr = "لكل وحدة",
                    Description = "Standard unit for items"
                },
                new Unit
                {
                    Id = 2,
                    Code = "KG",
                    NameEn = "Kilogram",
                    NameAr = "كيلوغرام",
                    Description = "Used for weight-based goods"
                },
                new Unit
                {
                    Id = 3,
                    Code = "GM",
                    NameEn = "Gram",
                    NameAr = "غرام",
                    Description = "Small weight measurements"
                },
                new Unit
                {
                    Id = 4,
                    Code = "LT",
                    NameEn = "Liter",
                    NameAr = "لتر",
                    Description = "Volume measurement"
                },
                new Unit
                {
                    Id = 5,
                    Code = "ML",
                    NameEn = "Milliliter",
                    NameAr = "ملليلتر",
                    Description = "Small volume measurements"
                },
                new Unit
                {
                    Id = 6,
                    Code = "MTR",
                    NameEn = "Meter",
                    NameAr = "متر",
                    Description = "Length measurement"
                },
                new Unit
                {
                    Id = 7,
                    Code = "CM",
                    NameEn = "Centimeter",
                    NameAr = "سنتيمتر",
                    Description = "Smaller length unit"
                },
                new Unit
                {
                    Id = 8,
                    Code = "INCH",
                    NameEn = "Inch",
                    NameAr = "إنش",
                    Description = "Imperial length unit"
                },
                new Unit
                {
                    Id = 9,
                    Code = "PKT",
                    NameEn = "Packet",
                    NameAr = "عبوة",
                    Description = "Packaged quantity"
                },
                new Unit
                {
                    Id = 10,
                    Code = "BOX",
                    NameEn = "Box",
                    NameAr = "صندوق",
                    Description = "Bulk packaging unit"
                }
            );

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, NameEn = "Admin", NameAr = "مدير" },
                new Role { Id = 2, NameEn = "WarehouseManager", NameAr = "مسؤول المستودع" },
                new Role { Id = 3, NameEn = "User", NameAr = "مستخدم" }
            );

            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, Code = "VIEW_PRODUCTS", NameEn = "View Products", NameAr = "عرض المنتجات" },
                new Permission { Id = 2, Code = "EDIT_PRODUCTS", NameEn = "Edit Products", NameAr = "تعديل المنتجات" },
                new Permission { Id = 3, Code = "VIEW_TRANSACTIONS", NameEn = "View Transactions", NameAr = "عرض العمليات" },
                new Permission { Id = 4, Code = "CREATE_TRANSACTIONS", NameEn = "Create Transactions", NameAr = "إنشاء العمليات" },
                new Permission { Id = 5, Code = "MANAGE_USERS", NameEn = "Manage Users", NameAr = "إدارة المستخدمين" }
            );

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                NameEn = "Admin",
                NameAr = "مشرف",
                Username = "admin",
                PasswordHash = HashPassword("Admin@123"), // hash of Admin@123
                Phone = "0000000000",
                Email = "admin@example.com",
                Address = "Admin Address",
                CreatedAt = new DateTime(2024, 1, 1),
                CreatedBy = "system"
            });

            modelBuilder.Entity<UserRole>().HasData(new UserRole
            {
                Id = 1,
                UserId = 1,
                RoleId = 1
            });

            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { Id = 1, RoleId = 1, PermissionId = 1 },
                new RolePermission { Id = 2, RoleId = 1, PermissionId = 2 },
                new RolePermission { Id = 3, RoleId = 1, PermissionId = 3 },
                new RolePermission { Id = 4, RoleId = 1, PermissionId = 4 },
                new RolePermission { Id = 5, RoleId = 1, PermissionId = 5 }
            );
        }


        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderType> OrderTypes { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }
        public DbSet<RoleCategory> RoleCategories { get; set; }
        public DbSet<RoleProduct> RoleProducts { get; set; }


        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var userNameClaim = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            foreach (var entry in ChangeTracker.Entries<BaseClass>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.UserId = Convert.ToInt32(userIdClaim);
                    entry.Entity.CreatedBy = userNameClaim;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    Console.WriteLine($"CreatedBy: {entry.Entity.CreatedBy}, CreatedAt: {entry.Entity.CreatedAt}");
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedBy = userIdClaim;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    Console.WriteLine($"UpdatedBy: {entry.Entity.UpdatedBy}, UpdatedAt: {entry.Entity.UpdatedAt}");
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}