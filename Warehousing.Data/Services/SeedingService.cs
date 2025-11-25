using Microsoft.EntityFrameworkCore;
using Warehousing.Data.Context;
using Warehousing.Data.Entities;

namespace Warehousing.Data.Services
{
    public class SeedingService
    {
        private readonly WarehousingContext _context;

        public SeedingService(WarehousingContext context)
        {
            _context = context;
        }

        public async Task SeedDataAsync()
        {
            try
            {
                Console.WriteLine("Starting database seeding...");
                
                // Always seed permissions first (they may be missing even if roles exist)
                await SeedPermissionsAsync();
                
                // Seed lookup tables (always check and seed if missing)
                await SeedStatusesAsync();
                await SeedOrderTypesAsync();
                await SeedTransactionTypesAsync();
                await SeedUnitsAsync();
                await SeedStoresAsync();
                await SeedPrinterConfigurationsAsync();
                
                // Always seed WorkingHours and Company (they may be missing)
                await SeedWorkingHoursAsync();
                await SeedCompanyAsync();
                
                // Check if roles already exist
                if (await _context.Roles.AnyAsync())
                {
                    Console.WriteLine("Roles already exist. Ensuring admin role has all permissions...");
                    // If roles exist, ensure admin role has all permissions and user roles
                    await EnsureAdminRoleHasAllPermissionsAsync();
                    await SeedUserRolesAsync(); // Ensure admin user has admin role
                    Console.WriteLine("Database seeding completed (roles already existed).");
                    return; // Data already seeded
                }

                // Seed roles, users, and relationships (only if roles don't exist)
                Console.WriteLine("Seeding roles, users, and relationships...");
                await SeedRolesAsync();
                await SeedUsersAsync();
                await SeedRolePermissionsAsync();
                await SeedUserRolesAsync();
                
                Console.WriteLine("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during database seeding: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to see the error in application logs
            }
        }

        private async Task SeedPermissionsAsync()
        {
            // Get existing permissions
            var existingPermissions = await _context.Permissions.ToListAsync();
            var existingCodes = existingPermissions.Select(p => p.Code).ToHashSet();

            // Add all permissions from the database tables (without explicit IDs - let database auto-generate)
            var permissions = new List<Permission>
            {
                // Product Management
                new Permission { Code = "VIEW_PRODUCTS", NameEn = "View Products", NameAr = "عرض المنتجات" },
                new Permission { Code = "ADD_PRODUCT", NameEn = "Add Product", NameAr = "إضافة منتج" },
                new Permission { Code = "EDIT_PRODUCT", NameEn = "Edit Product", NameAr = "تعديل منتج" },
                new Permission { Code = "DELETE_PRODUCT", NameEn = "Delete Product", NameAr = "حذف منتج" },
                new Permission { Code = "PRINT_PRODUCTS", NameEn = "Print Products", NameAr = "طباعة المنتجات" },
                
                // User Management
                new Permission { Code = "VIEW_USERS", NameEn = "View Users", NameAr = "عرض المستخدمين" },
                new Permission { Code = "ADD_USER", NameEn = "Add User", NameAr = "إضافة مستخدم" },
                new Permission { Code = "EDIT_USER", NameEn = "Edit User", NameAr = "تعديل مستخدم" },
                new Permission { Code = "DELETE_USER", NameEn = "Delete User", NameAr = "حذف مستخدم" },
                new Permission { Code = "RESET_USER_PASSWORD", NameEn = "Reset User Password", NameAr = "إعادة تعيين كلمة مرور المستخدم" },
                new Permission { Code = "ASSIGN_ROLES_TO_USER", NameEn = "Assign Roles to User", NameAr = "تعيين أدوار للمستخدم" },
                
                // Role Management
                new Permission { Code = "VIEW_ROLES", NameEn = "View Roles", NameAr = "عرض الأدوار" },
                new Permission { Code = "ADD_ROLE", NameEn = "Add Role", NameAr = "إضافة دور" },
                new Permission { Code = "EDIT_ROLE", NameEn = "Edit Role", NameAr = "تعديل دور" },
                new Permission { Code = "DELETE_ROLE", NameEn = "Delete Role", NameAr = "حذف دور" },
                new Permission { Code = "ASSIGN_PERMISSIONS_TO_ROLE", NameEn = "Assign Permissions to Role", NameAr = "تعيين صلاحيات للدور" },
                
                // Customer Management
                new Permission { Code = "VIEW_CUSTOMERS", NameEn = "View Customers", NameAr = "عرض العملاء" },
                new Permission { Code = "ADD_CUSTOMER", NameEn = "Add Customer", NameAr = "إضافة عميل" },
                new Permission { Code = "EDIT_CUSTOMER", NameEn = "Edit Customer", NameAr = "تعديل عميل" },
                new Permission { Code = "DELETE_CUSTOMER", NameEn = "Delete Customer", NameAr = "حذف عميل" },
                new Permission { Code = "PRINT_CUSTOMERS", NameEn = "Print Customers", NameAr = "طباعة العملاء" },
                
                // Supplier Management
                new Permission { Code = "VIEW_SUPPLIERS", NameEn = "View Suppliers", NameAr = "عرض الموردين" },
                new Permission { Code = "ADD_SUPPLIER", NameEn = "Add Supplier", NameAr = "إضافة مورد" },
                new Permission { Code = "EDIT_SUPPLIER", NameEn = "Edit Supplier", NameAr = "تعديل مورد" },
                new Permission { Code = "DELETE_SUPPLIER", NameEn = "Delete Supplier", NameAr = "حذف مورد" },
                new Permission { Code = "PRINT_SUPPLIERS", NameEn = "Print Suppliers", NameAr = "طباعة الموردين" },
                
                // Purchase Orders
                new Permission { Code = "VIEW_PURCHASE_ORDERS", NameEn = "View Purchase Orders", NameAr = "عرض أوامر الشراء" },
                new Permission { Code = "ADD_PURCHASE_ORDER", NameEn = "Add Purchase Order", NameAr = "إضافة أمر شراء" },
                new Permission { Code = "EDIT_PURCHASE_ORDER", NameEn = "Edit Purchase Order", NameAr = "تعديل أمر شراء" },
                new Permission { Code = "DELETE_PURCHASE_ORDER", NameEn = "Delete Purchase Order", NameAr = "حذف أمر شراء" },
                new Permission { Code = "COMPLETE_PURCHASE_ORDER", NameEn = "Complete Purchase Order", NameAr = "إنهاء أمر شراء" },
                new Permission { Code = "PRINT_PURCHASE_ORDER", NameEn = "Print Purchase Order", NameAr = "طباعة أمر شراء" },
                new Permission { Code = "APPROVE_PURCHASE_ORDER", NameEn = "Approve Purchase Order", NameAr = "اعتماد أمر شراء" },
                new Permission { Code = "CANCEL_PURCHASE_ORDER", NameEn = "Cancel Purchase Order", NameAr = "إلغاء أمر شراء" },
                
                // Sale Orders
                new Permission { Code = "VIEW_SALE_ORDERS", NameEn = "View Sale Orders", NameAr = "عرض أوامر البيع" },
                new Permission { Code = "ADD_SALE_ORDER", NameEn = "Add Sale Order", NameAr = "إضافة أمر بيع" },
                new Permission { Code = "EDIT_SALE_ORDER", NameEn = "Edit Sale Order", NameAr = "تعديل أمر بيع" },
                new Permission { Code = "DELETE_SALE_ORDER", NameEn = "Delete Sale Order", NameAr = "حذف أمر بيع" },
                new Permission { Code = "COMPLETE_SALE_ORDER", NameEn = "Complete Sale Order", NameAr = "إنهاء أمر بيع" },
                new Permission { Code = "PRINT_SALE_ORDER", NameEn = "Print Sale Order", NameAr = "طباعة أمر بيع" },
                new Permission { Code = "APPROVE_SALE_ORDER", NameEn = "Approve Sale Order", NameAr = "اعتماد أمر بيع" },
                new Permission { Code = "CANCEL_SALE_ORDER", NameEn = "Cancel Sale Order", NameAr = "إلغاء أمر بيع" },
                
                // Reports
                new Permission { Code = "VIEW_INVENTORY_REPORT", NameEn = "View Inventory Report", NameAr = "عرض تقرير المخزون" },
                new Permission { Code = "PRINT_INVENTORY_REPORT", NameEn = "Print Inventory Report", NameAr = "طباعة تقرير المخزون" },
                
                // Settings
                new Permission { Code = "VIEW_SETTINGS", NameEn = "View Settings", NameAr = "عرض الإعدادات" },
                new Permission { Code = "EDIT_SETTINGS", NameEn = "Edit Settings", NameAr = "تعديل الإعدادات" },
                
                // Admin Panel
                new Permission { Code = "VIEW_ADMIN", NameEn = "View Admin", NameAr = "الإدارة" },
                
                // Category Management
                new Permission { Code = "VIEW_CATEGORIES", NameEn = "View Categories", NameAr = "عرض التصنيفات" },
                new Permission { Code = "ADD_CATEGORY", NameEn = "Add Category", NameAr = "إضافة تصنيف" },
                new Permission { Code = "EDIT_CATEGORY", NameEn = "Edit Category", NameAr = "تعديل تصنيف" },
                new Permission { Code = "DELETE_CATEGORY", NameEn = "Delete Category", NameAr = "حذف تصنيف" },
                new Permission { Code = "PRINT_CATEGORIES", NameEn = "Print Categories", NameAr = "طباعة التصنيفات" },
                
                // Unit Management
                new Permission { Code = "VIEW_UNITS", NameEn = "View Units", NameAr = "عرض الوحدات" },
                new Permission { Code = "ADD_UNIT", NameEn = "Add Unit", NameAr = "إضافة وحدة" },
                new Permission { Code = "EDIT_UNIT", NameEn = "Edit Unit", NameAr = "تعديل وحدة" },
                new Permission { Code = "DELETE_UNIT", NameEn = "Delete Unit", NameAr = "حذف وحدة" },
                new Permission { Code = "PRINT_UNITS", NameEn = "Print Units", NameAr = "طباعة الوحدات" },
                
                // Store Management
                new Permission { Code = "VIEW_STORES", NameEn = "View Stores", NameAr = "عرض المستودعات" },
                new Permission { Code = "ADD_STORE", NameEn = "Add Store", NameAr = "إضافة مستودع" },
                new Permission { Code = "EDIT_STORE", NameEn = "Edit Store", NameAr = "تعديل مستودع" },
                new Permission { Code = "DELETE_STORE", NameEn = "Delete Store", NameAr = "حذف مستودع" },
                new Permission { Code = "PRINT_STORES", NameEn = "Print Stores", NameAr = "طباعة المستودعات" },
                
                // Store Transfers
                new Permission { Code = "VIEW_STORE_TRANSFERS", NameEn = "View Store Transfers", NameAr = "عرض تحويلات المخزون" },
                new Permission { Code = "ADD_STORE_TRANSFER", NameEn = "Add Store Transfer", NameAr = "إضافة تحويل مخزون" },
                new Permission { Code = "EDIT_STORE_TRANSFER", NameEn = "Edit Store Transfer", NameAr = "تعديل تحويل مخزون" },
                new Permission { Code = "DELETE_STORE_TRANSFER", NameEn = "Delete Store Transfer", NameAr = "حذف تحويل مخزون" },
                new Permission { Code = "APPROVE_STORE_TRANSFER", NameEn = "Approve Store Transfer", NameAr = "اعتماد تحويل مخزون" },
                new Permission { Code = "PRINT_STORE_TRANSFERS", NameEn = "Print Store Transfers", NameAr = "طباعة تحويلات المخزون" },
                
                // Inventory Management
                new Permission { Code = "VIEW_INVENTORY_MANAGEMENT", NameEn = "View Inventory Management", NameAr = "عرض إدارة المخزون" },
                new Permission { Code = "MANAGE_INVENTORY", NameEn = "Manage Inventory", NameAr = "إدارة المخزون" },
                new Permission { Code = "ADJUST_INVENTORY", NameEn = "Adjust Inventory", NameAr = "تعديل المخزون" },
                new Permission { Code = "VIEW_LOW_STOCK", NameEn = "View Low Stock", NameAr = "عرض المخزون المنخفض" },
                
                // Subcategory Management
                new Permission { Code = "VIEW_SUBCATEGORIES", NameEn = "View Subcategories", NameAr = "عرض التصنيفات الفرعية" },
                new Permission { Code = "ADD_SUBCATEGORY", NameEn = "Add Subcategory", NameAr = "إضافة تصنيف فرعي" },
                new Permission { Code = "EDIT_SUBCATEGORY", NameEn = "Edit Subcategory", NameAr = "تعديل تصنيف فرعي" },
                new Permission { Code = "DELETE_SUBCATEGORY", NameEn = "Delete Subcategory", NameAr = "حذف تصنيف فرعي" },
                new Permission { Code = "PRINT_SUBCATEGORIES", NameEn = "Print Subcategories", NameAr = "طباعة التصنيفات الفرعية" },
                
                // Additional permissions
                new Permission { Code = "EDIT_APPROVED_INVOICE", NameEn = "Edit Approved Invoice", NameAr = "تعديل فاتورة معتمدة" },
                
                // Activity Logs Management
                new Permission { Code = "VIEW_ACTIVITY_LOGS", NameEn = "View Activity Logs", NameAr = "عرض سجل الأنشطة" },
                new Permission { Code = "EXPORT_ACTIVITY_LOGS", NameEn = "Export Activity Logs", NameAr = "تصدير سجل الأنشطة" },
                
                // Working Hours Management
                new Permission { Code = "VIEW_WORKING_HOURS", NameEn = "View Working Hours", NameAr = "عرض ساعات العمل" },
                new Permission { Code = "EDIT_WORKING_HOURS", NameEn = "Edit Working Hours", NameAr = "تعديل ساعات العمل" },
                new Permission { Code = "MANAGE_WORKING_HOURS_EXCEPTIONS", NameEn = "Manage Working Hours Exceptions", NameAr = "إدارة استثناءات ساعات العمل" },
                new Permission { Code = "WORK_OUTSIDE_WORKING_HOURS", NameEn = "Work Outside Working Hours", NameAr = "العمل خارج ساعات العمل" },
                
                // Printer Configuration Management
                new Permission { Code = "VIEW_PRINTER_CONFIGURATIONS", NameEn = "View Printer Configurations", NameAr = "عرض إعدادات الطابعة" },
                new Permission { Code = "ADD_PRINTER_CONFIGURATION", NameEn = "Add Printer Configuration", NameAr = "إضافة إعدادات طابعة" },
                new Permission { Code = "EDIT_PRINTER_CONFIGURATION", NameEn = "Edit Printer Configuration", NameAr = "تعديل إعدادات طابعة" },
                new Permission { Code = "DELETE_PRINTER_CONFIGURATION", NameEn = "Delete Printer Configuration", NameAr = "حذف إعدادات طابعة" }
            };

            // Only add permissions that don't already exist
            var permissionsToAdd = permissions.Where(p => !existingCodes.Contains(p.Code)).ToList();
            
            if (permissionsToAdd.Any())
            {
                _context.Permissions.AddRange(permissionsToAdd);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Seeded {permissionsToAdd.Count} new permissions.");
            }
            else
            {
                Console.WriteLine("All permissions already exist in database.");
            }
        }

        private async Task SeedRolesAsync()
        {
            Console.WriteLine("Seeding Roles...");
            var roles = new List<Role>
            {
                new Role { Code = "ADMIN", NameEn = "Admin", NameAr = "مدير", IsActive = true },
                new Role { Code = "WAREHOUSE_MANAGER", NameEn = "Warehouse Manager", NameAr = "مسؤول المستودع", IsActive = true },
                new Role { Code = "SALES_MANAGER", NameEn = "Sales Manager", NameAr = "مدير المبيعات", IsActive = true },
                new Role { Code = "PURCHASE_MANAGER", NameEn = "Purchase Manager", NameAr = "مدير المشتريات", IsActive = true },
                new Role { Code = "USER", NameEn = "User", NameAr = "مستخدم", IsActive = true }
            };

            _context.Roles.AddRange(roles);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Roles seeded successfully ({roles.Count} records).");
        }

        private async Task SeedUsersAsync()
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            if (existingUser != null)
            {
                Console.WriteLine("Admin user already exists. Skipping seeding.");
                return; // Admin user already exists
            }

            Console.WriteLine("Seeding Admin User...");

            var adminUser = new User
            {
                NameEn = "System Administrator",
                NameAr = "مدير النظام",
                Username = "admin",
                PasswordHash = HashPassword("Admin@123"),
                Phone = "0000000000",
                Email = "admin@warehousing.com",
                Address = "System Administration",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();
            Console.WriteLine("Admin user seeded successfully.");
        }

        private async Task SeedStatusesAsync()
        {
            // Get existing statuses
            var existingStatuses = await _context.Statuses.ToListAsync();
            var existingCodes = existingStatuses.Select(s => s.Code).ToHashSet();

            // Define all required statuses
            var statuses = new List<Status>
            {
                new Status { Code = "PENDING", NameEn = "Pending", NameAr = "قيد الانتظار", Description = "Order is created but not processed yet" },
                new Status { Code = "PROCESSING", NameEn = "Processing", NameAr = "جاري المعالجة", Description = "Order is being prepared or reviewed" },
                new Status { Code = "CONFIRMED", NameEn = "Confirmed", NameAr = "مؤكد", Description = "Order has been confirmed by the supplier/customer" },
                new Status { Code = "SHIPPED", NameEn = "Shipped", NameAr = "تم الشحن", Description = "Goods have been dispatched" },
                new Status { Code = "DELIVERED", NameEn = "Delivered", NameAr = "تم التسليم", Description = "Goods have been successfully delivered" },
                new Status { Code = "CANCELLED", NameEn = "Cancelled", NameAr = "تم الإلغاء", Description = "Order was cancelled" },
                new Status { Code = "RETURNED", NameEn = "Returned", NameAr = "تم الإرجاع", Description = "Goods were returned after delivery" },
                new Status { Code = "COMPLETED", NameEn = "Completed", NameAr = "مكتمل", Description = "Order completed successfully" },
                new Status { Code = "ONHOLD", NameEn = "On Hold", NameAr = "معلق", Description = "Order temporarily paused" },
                new Status { Code = "FAILED", NameEn = "Failed", NameAr = "فشل", Description = "Order failed due to payment or stock issue" },
                new Status { Code = "DRAFT", NameEn = "Save as draft", NameAr = "حفظ كمسودة", Description = "Order is saved but not submitted" }
            };

            // Only add statuses that don't already exist
            var statusesToAdd = statuses.Where(s => !existingCodes.Contains(s.Code)).ToList();
            
            if (statusesToAdd.Any())
            {
                _context.Statuses.AddRange(statusesToAdd);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Seeded {statusesToAdd.Count} new statuses.");
            }
            else
            {
                Console.WriteLine($"All statuses already exist in database ({existingStatuses.Count} records).");
            }
        }

        private async Task SeedOrderTypesAsync()
        {
            var existingOrderTypes = await _context.OrderTypes.ToListAsync();
            if (existingOrderTypes.Any())
            {
                Console.WriteLine($"OrderTypes already exist ({existingOrderTypes.Count} records). Skipping seeding.");
                return; // Order types already exist
            }

            Console.WriteLine("Seeding OrderTypes...");

            var orderTypes = new List<OrderType>
            {
                new OrderType { NameEn = "Purchase Order", NameAr = "أمر شراء", Description = "Order for purchasing products from suppliers", IsActive = true },
                new OrderType { NameEn = "Sale Order", NameAr = "أمر بيع", Description = "Order for selling products to customers", IsActive = true }
            };

            _context.OrderTypes.AddRange(orderTypes);
            await _context.SaveChangesAsync();
            Console.WriteLine($"OrderTypes seeded successfully ({orderTypes.Count} records).");
        }

        private async Task SeedTransactionTypesAsync()
        {
            var existingTransactionTypes = await _context.TransactionTypes.ToListAsync();
            if (existingTransactionTypes.Any())
            {
                Console.WriteLine($"TransactionTypes already exist ({existingTransactionTypes.Count} records). Skipping seeding.");
                return; // Transaction types already exist
            }

            Console.WriteLine("Seeding TransactionTypes...");

            var transactionTypes = new List<TransactionType>
            {
                new TransactionType { Code = "PURCHASE", NameEn = "Purchase Receipt", NameAr = "استلام مشتريات", Description = "Product received from a supplier" },
                new TransactionType { Code = "SALE", NameEn = "Sale Dispatch", NameAr = "صرف مبيعات", Description = "Product dispatched to a customer" },
                new TransactionType { Code = "RETURN_IN", NameEn = "Supplier Return", NameAr = "إرجاع للمورد", Description = "Product returned to the supplier" },
                new TransactionType { Code = "RETURN_OUT", NameEn = "Customer Return", NameAr = "إرجاع من العميل", Description = "Product returned by a customer" },
                new TransactionType { Code = "ADJUSTMENT_PLUS", NameEn = "Inventory Adjustment (+)", NameAr = "تعديل مخزون (+)", Description = "Increase stock manually (e.g., audit correction)" },
                new TransactionType { Code = "ADJUSTMENT_MINUS", NameEn = "Inventory Adjustment (-)", NameAr = "تعديل مخزون (-)", Description = "Decrease stock manually" },
                new TransactionType { Code = "TRANSFER_IN", NameEn = "Transfer In", NameAr = "تحويل وارد", Description = "Stock transferred into this warehouse/location" },
                new TransactionType { Code = "TRANSFER_OUT", NameEn = "Transfer Out", NameAr = "تحويل صادر", Description = "Stock transferred out of this warehouse/location" },
                new TransactionType { Code = "DAMAGE_LOSS", NameEn = "Damage / Loss", NameAr = "تالف أو ضائع", Description = "Stock marked as damaged or lost" },
                new TransactionType { Code = "SAMPLE", NameEn = "Sample Issue", NameAr = "إصدار نموذج", Description = "Issued for sample or demo purposes" },
                new TransactionType { Code = "", NameEn = "Initial Stock", NameAr = "رصيد ابتدائي", Description = "Initial stock setup for new system implementation" }
            };

            _context.TransactionTypes.AddRange(transactionTypes);
            await _context.SaveChangesAsync();
            Console.WriteLine($"TransactionTypes seeded successfully ({transactionTypes.Count} records).");
        }

        private async Task SeedUnitsAsync()
        {
            var existingUnits = await _context.Units.ToListAsync();
            if (existingUnits.Any())
            {
                Console.WriteLine($"Units already exist ({existingUnits.Count} records). Skipping seeding.");
                return; // Units already exist
            }

            Console.WriteLine("Seeding Units...");

            var units = new List<Unit>
            {
                new Unit { Code = "EA", NameEn = "Each", NameAr = "لكل وحدة", Description = "Standard unit for items", IsActive = true },
                new Unit { Code = "KG", NameEn = "Kilogram", NameAr = "كيلوغرام", Description = "Used for weight-based goods", IsActive = true },
                new Unit { Code = "GM", NameEn = "Gram", NameAr = "غرام", Description = "Small weight measurements", IsActive = true },
                new Unit { Code = "LT", NameEn = "Liter", NameAr = "لتر", Description = "Volume measurement", IsActive = true },
                new Unit { Code = "ML", NameEn = "Milliliter", NameAr = "ملليلتر", Description = "Small volume measurements", IsActive = true },
                new Unit { Code = "MTR", NameEn = "Meter", NameAr = "متر", Description = "Length measurement", IsActive = true },
                new Unit { Code = "CM", NameEn = "Centimeter", NameAr = "سنتيمتر", Description = "Smaller length unit", IsActive = true },
                new Unit { Code = "INCH", NameEn = "Inch", NameAr = "إنش", Description = "Imperial length unit", IsActive = true },
                new Unit { Code = "PKT", NameEn = "Packet", NameAr = "عبوة", Description = "Packaged quantity", IsActive = true },
                new Unit { Code = "BOX", NameEn = "Box", NameAr = "صندوق", Description = "Bulk packaging unit", IsActive = true }
            };

            _context.Units.AddRange(units);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Units seeded successfully ({units.Count} records).");
        }

        private async Task SeedStoresAsync()
        {
            var existingStores = await _context.Stores.ToListAsync();
            if (existingStores.Any())
            {
                Console.WriteLine($"Stores already exist ({existingStores.Count} records). Skipping seeding.");
                return; // Stores already exist
            }

            Console.WriteLine("Seeding Stores...");

            var stores = new List<Store>
            {
                new Store { NameEn = "Main Warehouse", NameAr = "المستودع الرئيسي", Description = "Primary storage location", Address = "Main Warehouse Address", IsActive = true },
                new Store { NameEn = "Secondary Warehouse", NameAr = "المستودع الثانوي", Description = "Secondary storage location", Address = "Secondary Warehouse Address", IsActive = true }
            };

            _context.Stores.AddRange(stores);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Stores seeded successfully ({stores.Count} records).");
        }

        private async Task SeedCompanyAsync()
        {
            var existingCompany = await _context.Companies.FirstOrDefaultAsync();
            if (existingCompany != null)
            {
                Console.WriteLine("Company already exists. Skipping seeding.");
                return; // Company already exists
            }

            Console.WriteLine("Seeding Company...");
            var company = new Company
            {
                NameEn = "General Trading Company",
                NameAr = "شركة التجارة العامة",
                AddressEn = "Business District, Main Street, Building No. 100",
                AddressAr = "منطقة الأعمال، الشارع الرئيسي، مبنى رقم 100",
                Phone = "+962 6 123 4567",
                Email = "info@company.com",
                Website = "https://www.company.com",
                TaxNumber = "123456789",
                Fax = "+962 6 123 4568",
                RegistrationNumber = "CR-2024-001234",
                Capital = 100000.00m,
                SloganEn = "Your Trusted Business Partner",
                SloganAr = "شريكك التجاري الموثوق",
                CurrencyCode = "JOD",
                FooterNoteEn = "Thank you for choosing us. We appreciate your business and look forward to serving you.",
                FooterNoteAr = "شكراً لاختياركم لنا. نحن نقدر تعاملكم معنا ونتطلع لخدمتكم.",
                TermsEn = "1. All orders are subject to stock availability.\n2. Prices are subject to change without prior notice.\n3. Payment terms: Net 30 days from invoice date.\n4. Delivery terms: FOB warehouse unless otherwise specified.\n5. Returns accepted within 7 days of delivery in original condition.\n6. Warranty terms apply as per manufacturer specifications.\n7. This invoice is valid for 30 days from the date of issue.",
                TermsAr = "1. جميع الطلبات خاضعة لتوفر المخزون.\n2. الأسعار قابلة للتغيير دون إشعار مسبق.\n3. شروط الدفع: صافي 30 يوماً من تاريخ الفاتورة.\n4. شروط التسليم: تسليم من المستودع ما لم يُذكر خلاف ذلك.\n5. يتم قبول المرتجعات خلال 7 أيام من التسليم بحالتها الأصلية.\n6. شروط الضمان تطبق وفقاً لمواصفات الشركة المصنعة.\n7. هذه الفاتورة صالحة لمدة 30 يوماً من تاريخ الإصدار.",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            Console.WriteLine("Company seeded successfully.");
        }

        private async Task SeedRolePermissionsAsync()
        {
            // Get all permissions and assign them to admin role
            var permissions = await _context.Permissions.ToListAsync();
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "ADMIN");
            
            if (adminRole != null && permissions.Any())
            {
                // Remove existing role permissions for admin (if any)
                var existingRolePermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == adminRole.Id)
                    .ToListAsync();
                
                if (existingRolePermissions.Any())
                {
                    _context.RolePermissions.RemoveRange(existingRolePermissions);
                    await _context.SaveChangesAsync();
                }

                // Add all permissions to admin role
                var rolePermissions = permissions.Select(p => new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = p.Id
                }).ToList();

                _context.RolePermissions.AddRange(rolePermissions);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Assigned {rolePermissions.Count} permissions to ADMIN role.");
            }
        }

