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

            // Configure StoreTransfer relationships
            modelBuilder.Entity<StoreTransfer>()
                .HasOne(st => st.FromStore)
                .WithMany()
                .HasForeignKey(st => st.FromStoreId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StoreTransfer>()
                .HasOne(st => st.ToStore)
                .WithMany()
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
}