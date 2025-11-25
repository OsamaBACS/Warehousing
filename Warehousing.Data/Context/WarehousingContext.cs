using System.Security.Cryptography;
using System.Text;
using Warehousing.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;


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

            // ===========================================
            // SEEDING DATA - Using SeedingService
            // ===========================================
            // All seeding data is now handled by SeedingService
            // This ensures we use the actual data from your database

            // Configure ProductRecipe relationships
            modelBuilder.Entity<ProductRecipe>()
                .HasOne(pr => pr.ParentProduct)
                .WithMany(p => p.RecipeAsParent)
                .HasForeignKey(pr => pr.ParentProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductRecipe>()
                .HasOne(pr => pr.ComponentProduct)
                .WithMany(p => p.RecipeAsComponent)
                .HasForeignKey(pr => pr.ComponentProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure StoreTransfer relationships - explicitly specify navigation properties
            modelBuilder.Entity<StoreTransfer>()
                .HasOne(st => st.FromStore)
                .WithMany(s => s.TransfersFrom)
                .HasForeignKey(st => st.FromStoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StoreTransfer>()
                .HasOne(st => st.ToStore)
                .WithMany(s => s.TransfersTo)
                .HasForeignKey(st => st.ToStoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StoreTransfer>()
                .HasOne(st => st.Status)
                .WithMany()
                .HasForeignKey(st => st.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure StoreTransferItem relationships
            modelBuilder.Entity<StoreTransferItem>()
                .HasOne(sti => sti.Transfer)
                .WithMany(st => st.Items)
                .HasForeignKey(sti => sti.TransferId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StoreTransferItem>()
                .HasOne(sti => sti.Product)
                .WithMany(p => p.TransferItems)
                .HasForeignKey(sti => sti.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product Variants Configuration
            modelBuilder.Entity<ProductVariant>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product Modifiers Configuration
            modelBuilder.Entity<ProductModifierOption>()
                .HasOne(pmo => pmo.Modifier)
                .WithMany(pm => pm.Options)
                .HasForeignKey(pmo => pmo.ModifierId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductModifierGroup>()
                .HasOne(pmg => pmg.Product)
                .WithMany(p => p.ModifierGroups)
                .HasForeignKey(pmg => pmg.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductModifierGroup>()
                .HasOne(pmg => pmg.Modifier)
                .WithMany(pm => pm.Groups)
                .HasForeignKey(pmg => pmg.ModifierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order Item Modifiers Configuration
            modelBuilder.Entity<OrderItemModifier>()
                .HasOne(oim => oim.OrderItem)
                .WithMany(oi => oi.Modifiers)
                .HasForeignKey(oim => oim.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItemModifier>()
                .HasOne(oim => oim.ModifierOption)
                .WithMany(pmo => pmo.OrderItemModifiers)
                .HasForeignKey(oim => oim.ModifierOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order Item Variants Configuration
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Variant)
                .WithMany(pv => pv.OrderItems)
                .HasForeignKey(oi => oi.VariantId)
                .OnDelete(DeleteBehavior.Restrict);

            // User Activity Log Configuration
            modelBuilder.Entity<UserActivityLog>()
                .HasOne(ual => ual.User)
                .WithMany()
                .HasForeignKey(ual => ual.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Working Hours Configuration
            modelBuilder.Entity<WorkingHoursException>()
                .HasOne(whe => whe.WorkingHours)
                .WithMany(wh => wh.Exceptions)
                .HasForeignKey(whe => whe.WorkingHoursId)
                .OnDelete(DeleteBehavior.Cascade);

            // Working Hours Day Configuration
            modelBuilder.Entity<WorkingHoursDay>()
                .HasOne(whd => whd.WorkingHours)
                .WithMany(wh => wh.Days)
                .HasForeignKey(whd => whd.WorkingHoursId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: One WorkingHoursDay per WorkingHours per DayOfWeek
            modelBuilder.Entity<WorkingHoursDay>()
                .HasIndex(whd => new { whd.WorkingHoursId, whd.DayOfWeek })
                .IsUnique();

            // Inventory Variants Configuration
            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Variant)
                .WithMany(pv => pv.Inventories)
                .HasForeignKey(i => i.VariantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraints
            modelBuilder.Entity<ProductVariant>()
                .HasIndex(pv => new { pv.ProductId, pv.Name })
                .IsUnique();

            modelBuilder.Entity<ProductModifierGroup>()
                .HasIndex(pmg => new { pmg.ProductId, pmg.ModifierId })
                .IsUnique();

            // Printer Configuration - Role Relationship
            modelBuilder.Entity<Role>()
                .HasOne(r => r.PrinterConfiguration)
                .WithMany(pc => pc.Roles)
                .HasForeignKey(r => r.PrinterConfigurationId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure decimal precision and scale for all decimal properties
            // Company
            modelBuilder.Entity<Company>()
                .Property(c => c.Capital)
                .HasPrecision(18, 2);

            // Inventory
            modelBuilder.Entity<Inventory>()
                .Property(i => i.Quantity)
                .HasPrecision(18, 3);

            // InventoryTransaction
            modelBuilder.Entity<InventoryTransaction>()
                .Property(it => it.QuantityAfter)
                .HasPrecision(18, 3);
            modelBuilder.Entity<InventoryTransaction>()
                .Property(it => it.QuantityBefore)
                .HasPrecision(18, 3);
            modelBuilder.Entity<InventoryTransaction>()
                .Property(it => it.QuantityChanged)
                .HasPrecision(18, 3);
            modelBuilder.Entity<InventoryTransaction>()
                .Property(it => it.UnitCost)
                .HasPrecision(18, 2);

            // Order
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            // OrderItem
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Discount)
                .HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Quantity)
                .HasPrecision(18, 3);
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitCost)
                .HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            // OrderItemModifier
            modelBuilder.Entity<OrderItemModifier>()
                .Property(oim => oim.CostAdjustment)
                .HasPrecision(18, 2);
            modelBuilder.Entity<OrderItemModifier>()
                .Property(oim => oim.PriceAdjustment)
                .HasPrecision(18, 2);

            // Product
            modelBuilder.Entity<Product>()
                .Property(p => p.CostPrice)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Product>()
                .Property(p => p.ReorderLevel)
                .HasPrecision(18, 3);
            modelBuilder.Entity<Product>()
                .Property(p => p.SellingPrice)
                .HasPrecision(18, 2);

            // ProductModifier
            modelBuilder.Entity<ProductModifier>()
                .Property(pm => pm.CostAdjustment)
                .HasPrecision(18, 2);
            modelBuilder.Entity<ProductModifier>()
                .Property(pm => pm.PriceAdjustment)
                .HasPrecision(18, 2);

            // ProductModifierOption
            modelBuilder.Entity<ProductModifierOption>()
                .Property(pmo => pmo.CostAdjustment)
                .HasPrecision(18, 2);
            modelBuilder.Entity<ProductModifierOption>()
                .Property(pmo => pmo.PriceAdjustment)
                .HasPrecision(18, 2);

            // ProductRecipe
            modelBuilder.Entity<ProductRecipe>()
                .Property(pr => pr.Quantity)
                .HasPrecision(18, 3);

            // ProductVariant
            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.CostAdjustment)
                .HasPrecision(18, 2);
            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.PriceAdjustment)
                .HasPrecision(18, 2);
            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.ReorderLevel)
                .HasPrecision(18, 3);

            // StoreTransferItem
            modelBuilder.Entity<StoreTransferItem>()
                .Property(sti => sti.Quantity)
                .HasPrecision(18, 3);
            modelBuilder.Entity<StoreTransferItem>()
                .Property(sti => sti.UnitCost)
                .HasPrecision(18, 2);
        }


        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductRecipe> ProductRecipes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderType> OrderTypes { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<StoreTransfer> StoreTransfers { get; set; }
        public DbSet<StoreTransferItem> StoreTransferItems { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<RoleCategory> RoleCategories { get; set; }
        public DbSet<RoleProduct> RoleProducts { get; set; }
        public DbSet<RoleSubCategory> RoleSubCategories { get; set; }
        
        // Variants and Modifiers
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductModifier> ProductModifiers { get; set; }
        public DbSet<ProductModifierOption> ProductModifierOptions { get; set; }
        public DbSet<ProductModifierGroup> ProductModifierGroups { get; set; }
        public DbSet<OrderItemModifier> OrderItemModifiers { get; set; }
        
        // User Activity Logging
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }
        
        // Working Hours Configuration
        public DbSet<WorkingHours> WorkingHours { get; set; }
        public DbSet<WorkingHoursException> WorkingHoursExceptions { get; set; }
        public DbSet<WorkingHoursDay> WorkingHoursDays { get; set; }
        
        // Printer Configuration
        public DbSet<PrinterConfiguration> PrinterConfigurations { get; set; }


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

            foreach (var entry in ChangeTracker.Entries<BaseClass>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = userNameClaim;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    Console.WriteLine($"CreatedBy: {entry.Entity.CreatedBy}, CreatedAt: {entry.Entity.CreatedAt}");
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedBy = userNameClaim;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    Console.WriteLine($"UpdatedBy: {entry.Entity.UpdatedBy}, UpdatedAt: {entry.Entity.UpdatedAt}");
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }

    public class WarehousingContextFactory : IDesignTimeDbContextFactory<WarehousingContext>
    {
        public WarehousingContext CreateDbContext(string[] args)
        {
            var solutionDir = Directory.GetCurrentDirectory();
            // Attempt to locate the API project's appsettings for connection strings
            var apiProjectPath = Path.Combine(solutionDir, "..", "Warehousing.Api");
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true);

            var config = configBuilder.Build();

            var builder = new DbContextOptionsBuilder<WarehousingContext>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            builder.UseSqlServer(connectionString);
            return new WarehousingContext(builder.Options, null!);
        }
    }
}