        private async Task EnsureAdminRoleHasAllPermissionsAsync()
        {
            // Ensure admin role has all permissions (useful when permissions are added later)
            var permissions = await _context.Permissions.ToListAsync();
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "ADMIN");
            
            if (adminRole != null && permissions.Any())
            {
                var existingRolePermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == adminRole.Id)
                    .Select(rp => rp.PermissionId)
                    .ToHashSetAsync();

                var missingPermissions = permissions
                    .Where(p => !existingRolePermissions.Contains(p.Id))
                    .Select(p => new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = p.Id
                    })
                    .ToList();

                if (missingPermissions.Any())
                {
                    _context.RolePermissions.AddRange(missingPermissions);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Added {missingPermissions.Count} missing permissions to ADMIN role.");
                }
            }
        }

        private async Task SeedUserRolesAsync()
        {
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "ADMIN");
            
            if (adminUser != null && adminRole != null)
            {
                // Check if user role already exists
                var existingUserRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);
                
                if (existingUserRole == null)
                {
                    var userRole = new UserRole
                    {
                        UserId = adminUser.Id,
                        RoleId = adminRole.Id
                    };

                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Assigned ADMIN role to admin user.");
                }
            }
        }

        private async Task SeedWorkingHoursAsync()
        {
            try
            {
                // Get or create WorkingHours
                var workingHours = await _context.WorkingHours.FirstOrDefaultAsync();
                
                if (workingHours == null)
                {
                    Console.WriteLine("Seeding WorkingHours...");
                    workingHours = new WorkingHours
                    {
                        Name = "Default Working Hours",
                        Description = "Standard working hours (Sunday to Thursday, 8:00 AM to 5:00 PM)",
                        StartTime = new TimeSpan(8, 0, 0), // 8:00 AM
                        EndTime = new TimeSpan(17, 0, 0), // 5:00 PM
                        StartDay = DayOfWeek.Sunday,
                        EndDay = DayOfWeek.Thursday,
                        AllowWeekends = false,
                        AllowHolidays = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "system"
                    };

                    _context.WorkingHours.Add(workingHours);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"WorkingHours record created with ID: {workingHours.Id}");
                }
                else
                {
                    Console.WriteLine($"WorkingHours already exists (ID: {workingHours.Id}). Checking WorkingHoursDays...");
                }
                
                // Always check and seed WorkingHoursDays (even if WorkingHours already existed)
                var existingDays = await _context.WorkingHoursDays
                    .Where(whd => whd.WorkingHoursId == workingHours.Id)
                    .ToListAsync();
                
                if (existingDays.Any())
                {
                    Console.WriteLine($"WorkingHoursDays already exist ({existingDays.Count} records). Skipping seeding.");
                    return;
                }
                
                // Seed WorkingHoursDays for all days of the week
                Console.WriteLine("Seeding WorkingHoursDays...");
                var workingDays = new List<WorkingHoursDay>
                {
                    new WorkingHoursDay
                    {
                        WorkingHoursId = workingHours.Id,
                        DayOfWeek = DayOfWeek.Sunday,
                        StartTime = new TimeSpan(8, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0),
                        IsEnabled = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "system"
                    },
                    new WorkingHoursDay
                    {
                        WorkingHoursId = workingHours.Id,
                        DayOfWeek = DayOfWeek.Monday,
                        StartTime = new TimeSpan(8, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0),
                        IsEnabled = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "system"
                    },
                    new WorkingHoursDay
                    {
                        WorkingHoursId = workingHours.Id,
                        DayOfWeek = DayOfWeek.Tuesday,
                        StartTime = new TimeSpan(8, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0),
                        IsEnabled = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "system"
                    },
                    new WorkingHoursDay
                    {
                        WorkingHoursId = workingHours.Id,
                        DayOfWeek = DayOfWeek.Wednesday,
                        StartTime = new TimeSpan(8, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0),
                        IsEnabled = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "system"
                    },
                    new WorkingHoursDay
                    {
                        WorkingHoursId = workingHours.Id,
                        DayOfWeek = DayOfWeek.Thursday,
                        StartTime = new TimeSpan(8, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0),
                        IsEnabled = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "system"
                    },
                    // Friday and Saturday are not working days
                    new WorkingHoursDay
                    {
                        WorkingHoursId = workingHours.Id,
                        DayOfWeek = DayOfWeek.Friday,
                        StartTime = null,
                        EndTime = null,
                        IsEnabled = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "system"
                    },
                    new WorkingHoursDay
                    {
                        WorkingHoursId = workingHours.Id,
                        DayOfWeek = DayOfWeek.Saturday,
                        StartTime = null,
                        EndTime = null,
                        IsEnabled = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "system"
                    }
                };

                _context.WorkingHoursDays.AddRange(workingDays);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"WorkingHoursDays seeded successfully ({workingDays.Count} days configured).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding WorkingHours: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to see the error
            }
        }

        private async Task SeedPrinterConfigurationsAsync()
        {
            var existingConfigs = await _context.PrinterConfigurations.AnyAsync();
            if (existingConfigs)
            {
                Console.WriteLine("PrinterConfigurations already exist. Skipping seeding.");
                return; // Printer configurations already exist
            }

            Console.WriteLine("Seeding PrinterConfigurations...");

            // A4 Default Configuration
            var a4Config = new PrinterConfiguration
            {
                NameAr = "طابعة A4 الافتراضية",
                NameEn = "Default A4 Printer",
                Description = "إعدادات افتراضية لطابعة A4",
                PrinterType = "A4",
                PaperFormat = "A4",
                PaperWidth = 210,
                PaperHeight = 297,
                Margins = System.Text.Json.JsonSerializer.Serialize(new
                {
                    top = "20mm",
                    right = "20mm",
                    bottom = "20mm",
                    left = "20mm"
                }),
                FontSettings = System.Text.Json.JsonSerializer.Serialize(new
                {
                    fontFamily = "Arial",
                    baseFontSize = 12,
                    headerFontSize = 16,
                    footerFontSize = 10,
                    tableFontSize = 11
                }),
                PrintInColor = true,
                PrintBackground = true,
                Orientation = "Portrait",
                Scale = 1.0,
                IsActive = true,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            // POS/Thermal Default Configuration
            var posConfig = new PrinterConfiguration
            {
                NameAr = "طابعة نقاط البيع الحرارية",
                NameEn = "POS Thermal Printer",
                Description = "إعدادات افتراضية لطابعة نقاط البيع الحرارية 80مم",
                PrinterType = "POS",
                PaperFormat = "Thermal",
                PaperWidth = 80,
                PaperHeight = 0, // Continuous roll
                Margins = System.Text.Json.JsonSerializer.Serialize(new
                {
                    top = "5mm",
                    right = "5mm",
                    bottom = "5mm",
                    left = "5mm"
                }),
                FontSettings = System.Text.Json.JsonSerializer.Serialize(new
                {
                    fontFamily = "Courier",
                    baseFontSize = 10,
                    headerFontSize = 12,
                    footerFontSize = 8,
                    tableFontSize = 9
                }),
                PosSettings = System.Text.Json.JsonSerializer.Serialize(new
                {
                    encoding = "UTF-8",
                    copies = 1,
                    autoCut = true,
                    openCashDrawer = false,
                    printDensity = 8,
                    printSpeed = 3,
                    useEscPos = true,
                    connectionType = "USB",
                    connectionString = (string?)null
                }),
                PrintInColor = false,
                PrintBackground = false,
                Orientation = "Portrait",
                Scale = 1.0,
                IsActive = true,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _context.PrinterConfigurations.AddRange(a4Config, posConfig);
            await _context.SaveChangesAsync();

            // Assign A4 config to Admin role
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "ADMIN");
            if (adminRole != null)
            {
                adminRole.PrinterConfigurationId = a4Config.Id;
                await _context.SaveChangesAsync();
                Console.WriteLine("Assigned A4 printer configuration to ADMIN role.");
            }
            
            Console.WriteLine("PrinterConfigurations seeded successfully (2 configurations).");
        }

        private string HashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
