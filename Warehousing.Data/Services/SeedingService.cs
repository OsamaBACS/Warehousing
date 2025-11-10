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
            // Check if data already exists
            if (await _context.Roles.AnyAsync())
            {
                return; // Data already seeded
            }

            await SeedRolesAsync();
            await SeedPermissionsAsync();
            await SeedUsersAsync();
            await SeedCategoriesAsync();
            await SeedSubCategoriesAsync();
            await SeedStatusesAsync();
            await SeedOrderTypesAsync();
            await SeedTransactionTypesAsync();
            await SeedUnitsAsync();
            await SeedStoresAsync();
            await SeedCompanyAsync();
            await SeedRolePermissionsAsync();
            await SeedUserRolesAsync();
            await SeedWorkingHoursAsync();
        }

        private async Task SeedRolesAsync()
        {
            var roles = new List<Role>
            {
                new Role { Id = 1, Code = "ADMIN", NameEn = "Admin", NameAr = "مدير", IsActive = true },
                new Role { Id = 2, Code = "WAREHOUSE_MANAGER", NameEn = "Warehouse Manager", NameAr = "مسؤول المستودع", IsActive = true },
                new Role { Id = 3, Code = "SALES_MANAGER", NameEn = "Sales Manager", NameAr = "مدير المبيعات", IsActive = true },
                new Role { Id = 4, Code = "PURCHASE_MANAGER", NameEn = "Purchase Manager", NameAr = "مدير المشتريات", IsActive = true },
                new Role { Id = 5, Code = "USER", NameEn = "User", NameAr = "مستخدم", IsActive = true }
            };

            _context.Roles.AddRange(roles);
            await _context.SaveChangesAsync();
        }

        private async Task SeedPermissionsAsync()
        {
            // Get permissions from existing database
            var existingPermissions = await _context.Permissions.ToListAsync();
            if (existingPermissions.Any())
            {
                return; // Permissions already exist
            }

            // Add all the permissions from your existing database
            var permissions = new List<Permission>
            {
                // Product Management
                new Permission { Id = 6, Code = "VIEW_PRODUCTS", NameEn = "View Products", NameAr = "عرض المنتجات" },
                new Permission { Id = 7, Code = "ADD_PRODUCT", NameEn = "Add Product", NameAr = "إضافة منتج" },
                new Permission { Id = 8, Code = "EDIT_PRODUCT", NameEn = "Edit Product", NameAr = "تعديل منتج" },
                new Permission { Id = 9, Code = "DELETE_PRODUCT", NameEn = "Delete Product", NameAr = "حذف منتج" },
                new Permission { Id = 10, Code = "PRINT_PRODUCTS", NameEn = "Print Products", NameAr = "طباعة المنتجات" },
                
                // User Management
                new Permission { Id = 11, Code = "VIEW_USERS", NameEn = "View Users", NameAr = "عرض المستخدمين" },
                new Permission { Id = 12, Code = "ADD_USER", NameEn = "Add User", NameAr = "إضافة مستخدم" },
                new Permission { Id = 13, Code = "EDIT_USER", NameEn = "Edit User", NameAr = "تعديل مستخدم" },
                new Permission { Id = 14, Code = "DELETE_USER", NameEn = "Delete User", NameAr = "حذف مستخدم" },
                new Permission { Id = 15, Code = "RESET_USER_PASSWORD", NameEn = "Reset User Password", NameAr = "إعادة تعيين كلمة مرور المستخدم" },
                new Permission { Id = 16, Code = "ASSIGN_ROLES_TO_USER", NameEn = "Assign Roles to User", NameAr = "تعيين أدوار للمستخدم" },
                
                // Role Management
                new Permission { Id = 17, Code = "VIEW_ROLES", NameEn = "View Roles", NameAr = "عرض الأدوار" },
                new Permission { Id = 18, Code = "ADD_ROLE", NameEn = "Add Role", NameAr = "إضافة دور" },
                new Permission { Id = 19, Code = "EDIT_ROLE", NameEn = "Edit Role", NameAr = "تعديل دور" },
                new Permission { Id = 20, Code = "DELETE_ROLE", NameEn = "Delete Role", NameAr = "حذف دور" },
                new Permission { Id = 21, Code = "ASSIGN_PERMISSIONS_TO_ROLE", NameEn = "Assign Permissions to Role", NameAr = "تعيين صلاحيات للدور" },
                
                // Customer Management
                new Permission { Id = 22, Code = "VIEW_CUSTOMERS", NameEn = "View Customers", NameAr = "عرض العملاء" },
                new Permission { Id = 23, Code = "ADD_CUSTOMER", NameEn = "Add Customer", NameAr = "إضافة عميل" },
                new Permission { Id = 24, Code = "EDIT_CUSTOMER", NameEn = "Edit Customer", NameAr = "تعديل عميل" },
                new Permission { Id = 25, Code = "DELETE_CUSTOMER", NameEn = "Delete Customer", NameAr = "حذف عميل" },
                new Permission { Id = 26, Code = "PRINT_CUSTOMERS", NameEn = "Print Customers", NameAr = "طباعة العملاء" },
                
                // Supplier Management
                new Permission { Id = 27, Code = "VIEW_SUPPLIERS", NameEn = "View Suppliers", NameAr = "عرض الموردين" },
                new Permission { Id = 28, Code = "ADD_SUPPLIER", NameEn = "Add Supplier", NameAr = "إضافة مورد" },
                new Permission { Id = 29, Code = "EDIT_SUPPLIER", NameEn = "Edit Supplier", NameAr = "تعديل مورد" },
                new Permission { Id = 30, Code = "DELETE_SUPPLIER", NameEn = "Delete Supplier", NameAr = "حذف مورد" },
                new Permission { Id = 31, Code = "PRINT_SUPPLIERS", NameEn = "Print Suppliers", NameAr = "طباعة الموردين" },
                
                // Purchase Orders
                new Permission { Id = 32, Code = "VIEW_PURCHASE_ORDERS", NameEn = "View Purchase Orders", NameAr = "عرض أوامر الشراء" },
                new Permission { Id = 33, Code = "ADD_PURCHASE_ORDER", NameEn = "Add Purchase Order", NameAr = "إضافة أمر شراء" },
                new Permission { Id = 34, Code = "EDIT_PURCHASE_ORDER", NameEn = "Edit Purchase Order", NameAr = "تعديل أمر شراء" },
                new Permission { Id = 35, Code = "DELETE_PURCHASE_ORDER", NameEn = "Delete Purchase Order", NameAr = "حذف أمر شراء" },
                new Permission { Id = 36, Code = "COMPLETE_PURCHASE_ORDER", NameEn = "Complete Purchase Order", NameAr = "إنهاء أمر شراء" },
                new Permission { Id = 37, Code = "PRINT_PURCHASE_ORDER", NameEn = "Print Purchase Order", NameAr = "طباعة أمر شراء" },
                new Permission { Id = 38, Code = "APPROVE_PURCHASE_ORDER", NameEn = "Approve Purchase Order", NameAr = "اعتماد أمر شراء" },
                new Permission { Id = 39, Code = "CANCEL_PURCHASE_ORDER", NameEn = "Cancel Purchase Order", NameAr = "إلغاء أمر شراء" },
                
                // Sale Orders
                new Permission { Id = 40, Code = "VIEW_SALE_ORDERS", NameEn = "View Sale Orders", NameAr = "عرض أوامر البيع" },
                new Permission { Id = 41, Code = "ADD_SALE_ORDER", NameEn = "Add Sale Order", NameAr = "إضافة أمر بيع" },
                new Permission { Id = 42, Code = "EDIT_SALE_ORDER", NameEn = "Edit Sale Order", NameAr = "تعديل أمر بيع" },
                new Permission { Id = 43, Code = "DELETE_SALE_ORDER", NameEn = "Delete Sale Order", NameAr = "حذف أمر بيع" },
                new Permission { Id = 44, Code = "COMPLETE_SALE_ORDER", NameEn = "Complete Sale Order", NameAr = "إنهاء أمر بيع" },
                new Permission { Id = 45, Code = "PRINT_SALE_ORDER", NameEn = "Print Sale Order", NameAr = "طباعة أمر بيع" },
                new Permission { Id = 46, Code = "APPROVE_SALE_ORDER", NameEn = "Approve Sale Order", NameAr = "اعتماد أمر بيع" },
                new Permission { Id = 47, Code = "CANCEL_SALE_ORDER", NameEn = "Cancel Sale Order", NameAr = "إلغاء أمر بيع" },
                
                // Reports
                new Permission { Id = 48, Code = "VIEW_INVENTORY_REPORT", NameEn = "View Inventory Report", NameAr = "عرض تقرير المخزون" },
                new Permission { Id = 49, Code = "PRINT_INVENTORY_REPORT", NameEn = "Print Inventory Report", NameAr = "طباعة تقرير المخزون" },
                
                // Settings
                new Permission { Id = 50, Code = "VIEW_SETTINGS", NameEn = "View Settings", NameAr = "عرض الإعدادات" },
                new Permission { Id = 51, Code = "EDIT_SETTINGS", NameEn = "Edit Settings", NameAr = "تعديل الإعدادات" },
                
                // Admin Panel
                new Permission { Id = 52, Code = "VIEW_ADMIN", NameEn = "View Admin", NameAr = "الإدارة" },
                
                // Category Management
                new Permission { Id = 53, Code = "VIEW_CATEGORIES", NameEn = "View Categories", NameAr = "عرض التصنيفات" },
                new Permission { Id = 54, Code = "ADD_CATEGORY", NameEn = "Add Category", NameAr = "إضافة تصنيف" },
                new Permission { Id = 55, Code = "EDIT_CATEGORY", NameEn = "Edit Category", NameAr = "تعديل تصنيف" },
                new Permission { Id = 56, Code = "DELETE_CATEGORY", NameEn = "Delete Category", NameAr = "حذف تصنيف" },
                new Permission { Id = 57, Code = "PRINT_CATEGORIES", NameEn = "Print Categories", NameAr = "طباعة التصنيفات" },
                
                // Unit Management
                new Permission { Id = 58, Code = "VIEW_UNITS", NameEn = "View Units", NameAr = "عرض الوحدات" },
                new Permission { Id = 59, Code = "ADD_UNIT", NameEn = "Add Unit", NameAr = "إضافة وحدة" },
                new Permission { Id = 60, Code = "EDIT_UNIT", NameEn = "Edit Unit", NameAr = "تعديل وحدة" },
                new Permission { Id = 61, Code = "DELETE_UNIT", NameEn = "Delete Unit", NameAr = "حذف وحدة" },
                new Permission { Id = 62, Code = "PRINT_UNITS", NameEn = "Print Units", NameAr = "طباعة الوحدات" },
                
                // Store Management
                new Permission { Id = 63, Code = "VIEW_STORES", NameEn = "View Stores", NameAr = "عرض المستودعات" },
                new Permission { Id = 64, Code = "ADD_STORE", NameEn = "Add Store", NameAr = "إضافة مستودع" },
                new Permission { Id = 65, Code = "EDIT_STORE", NameEn = "Edit Store", NameAr = "تعديل مستودع" },
                new Permission { Id = 66, Code = "DELETE_STORE", NameEn = "Delete Store", NameAr = "حذف مستودع" },
                new Permission { Id = 67, Code = "PRINT_STORES", NameEn = "Print Stores", NameAr = "طباعة المستودعات" },
                
                // Store Transfers
                new Permission { Id = 68, Code = "VIEW_STORE_TRANSFERS", NameEn = "View Store Transfers", NameAr = "عرض تحويلات المخزون" },
                new Permission { Id = 69, Code = "ADD_STORE_TRANSFER", NameEn = "Add Store Transfer", NameAr = "إضافة تحويل مخزون" },
                new Permission { Id = 70, Code = "EDIT_STORE_TRANSFER", NameEn = "Edit Store Transfer", NameAr = "تعديل تحويل مخزون" },
                new Permission { Id = 71, Code = "DELETE_STORE_TRANSFER", NameEn = "Delete Store Transfer", NameAr = "حذف تحويل مخزون" },
                new Permission { Id = 72, Code = "APPROVE_STORE_TRANSFER", NameEn = "Approve Store Transfer", NameAr = "اعتماد تحويل مخزون" },
                new Permission { Id = 73, Code = "PRINT_STORE_TRANSFERS", NameEn = "Print Store Transfers", NameAr = "طباعة تحويلات المخزون" },
                
                // Inventory Management
                new Permission { Id = 74, Code = "VIEW_INVENTORY_MANAGEMENT", NameEn = "View Inventory Management", NameAr = "عرض إدارة المخزون" },
                new Permission { Id = 75, Code = "MANAGE_INVENTORY", NameEn = "Manage Inventory", NameAr = "إدارة المخزون" },
                new Permission { Id = 76, Code = "ADJUST_INVENTORY", NameEn = "Adjust Inventory", NameAr = "تعديل المخزون" },
                new Permission { Id = 77, Code = "VIEW_LOW_STOCK", NameEn = "View Low Stock", NameAr = "عرض المخزون المنخفض" },
                
                // Subcategory Management
                new Permission { Id = 78, Code = "VIEW_SUBCATEGORIES", NameEn = "View Subcategories", NameAr = "عرض التصنيفات الفرعية" },
                new Permission { Id = 79, Code = "ADD_SUBCATEGORY", NameEn = "Add Subcategory", NameAr = "إضافة تصنيف فرعي" },
                new Permission { Id = 80, Code = "EDIT_SUBCATEGORY", NameEn = "Edit Subcategory", NameAr = "تعديل تصنيف فرعي" },
                new Permission { Id = 81, Code = "DELETE_SUBCATEGORY", NameEn = "Delete Subcategory", NameAr = "حذف تصنيف فرعي" },
                new Permission { Id = 82, Code = "PRINT_SUBCATEGORIES", NameEn = "Print Subcategories", NameAr = "طباعة التصنيفات الفرعية" },
                
                // Additional permissions for invoice management
                new Permission { Id = 1006, Code = "EDIT_APPROVED_INVOICE", NameEn = "Edit Approved Invoice", NameAr = "تعديل فاتورة معتمدة" },
                
                // Activity Logs Management
                new Permission { Id = 1007, Code = "VIEW_ACTIVITY_LOGS", NameEn = "View Activity Logs", NameAr = "عرض سجل الأنشطة" },
                new Permission { Id = 1008, Code = "EXPORT_ACTIVITY_LOGS", NameEn = "Export Activity Logs", NameAr = "تصدير سجل الأنشطة" },
                
                // Working Hours Management
                new Permission { Id = 1009, Code = "VIEW_WORKING_HOURS", NameEn = "View Working Hours", NameAr = "عرض ساعات العمل" },
                new Permission { Id = 1010, Code = "EDIT_WORKING_HOURS", NameEn = "Edit Working Hours", NameAr = "تعديل ساعات العمل" },
                new Permission { Id = 1011, Code = "MANAGE_WORKING_HOURS_EXCEPTIONS", NameEn = "Manage Working Hours Exceptions", NameAr = "إدارة استثناءات ساعات العمل" },
                new Permission { Id = 1012, Code = "WORK_OUTSIDE_WORKING_HOURS", NameEn = "Work Outside Working Hours", NameAr = "العمل خارج ساعات العمل" }
            };

            _context.Permissions.AddRange(permissions);
            await _context.SaveChangesAsync();
        }

        private async Task SeedUsersAsync()
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            if (existingUser != null)
            {
                return; // Admin user already exists
            }

            var adminUser = new User
            {
                Id = 1,
                NameEn = "System Administrator",
                NameAr = "مدير النظام",
                Username = "admin",
                PasswordHash = HashPassword("Admin@123"),
                Phone = "0000000000",
                Email = "admin@warehousing.com",
                Address = "System Administration",
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1),
                CreatedBy = "system"
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();
        }

        private async Task SeedCategoriesAsync()
        {
            // Get categories from existing database
            var existingCategories = await _context.Categories.ToListAsync();
            if (existingCategories.Any())
            {
                return; // Categories already exist
            }

            // Categories will be populated from your existing database
        }

        private async Task SeedSubCategoriesAsync()
        {
            // Get subcategories from existing database
            var existingSubCategories = await _context.SubCategories.ToListAsync();
            if (existingSubCategories.Any())
            {
                return; // Subcategories already exist
            }

            // Subcategories will be populated from your existing database
        }

        private async Task SeedStatusesAsync()
        {
            // Get statuses from existing database
            var existingStatuses = await _context.Statuses.ToListAsync();
            if (existingStatuses.Any())
            {
                return; // Statuses already exist
            }

            // Statuses will be populated from your existing database
        }

        private async Task SeedOrderTypesAsync()
        {
            // Get order types from existing database
            var existingOrderTypes = await _context.OrderTypes.ToListAsync();
            if (existingOrderTypes.Any())
            {
                return; // Order types already exist
            }

            // Add basic order types
            var orderTypes = new List<OrderType>
            {
                new OrderType { Id = 1, NameEn = "Purchase Order", NameAr = "طلب شراء", Description = "Order for purchasing products from suppliers", IsActive = true },
                new OrderType { Id = 2, NameEn = "Sale Order", NameAr = "طلب بيع", Description = "Order for selling products to customers", IsActive = true },
                new OrderType { Id = 3, NameEn = "Transfer Order", NameAr = "طلب نقل", Description = "Order for transferring products between stores", IsActive = true },
                new OrderType { Id = 4, NameEn = "Return Order", NameAr = "طلب إرجاع", Description = "Order for returning products", IsActive = true }
            };

            _context.OrderTypes.AddRange(orderTypes);
            await _context.SaveChangesAsync();
        }

        private async Task SeedTransactionTypesAsync()
        {
            // Get transaction types from existing database
            var existingTransactionTypes = await _context.TransactionTypes.ToListAsync();
            if (existingTransactionTypes.Any())
            {
                return; // Transaction types already exist
            }

            // Transaction types will be populated from your existing database
        }

        private async Task SeedUnitsAsync()
        {
            // Get units from existing database
            var existingUnits = await _context.Units.ToListAsync();
            if (existingUnits.Any())
            {
                return; // Units already exist
            }

            // Add units from your existing database
            var units = new List<Unit>
            {
                new Unit { Id = 1, Code = "EA", NameEn = "Each", NameAr = "لكل وحدة", Description = "Standard unit for items", IsActive = true },
                new Unit { Id = 2, Code = "KG", NameEn = "Kilogram", NameAr = "كيلوغرام", Description = "Used for weight-based goods", IsActive = true },
                new Unit { Id = 3, Code = "GM", NameEn = "Gram", NameAr = "غرام", Description = "Small weight measurements", IsActive = true },
                new Unit { Id = 4, Code = "LT", NameEn = "Liter", NameAr = "لتر", Description = "Volume measurement", IsActive = true },
                new Unit { Id = 5, Code = "ML", NameEn = "Milliliter", NameAr = "ملليلتر", Description = "Small volume measurements", IsActive = true },
                new Unit { Id = 6, Code = "MTR", NameEn = "Meter", NameAr = "متر", Description = "Length measurement", IsActive = true },
                new Unit { Id = 7, Code = "CM", NameEn = "Centimeter", NameAr = "سنتيمتر", Description = "Smaller length unit", IsActive = true },
                new Unit { Id = 8, Code = "INCH", NameEn = "Inch", NameAr = "إنش", Description = "Imperial length unit", IsActive = true },
                new Unit { Id = 9, Code = "PKT", NameEn = "Packet", NameAr = "عبوة", Description = "Packaged quantity", IsActive = true },
                new Unit { Id = 10, Code = "BOX", NameEn = "Box", NameAr = "صندوق", Description = "Bulk packaging unit", IsActive = true }
            };

            _context.Units.AddRange(units);
            await _context.SaveChangesAsync();
        }

        private async Task SeedStoresAsync()
        {
            var existingStores = await _context.Stores.ToListAsync();
            if (existingStores.Any())
            {
                return; // Stores already exist
            }

            var stores = new List<Store>
            {
                new Store { Id = 1, NameEn = "Main Warehouse", NameAr = "المستودع الرئيسي", Description = "Primary storage location", Address = "Main Warehouse Address", IsActive = true },
                new Store { Id = 2, NameEn = "Secondary Warehouse", NameAr = "المستودع الثانوي", Description = "Secondary storage location", Address = "Secondary Warehouse Address", IsActive = true }
            };

            _context.Stores.AddRange(stores);
            await _context.SaveChangesAsync();
        }

        private async Task SeedCompanyAsync()
        {
            var existingCompany = await _context.Companies.FirstOrDefaultAsync();
            if (existingCompany != null)
            {
                return; // Company already exists
            }

            var company = new Company
            {
                Id = 1,
                NameEn = "Warehousing Solutions Ltd",
                NameAr = "شركة حلول المستودعات",
                AddressEn = "123 Business Street, City Center",
                AddressAr = "شارع الأعمال 123، وسط المدينة",
                Phone = "0000000000",
                Email = "info@warehousing.com",
                Website = "www.warehousing.com",
                TaxNumber = "TAX123456789",
                CurrencyCode = "JOD",
                FooterNoteEn = "Thank you for your business!",
                FooterNoteAr = "شكراً لتعاملكم معنا!",
                TermsEn = "Terms and conditions apply",
                TermsAr = "تطبق الشروط والأحكام",
                IsActive = true
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
        }

        private async Task SeedRolePermissionsAsync()
        {
            // Get all permissions and assign them to admin role
            var permissions = await _context.Permissions.ToListAsync();
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "ADMIN");
            
            if (adminRole != null && permissions.Any())
            {
                var rolePermissions = permissions.Select((p, index) => new RolePermission
                {
                    Id = index + 1,
                    RoleId = adminRole.Id,
                    PermissionId = p.Id
                }).ToList();

                _context.RolePermissions.AddRange(rolePermissions);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedUserRolesAsync()
        {
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "ADMIN");
            
            if (adminUser != null && adminRole != null)
            {
                var userRole = new UserRole
                {
                    Id = 1,
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                };

                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedWorkingHoursAsync()
        {
            var existingWorkingHours = await _context.WorkingHours.FirstOrDefaultAsync();
            if (existingWorkingHours != null)
            {
                return; // Working hours already exist
            }

            var workingHours = new WorkingHours
            {
                Id = 1,
                Name = "Default Working Hours",
                Description = "Standard working hours (Sunday to Thursday, 8:00 AM to 5:00 PM)",
                StartTime = new TimeSpan(8, 0, 0), // 8:00 AM
                EndTime = new TimeSpan(17, 0, 0), // 5:00 PM
                StartDay = DayOfWeek.Sunday,
                EndDay = DayOfWeek.Thursday,
                AllowWeekends = false,
                AllowHolidays = false,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1),
                CreatedBy = "system"
            };

            _context.WorkingHours.Add(workingHours);
            await _context.SaveChangesAsync();
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
