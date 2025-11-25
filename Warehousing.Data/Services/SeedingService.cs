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
            // Always seed permissions first (they may be missing even if roles exist)
            await SeedPermissionsAsync();

            // Run the rest of the seeding in an idempotent way
            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedCategoriesAsync();
            await SeedStatusesAsync();
            await SeedOrderTypesAsync();
            await SeedTransactionTypesAsync();
            await SeedUnitsAsync();
            await SeedStoresAsync();
            await SeedCompanyAsync();
            await SeedRolePermissionsAsync();
            await SeedUserRolesAsync();
            await SeedWorkingHoursAsync();
            await SeedPrinterConfigurationsAsync();
        }

        private async Task SeedRolesAsync()
        {
            var defaultRoles = new List<Role>
            {
                new Role { Code = "ADMIN", NameEn = "Admin", NameAr = "مدير", IsActive = true },
                new Role { Code = "WAREHOUSE_MANAGER", NameEn = "Warehouse Manager", NameAr = "مسؤول المستودع", IsActive = true },
                new Role { Code = "SALES_MANAGER", NameEn = "Sales Manager", NameAr = "مدير المبيعات", IsActive = true },
                new Role { Code = "PURCHASE_MANAGER", NameEn = "Purchase Manager", NameAr = "مدير المشتريات", IsActive = true },
                new Role { Code = "USER", NameEn = "User", NameAr = "مستخدم", IsActive = true }
            };

            var existingRoleCodes = await _context.Roles
                .Select(r => r.Code)
                .Where(c => c != null)
                .ToListAsync();

            var toAdd = defaultRoles
                .Where(r => !existingRoleCodes.Contains(r.Code))
                .ToList();

            if (toAdd.Any())
            {
                // Let SQL Server generate identity values
                _context.Roles.AddRange(toAdd);
                await _context.SaveChangesAsync();
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
        }

        private async Task SeedCategoriesAsync()
        {
            // If categories already contain the new data (identified by a key category), skip reseeding
            var hasNewData = await _context.Categories.AnyAsync(c => c.NameAr == "بديل الخشب");
            if (hasNewData)
            {
                return;
            }

            // Only seed if database is empty - never delete existing categories!
            // This prevents data loss on every startup
            var hasAnyCategories = await _context.Categories.AnyAsync();
            var hasAnySubCategories = await _context.SubCategories.AnyAsync();
            
            // If there's any existing data, don't seed (user might have modified it)
            if (hasAnyCategories || hasAnySubCategories)
            {
                Console.WriteLine("Categories or SubCategories already exist. Skipping seed to prevent data loss.");
                return;
            }

            // Define new categories dataset (LegacyId used only for mapping subcategories; DB Id is identity)
            var categories = new[]
            {
                new { LegacyId = 1,  NameAr = "بديل الخشب",              NameEn = "بديل الخشب",              Description = "",                                           ImagePath = (string?)"Resources/Images/Category/64cb3df8-aa35-445b-84a5-e8fc4d3d9a4c.jpeg", IsActive = true },
                new { LegacyId = 2,  NameAr = "لوازم النجارين",          NameEn = "لوازم النجارين",          Description = "Steel bars, mesh, structural steel",          ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 3,  NameAr = "الزهيوي",                 NameEn = "ZHWEI",                   Description = "فيابر حديد وستانليس\r\n\r\nألمازات\r\n\r\nريش حديد\r\n\r\nريش SDS\r\n\r\nريش خراقة\r\n\r\nريش حجر\r\n\r\nرواسي مصلب", ImagePath = (string?)null, IsActive = true },
                new { LegacyId = 4,  NameAr = "الأدوات الصحية",           NameEn = "الأدوات الصحية",           Description = "بطاريات + محابس + شطافات",                 ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 5,  NameAr = "الخردوات",                NameEn = "الخردوات",                Description = "جنازير + رول بلاك + عدد",                    ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 6,  NameAr = "زرافيل",                  NameEn = "زرافيل",                  Description = "زرافيل كامل + سلندرات",                    ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 7,  NameAr = "الديكور ولوازمه",         NameEn = "الديكور ولوازمه",         Description = "الديكور ولوازمه",                            ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 8,  NameAr = "البراغي والمسامير",       NameEn = "البراغي والمسامير",       Description = "البراغي والمسامير",                          ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 9,  NameAr = "لوازم الدهان",            NameEn = "لوازم الدهان",            Description = "",                                           ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 10, NameAr = "شبك وأسلاك",              NameEn = "شبك وأسلاك",              Description = "شبك وأسلاك",                                  ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 11, NameAr = "دهانات مشكل",             NameEn = "دهانات مشكل",             Description = "دهانات أصناف جديدة",                        ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 12, NameAr = "العدد ولوازمها",          NameEn = "العدد ولوازمها",          Description = "العدد ولوازمها",                            ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 13, NameAr = "فوم جدران رولات وقطع",   NameEn = "فوم جدران رولات وقطع",   Description = "فوم جدران رولات وقطع",                      ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 14, NameAr = "فوم جدران رولات",         NameEn = "فوم جدران رولات",         Description = "فوم جدران رولات",                            ImagePath = (string?)null,                                  IsActive = false },
                new { LegacyId = 15, NameAr = "معدات Conan",             NameEn = "معدات Conan",             Description = "معدات Conan",                                ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 16, NameAr = "Dulux Paints",            NameEn = "دهانات ديلوكس",           Description = "Dulux Paints",                               ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 17, NameAr = "Golden Paints",           NameEn = "دهانات جولدن",            Description = "دهانات جولدن",                               ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 18, NameAr = "Quds Paints",             NameEn = "دهانات القدس",            Description = "دهانات القدس",                               ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 19, NameAr = "منتجات سافيتو",          NameEn = "منتجات سافيتو",          Description = "منتجات سافيتو",                             ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 20, NameAr = "علب رش ومزيل صدأ",        NameEn = "علب رش ومزيل صدأ",        Description = "علب رش ومزيل صدأ",                           ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 21, NameAr = "دهانات روز باريس",        NameEn = "دهانات روز باريس",        Description = "دهانات روز باريس",                           ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 22, NameAr = "منتجات تيراكو",          NameEn = "منتجات تيراكو",          Description = "منتجات تيراكو",                             ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 23, NameAr = "منتجات أبولو",           NameEn = "منتجات أبولو",           Description = "منتجات أبولو",                              ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 24, NameAr = "الكهربائيات",            NameEn = "الكهربائيات",            Description = "الكهربائيات",                               ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 25, NameAr = "قطع مواسير تصريف",       NameEn = "قطع مواسير تصريف",       Description = "قطع مواسير تصريف",                          ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 26, NameAr = "دهانات ناشونال",          NameEn = "دهانات ناشونال",          Description = "دهانات ناشونال",                             ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 27, NameAr = "فورسيلينق",              NameEn = "فورسيلينق",              Description = "فورسيلينق",                                  ImagePath = (string?)null,                                  IsActive = false },
                new { LegacyId = 28, NameAr = "كيازر",                  NameEn = "كيازر",                  Description = "كيازر",                                      ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 29, NameAr = "معدات كازبر",            NameEn = "معدات كازبر",            Description = "معدات كازبر",                               ImagePath = (string?)null,                                  IsActive = true },
                new { LegacyId = 30, NameAr = "مضخات",                  NameEn = "مضخات",                  Description = "مضخات",                                      ImagePath = (string?)null,                                  IsActive = true }
            };

            var categoryEntities = categories.Select(c => new Category
            {
                NameAr = c.NameAr,
                NameEn = c.NameEn,
                Description = c.Description,
                ImagePath = c.ImagePath,
                IsActive = c.IsActive
            }).ToList();

            _context.Categories.AddRange(categoryEntities);
            await _context.SaveChangesAsync();

            // Build mapping from LegacyId to actual database Id using NameAr as key
            var categoryMap = _context.Categories
                .ToDictionary(c => c.NameAr, c => c.Id);

            var legacyIdToDbId = categories.ToDictionary(
                c => c.LegacyId,
                c => categoryMap[c.NameAr]
            );

            // Seed subcategories based on provided dataset
            await SeedSubCategoriesAsync(legacyIdToDbId);
        }

        private async Task SeedSubCategoriesAsync(Dictionary<int, int> categoryIdMap)
        {
            // If we already have any of the new subcategories, skip reseeding
            var hasNewData = await _context.SubCategories.AnyAsync(sc => sc.NameAr == "رونديلات");
            if (hasNewData)
            {
                return;
            }

            var subCategories = new[]
            {
                new { NameAr = "رونديلات",                          NameEn = "رونديلات",                          Description = "رونديلات",                          IsActive = true,  LegacyCategoryId = 8,  ImagePath = (string?)null },
                new { NameAr = "زوايا",                            NameEn = "زوايا",                            Description = "زوايا",                            IsActive = true,  LegacyCategoryId = 2,  ImagePath = (string?)null },
                new { NameAr = "ميزان ماء Conan",                  NameEn = "ميزان ماء Conan",                  Description = "ميزان ماء Conan",                  IsActive = true,  LegacyCategoryId = 15, ImagePath = (string?)null },
                new { NameAr = "Steel Mesh",                       NameEn = "شبك فولاذي",                       Description = "For concrete reinforcement",         IsActive = false, LegacyCategoryId = 2,  ImagePath = (string?)null },
                new { NameAr = "Clay Bricks",                      NameEn = "طوب طيني",                         Description = "Traditional building material",     IsActive = false, LegacyCategoryId = 3,  ImagePath = (string?)null },
                new { NameAr = "Concrete Blocks",                  NameEn = "بلوك خرساني",                      Description = "Hollow or solid blocks",            IsActive = false, LegacyCategoryId = 3,  ImagePath = (string?)null },
                new { NameAr = "PVC Pipes",                        NameEn = "أنابيب PVC",                        Description = "For drainage and water systems",    IsActive = false, LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "Copper Pipes",                     NameEn = "أنابيب نحاسية",                    Description = "Used for water supply",             IsActive = false, LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "Electrical Wires",                 NameEn = "أسلاك كهربائية",                   Description = "Insulated copper wires",            IsActive = false, LegacyCategoryId = 5,  ImagePath = (string?)null },
                new { NameAr = "Switches & Sockets",               NameEn = "مفاتيح ومقابس",                    Description = "Power control devices",             IsActive = false, LegacyCategoryId = 5,  ImagePath = (string?)null },
                new { NameAr = "زرفيل كامل HD",                   NameEn = "زرفيل كامل HD",                   Description = "زرفيل كامل HD",                   IsActive = true,  LegacyCategoryId = 6,  ImagePath = (string?)null },
                new { NameAr = "زرفيل كامل سوبر",                 NameEn = "زرفيل كامل سوبر",                 Description = "زرفيل كامل سوبر",                 IsActive = true,  LegacyCategoryId = 6,  ImagePath = (string?)null },
                new { NameAr = "Toilets",                          NameEn = "مراحيض",                            Description = "Standard and smart toilets",        IsActive = false, LegacyCategoryId = 7,  ImagePath = (string?)null },
                new { NameAr = "Bathroom Faucets",                 NameEn = "حنفيات حمام",                      Description = "Taps and mixers",                   IsActive = true,  LegacyCategoryId = 7,  ImagePath = (string?)null },
                new { NameAr = "Ceramic Tiles",                    NameEn = "بلاط سيراميك",                     Description = "Wall and floor tiles",              IsActive = false, LegacyCategoryId = 8,  ImagePath = (string?)null },
                new { NameAr = "Marble Flooring",                  NameEn = "أرضيات رخام",                      Description = "Luxury floor option",               IsActive = false, LegacyCategoryId = 8,  ImagePath = (string?)null },
                new { NameAr = "Wooden Doors",                     NameEn = "أبواب خشبية",                      Description = "Interior and exterior use",         IsActive = false, LegacyCategoryId = 9,  ImagePath = (string?)null },
                new { NameAr = "فراشي دهان",                      NameEn = "فراشي دهان",                       Description = "",                                  IsActive = false, LegacyCategoryId = 9,  ImagePath = (string?)null },
                new { NameAr = "Hand Tools",                       NameEn = "أدوات يدوية",                       Description = "Saws, hammers, screwdrivers",       IsActive = true,  LegacyCategoryId = 10, ImagePath = (string?)null },
                new { NameAr = "Power Tools",                      NameEn = "أدوات كهربائية",                    Description = "Drills, grinders, etc.",            IsActive = true,  LegacyCategoryId = 10, ImagePath = (string?)null },
                new { NameAr = "WPC (20cm X 290cm)",               NameEn = "بديل الخشب (20cm X 290cm)",        Description = "بديل الخشب",                        IsActive = true,  LegacyCategoryId = 1,  ImagePath = (string?)"Resources/Images/SubCategory/10775048-b935-462b-9258-b4f127db8967_4de9fffd-65a1-4eb2-94ef-f5e215ea6e10.jpeg" },
                new { NameAr = "(WPC (16cm X 290cm",               NameEn = "بديل الخشب (16cm X 290cm)",        Description = "",                                  IsActive = true,  LegacyCategoryId = 1,  ImagePath = (string?)null },
                new { NameAr = "(Circled WPC (16cm x 290cm",       NameEn = "بديل الخشب الدائري (16cm x 290cm)", Description = "",                                 IsActive = true,  LegacyCategoryId = 1,  ImagePath = (string?)null },
                new { NameAr = "WPC PS (12cm X 290cm)",            NameEn = "بديل الخشب PS (12cm X 290cm)",     Description = "",                                  IsActive = true,  LegacyCategoryId = 1,  ImagePath = (string?)null },
                new { NameAr = "محابس وحنفيات",                   NameEn = "محابس وحنفيات",                   Description = "محبس زاوية\r\n\r\nمحبس دبل\r\n\r\nمحبس غسالة\r\n\r\nمحبس ستيم\r\n\r\nحنفيات", IsActive = true, LegacyCategoryId = 4, ImagePath = (string?)null },
                new { NameAr = "برابيش",                          NameEn = "برابيش",                          Description = "بربيش غسالة \r\n\r\nبرابيش قيزر\r\n\r\nبرابيش بطارية", IsActive = true, LegacyCategoryId = 4, ImagePath = (string?)null },
                new { NameAr = "بطارية مغسلة",                    NameEn = "بطارية مغسلة",                    Description = "",                                  IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "بطارية مجلى",                     NameEn = "بطارية مجلى",                     Description = "",                                  IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "بطارية دش",                       NameEn = "بطارية دش",                       Description = "",                                  IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "شطافات",                          NameEn = "شطافات",                          Description = "شطافات كروم (نحاس أو بلاستك)\r\n\r\nشطافات بلاستيك ابيض", IsActive = true, LegacyCategoryId = 4, ImagePath = (string?)null },
                new { NameAr = "سيفونات وصبابات",                NameEn = "سيفونات وصبابات",                Description = "سيفون مغسلة\r\n\r\nسيفون مجلى\r\n\r\nصباب مغسلة كبس + عادي", IsActive = true, LegacyCategoryId = 4, ImagePath = (string?)null },
                new { NameAr = "ردادات ماء",                      NameEn = "ردادات ماء",                      Description = "ردادات ماء",                        IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "فيابر حديد وستانليس",            NameEn = "فيابر حديد وستانليس",            Description = "فيابر حديد وستانليس",               IsActive = true,  LegacyCategoryId = 3,  ImagePath = (string?)null },
                new { NameAr = "ألمازات",                         NameEn = "ألمازات",                         Description = "ألمازات قص\r\n\r\nألمازات حف",      IsActive = true,  LegacyCategoryId = 3,  ImagePath = (string?)null },
                new { NameAr = "ريش حديد",                        NameEn = "ريش حديد",                        Description = "ريش حديد",                         IsActive = true,  LegacyCategoryId = 3,  ImagePath = (string?)null },
                new { NameAr = "ريش SDS",                         NameEn = "ريش SDS",                         Description = "ريش SDS",                          IsActive = true,  LegacyCategoryId = 3,  ImagePath = (string?)null },
                new { NameAr = "ريش خراقة",                       NameEn = "ريش خراقة",                       Description = "ريش خراقة",                        IsActive = true,  LegacyCategoryId = 3,  ImagePath = (string?)null },
                new { NameAr = "ريش حجر",                         NameEn = "ريش حجر",                         Description = "ريش حجر",                          IsActive = true,  LegacyCategoryId = 3,  ImagePath = (string?)null },
                new { NameAr = "رواسي مصلب",                     NameEn = "رواسي مصلب",                     Description = "رواسي مصلب",                       IsActive = true,  LegacyCategoryId = 3,  ImagePath = (string?)null },
                new { NameAr = "نسلات وهولسو وفتاحات",           NameEn = "نسلات وهولسو وفتاحات",           Description = "نسلات وهولسو وفتاحات",             IsActive = true,  LegacyCategoryId = 3,  ImagePath = (string?)null },
                new { NameAr = "اقفال",                          NameEn = "اقفال",                          Description = "",                                  IsActive = true,  LegacyCategoryId = 5,  ImagePath = (string?)null },
                new { NameAr = "زرفيل قديمLD",                   NameEn = "زرفيل قديم LD",                  Description = "",                                  IsActive = true,  LegacyCategoryId = 6,  ImagePath = (string?)null },
                new { NameAr = "سلندرات",                        NameEn = "سلندرات",                        Description = "",                                  IsActive = true,  LegacyCategoryId = 6,  ImagePath = (string?)null },
                new { NameAr = "زرفيل حمام",                     NameEn = "زرفيل حمام",                     Description = "",                                  IsActive = true,  LegacyCategoryId = 6,  ImagePath = (string?)null },
                new { NameAr = "زرفيل زاتي",                     NameEn = "زرفيل زاتي",                     Description = "",                                  IsActive = true,  LegacyCategoryId = 6,  ImagePath = (string?)null },
                new { NameAr = "مشاحيف ستانليس",                NameEn = "مشاحيف ستانليس",                Description = "مشاحيف ستانليس",                  IsActive = true,  LegacyCategoryId = 9,  ImagePath = (string?)null },
                new { NameAr = "سكاكين معجونة ستانليس",          NameEn = "سكاكين معجونة ستانليس",          Description = "سكاكين معجونة ستانليس",            IsActive = true,  LegacyCategoryId = 9,  ImagePath = (string?)null },
                new { NameAr = "مشاحيف حديد",                    NameEn = "مشاحيف حديد",                    Description = "مشاحيف حديد",                      IsActive = true,  LegacyCategoryId = 9,  ImagePath = (string?)null },
                new { NameAr = "رولات دهان وفتايل",              NameEn = "رولات دهان وفتايل",              Description = "رولات دهان وفتايل",                IsActive = true,  LegacyCategoryId = 9,  ImagePath = (string?)null },
                new { NameAr = "قواعد حف دهان يدوي وماكنات",      NameEn = "قواعد حف دهان يدوي وماكنات",      Description = "قواعد حف دهان يدوي وماكنات",        IsActive = true,  LegacyCategoryId = 2,  ImagePath = (string?)null },
                new { NameAr = "فرد رش",                          NameEn = "فرد رش",                          Description = "فرد رش دهان",                       IsActive = true,  LegacyCategoryId = 9,  ImagePath = (string?)null },
                new { NameAr = "جنازير",                         NameEn = "جنازير",                         Description = "جنازير",                           IsActive = true,  LegacyCategoryId = 5,  ImagePath = (string?)null },
                new { NameAr = "زوايا رفوف ديكور",               NameEn = "زوايا رفوف ديكور",               Description = "زوايا رفوف ديكور",                 IsActive = true,  LegacyCategoryId = 2,  ImagePath = (string?)null },
                new { NameAr = "علاقات ملابس",                   NameEn = "علاقات ملابس",                   Description = "علاقات ملابس",                     IsActive = true,  LegacyCategoryId = 5,  ImagePath = (string?)null },
                new { NameAr = "سكك جوارير بيل",                 NameEn = "سكك جوارير بيل",                 Description = "سكك جوارير بيل",                   IsActive = true,  LegacyCategoryId = 2,  ImagePath = (string?)null },
                new { NameAr = "رسمات تطبيع",                    NameEn = "رسمات تطبيع",                    Description = "رسمات تطبيع",                      IsActive = true,  LegacyCategoryId = 9,  ImagePath = (string?)null },
                new { NameAr = "يونيكا",                         NameEn = "يونيكا",                         Description = "يونيكا",                           IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "جسر حمام",                       NameEn = "جسر حمام",                       Description = "جسر حمام",                         IsActive = true,  LegacyCategoryId = 5,  ImagePath = (string?)null },
                new { NameAr = "قاعدة اصلاح فصالة",              NameEn = "قاعدة اصلاح فصالة",              Description = "قاعدة اصلاح فصالة",                IsActive = true,  LegacyCategoryId = 2,  ImagePath = (string?)null },
                new { NameAr = "مجابد",                          NameEn = "مجابد",                          Description = "مجابد",                           IsActive = true,  LegacyCategoryId = 2,  ImagePath = (string?)null },
                new { NameAr = "فصالات",                         NameEn = "فصالات",                         Description = "فصالات",                          IsActive = true,  LegacyCategoryId = 2,  ImagePath = (string?)null },
                new { NameAr = "نبل بطارية",                     NameEn = "نبل بطارية",                     Description = "نبل بطارية",                       IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "سلك تربيط مجلفن 1ملم",          NameEn = "سلك تربيط مجلفن 1ملم",          Description = "سلك تربيط مجلفن 1ملم",            IsActive = true,  LegacyCategoryId = 10, ImagePath = (string?)null },
                new { NameAr = "براغي جبس بورد",                NameEn = "براغي جبس بورد",                Description = "براغي جبس بورد",                  IsActive = true,  LegacyCategoryId = 8,  ImagePath = (string?)null },
                new { NameAr = "براغي شيقا",                    NameEn = "براغي شيقا",                    Description = "براغي شيقا",                      IsActive = true,  LegacyCategoryId = 8,  ImagePath = (string?)null },
                new { NameAr = "براغي هنجر",                    NameEn = "براغي هنجر",                    Description = "براغي هنجر",                      IsActive = true,  LegacyCategoryId = 8,  ImagePath = (string?)null },
                new { NameAr = "براغي مسنن طول متر",            NameEn = "براغي مسنن طول متر",            Description = "براغي مسنن طول متر",              IsActive = true,  LegacyCategoryId = 8,  ImagePath = (string?)null },
                new { NameAr = "صواميل",                         NameEn = "صواميل",                         Description = "صواميل",                           IsActive = true,  LegacyCategoryId = 8,  ImagePath = (string?)null },
                new { NameAr = "براغي شيبورد",                  NameEn = "براغي شيبورد",                  Description = "براغي شيبورد",                    IsActive = true,  LegacyCategoryId = 8,  ImagePath = (string?)null },
                new { NameAr = "براغي رول بلاك",                NameEn = "براغي رول بلاك",                Description = "براغي رول بلاك",                  IsActive = true,  LegacyCategoryId = 8,  ImagePath = (string?)null },
                new { NameAr = "زمبرك شفاط قصدير",              NameEn = "زمبرك شفاط قصدير",              Description = "زمبرك شفاط قصدير",                IsActive = true,  LegacyCategoryId = 5,  ImagePath = (string?)null },
                new { NameAr = "فوم جدران رولات",               NameEn = "فوم جدران رولات",               Description = "فوم جدران رولات",                 IsActive = true,  LegacyCategoryId = 13, ImagePath = (string?)null },
                new { NameAr = "فوم جدران قطع",                 NameEn = "فوم جدران قطع",                 Description = "فوم جدران قطع",                   IsActive = true,  LegacyCategoryId = 13, ImagePath = (string?)null },
                new { NameAr = "مصارف ستانليس",                NameEn = "مصارف ستانليس",                Description = "مصارف ستانليس",                  IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "بديل الحجر",                     NameEn = "بديل الحجر",                     Description = "بديل الحجر",                       IsActive = true,  LegacyCategoryId = 7,  ImagePath = (string?)null },
                new { NameAr = "زوايا حديد رفيع (عرض 1.5سم)",   NameEn = "زوايا حديد رفيع (عرض 1.5سم)",   Description = "زوايا حديد رفيع (عرض 1.5سم)",      IsActive = true,  LegacyCategoryId = 2,  ImagePath = (string?)null },
                new { NameAr = "زوايا حديد عريض (عرض 3سم)",     NameEn = "زوايا حديد عريض (عرض 3سم)",     Description = "زوايا حديد عريض (عرض 3سم)",        IsActive = true,  LegacyCategoryId = 2,  ImagePath = (string?)null },
                new { NameAr = "شبك مربع 1x1سم",                NameEn = "شبك مربع 1x1سم",                Description = "شبك مربع 1x1سم",                  IsActive = true,  LegacyCategoryId = 10, ImagePath = (string?)null },
                new { NameAr = "أطقم براغي تثبيت",              NameEn = "أطقم براغي تثبيت",              Description = "أطقم براغي تثبيت",                IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "زوايا رف ابيض",                 NameEn = "زوايا رف ابيض",                 Description = "زوايا رف ابيض",                   IsActive = true,  LegacyCategoryId = 2,  ImagePath = (string?)null },
                new { NameAr = "مسامير بولاد",                  NameEn = "مسامير بولاد",                  Description = "مسامير بولاد",                    IsActive = true,  LegacyCategoryId = 8,  ImagePath = (string?)null },
                new { NameAr = "بوكس هنجر",                     NameEn = "بوكس هنجر",                     Description = "بوكس هنجر",                       IsActive = true,  LegacyCategoryId = 5,  ImagePath = (string?)null },
                new { NameAr = "مقصات Conan",                   NameEn = "مقصات Conan",                   Description = "مقصات Conan",                     IsActive = true,  LegacyCategoryId = 15, ImagePath = (string?)null },
                new { NameAr = "مشارط Conan",                   NameEn = "مشارط Conan",                   Description = "مشارط Conan",                     IsActive = true,  LegacyCategoryId = 15, ImagePath = (string?)null },
                new { NameAr = "مسطرين Conan",                  NameEn = "مسطرين Conan",                  Description = "مسطرين Conan",                    IsActive = true,  LegacyCategoryId = 15, ImagePath = (string?)null },
                new { NameAr = "أمتار Conan",                   NameEn = "أمتار Conan",                   Description = "أمتار Conan",                     IsActive = true,  LegacyCategoryId = 15, ImagePath = (string?)null },
                new { NameAr = "مفكات Conan",                   NameEn = "مفكات Conan",                   Description = "مفكات Conan",                     IsActive = true,  LegacyCategoryId = 15, ImagePath = (string?)null },
                new { NameAr = "زراديات وقطاعات Conan",        NameEn = "زراديات وقطاعات Conan",        Description = "زراديات وقطاعات Conan",          IsActive = true,  LegacyCategoryId = 15, ImagePath = (string?)null },
                new { NameAr = "الن كيه Conan",                 NameEn = "الن كيه Conan",                 Description = "الن كيه Conan",                   IsActive = true,  LegacyCategoryId = 15, ImagePath = (string?)null },
                new { NameAr = "منشار Conan",                   NameEn = "منشار Conan",                   Description = "منشار Conan",                     IsActive = true,  LegacyCategoryId = 15, ImagePath = (string?)null },
                new { NameAr = "فرد تباشيم Conan",              NameEn = "فرد تباشيم Conan",              Description = "فرد تباشيم Conan",                IsActive = true,  LegacyCategoryId = 15, ImagePath = (string?)null },
                new { NameAr = "مقصات شجر Conan",               NameEn = "مقصات شجر Conan",               Description = "مقصات شجر Conan",                 IsActive = true,  LegacyCategoryId = 15, ImagePath = (string?)null },
                new { NameAr = "فرد زراعي Conan",               NameEn = "فرد زراعي Conan",               Description = "فرد زراعي Conan",                 IsActive = true,  LegacyCategoryId = 15, ImagePath = (string?)null },
                new { NameAr = "اطقم بوكسات وطقطيقات Conan",    NameEn = "اطقم بوكسات وطقطيقات Conan",    Description = "اطقم بوكسات وطقطيقات Conan",      IsActive = true,  LegacyCategoryId = 15, ImagePath = (string?)null },
                new { NameAr = "مشحاف روبة Conan",              NameEn = "مشحاف روبة Conan",              Description = "مشحاف روبة Conan",                IsActive = true,  LegacyCategoryId = 15, ImagePath = (string?)null },
                new { NameAr = "موالج Conan",                   NameEn = "موالج Conan",                   Description = "موالج Conan",                     IsActive = true,  LegacyCategoryId = 15, ImagePath = (string?)null },
                new { NameAr = "زرفيل كامل سوبر يد خلية",      NameEn = "زرفيل كامل سوبر يد خلية",      Description = "زرفيل كامل سوبر يد خلية",        IsActive = true,  LegacyCategoryId = 6,  ImagePath = (string?)null },
                new { NameAr = " Vinyl Matt Dulux",             NameEn = " Vinyl Matt Dulux",             Description = " Vinyl Matt Dulux",               IsActive = true,  LegacyCategoryId = 16, ImagePath = (string?)null },
                new { NameAr = "Vinyl Eggshell Dulux",          NameEn = "Vinyl Eggshell Dulux",          Description = "Vinyl Eggshell Dulux",            IsActive = true,  LegacyCategoryId = 16, ImagePath = (string?)null },
                new { NameAr = "Vinyl Silk Dulux",              NameEn = "Vinyl Silk Dulux",              Description = "Vinyl Silk Dulux",                IsActive = true,  LegacyCategoryId = 16, ImagePath = (string?)null },
                new { NameAr = "امنشن جولدن",                   NameEn = "امنشن جولدن",                   Description = "امنشن جولدن",                     IsActive = true,  LegacyCategoryId = 17, ImagePath = (string?)null },
                new { NameAr = "اكشل جولدن",                    NameEn = "اكشل جولدن",                    Description = "اكشل جولدن",                      IsActive = true,  LegacyCategoryId = 17, ImagePath = (string?)null },
                new { NameAr = "فاينل سلك جولدن",               NameEn = "فاينل سلك جولدن",               Description = "فاينل سلك جولدن",                 IsActive = true,  LegacyCategoryId = 17, ImagePath = (string?)null },
                new { NameAr = "روف كوت جولدن",                 NameEn = "روف كوت جولدن",                 Description = "روف كوت جولدن",                   IsActive = true,  LegacyCategoryId = 17, ImagePath = (string?)null },
                new { NameAr = "معجونة جولدن",                  NameEn = "معجونة جولدن",                  Description = "معجونة جولدن",                    IsActive = true,  LegacyCategoryId = 17, ImagePath = (string?)null },
                new { NameAr = "ديكوري جولدن",                  NameEn = "ديكوري جولدن",                  Description = "ديكوري جولدن",                    IsActive = true,  LegacyCategoryId = 17, ImagePath = (string?)null },
                new { NameAr = "روف كوت سافيتو",                NameEn = "روف كوت سافيتو",                Description = "روف كوت سافيتو",                  IsActive = true,  LegacyCategoryId = 19, ImagePath = (string?)null },
                new { NameAr = "2K سافيتو",                     NameEn = "2K سافيتو",                     Description = "2K سافيتو",                        IsActive = true,  LegacyCategoryId = 19, ImagePath = (string?)null },
                new { NameAr = "مزيل صدأ",                      NameEn = "مزيل صدأ",                      Description = "مزيل صدأ",                        IsActive = true,  LegacyCategoryId = 20, ImagePath = (string?)null },
                new { NameAr = "سبريه رش صيني",                 NameEn = "سبريه رش صيني",                 Description = "سبريه رش صيني",                   IsActive = true,  LegacyCategoryId = 20, ImagePath = (string?)null },
                new { NameAr = "آجو ومنظف مواسير",              NameEn = "آجو ومنظف مواسير",              Description = "آجو ومنظف مواسير",                IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "ساعات غاز",                     NameEn = "ساعات غاز",                     Description = "ساعات غاز",                       IsActive = true,  LegacyCategoryId = 5,  ImagePath = (string?)null },
                new { NameAr = "مفرش نايلون",                   NameEn = "مفرش نايلون",                   Description = "مفرش نايلون",                     IsActive = true,  LegacyCategoryId = 9,  ImagePath = (string?)null },
                new { NameAr = "ديكورات روز باريس",             NameEn = "ديكورات روز باريس",             Description = "ديكورات روز باريس",               IsActive = true,  LegacyCategoryId = 21, ImagePath = (string?)null },
                new { NameAr = "عصا رول",                       NameEn = "عصا رول",                       Description = "عصا رول",                         IsActive = true,  LegacyCategoryId = 9,  ImagePath = (string?)null },
                new { NameAr = "امنشن طرق",                     NameEn = "امنشن طرق",                     Description = "امنشن طرق",                       IsActive = true,  LegacyCategoryId = 17, ImagePath = (string?)null },
                new { NameAr = "2K أبولو",                      NameEn = "2K أبولو",                      Description = "2K أبولو",                        IsActive = true,  LegacyCategoryId = 23, ImagePath = (string?)null },
                new { NameAr = "معجونة فلر القدس",              NameEn = "معجونة فلر القدس",              Description = "معجونة فلر القدس",                IsActive = true,  LegacyCategoryId = 18, ImagePath = (string?)null },
                new { NameAr = "دهانات تلكو فيا",              NameEn = "دهانات تلكو فيا",              Description = "دهانات تلكو فيا",                IsActive = true,  LegacyCategoryId = 11, ImagePath = (string?)null },
                new { NameAr = "معجونة فلر تيراكو",             NameEn = "معجونة فلر تيراكو",             Description = "معجونة فلر تيراكو",               IsActive = true,  LegacyCategoryId = 22, ImagePath = (string?)null },
                new { NameAr = "GM6 TiT",                       NameEn = "GM6 TiT",                       Description = "GM6 TiT",                         IsActive = true,  LegacyCategoryId = 11, ImagePath = (string?)null },
                new { NameAr = "زرفيل مع سلندر بدون يدين",      NameEn = "زرفيل مع سلندر بدون يدين",      Description = "زرفيل مع سلندر بدون يدين",        IsActive = true,  LegacyCategoryId = 6,  ImagePath = (string?)null },
                new { NameAr = "وصلة حبل ليد",                 NameEn = "وصلة حبل ليد",                 Description = "وصلة حبل ليد",                   IsActive = true,  LegacyCategoryId = 24, ImagePath = (string?)null },
                new { NameAr = "زوايا كورنر",                   NameEn = "زوايا كورنر",                   Description = "زوايا كورنر",                     IsActive = true,  LegacyCategoryId = 9,  ImagePath = (string?)null },
                new { NameAr = "امنشن القدس",                   NameEn = "امنشن القدس",                   Description = "امنشن القدس",                     IsActive = true,  LegacyCategoryId = 18, ImagePath = (string?)null },
                new { NameAr = "موالج بلاستيك",                 NameEn = "موالج بلاستيك",                 Description = "موالج بلاستيك",                   IsActive = true,  LegacyCategoryId = 9,  ImagePath = (string?)null },
                new { NameAr = "دهانات Elite",                  NameEn = "دهانات Elite",                  Description = "دهانات Elite",                    IsActive = true,  LegacyCategoryId = 11, ImagePath = (string?)null },
                new { NameAr = "أكواع",                         NameEn = "أكواع",                         Description = "أكواع",                           IsActive = true,  LegacyCategoryId = 25, ImagePath = (string?)null },
                new { NameAr = "فلتراب",                        NameEn = "فلتراب",                        Description = "فلتراب",                          IsActive = true,  LegacyCategoryId = 25, ImagePath = (string?)null },
                new { NameAr = "زوايا ديكور",                   NameEn = "زوايا ديكور",                   Description = "زوايا ديكور",                     IsActive = true,  LegacyCategoryId = 7,  ImagePath = (string?)null },
                new { NameAr = "سلر ولكر",                      NameEn = "سلر ولكر",                      Description = "سلر ولكر",                        IsActive = true,  LegacyCategoryId = 26, ImagePath = (string?)null },
                new { NameAr = "لمبات",                         NameEn = "لمبات",                         Description = "لمبات",                           IsActive = true,  LegacyCategoryId = 24, ImagePath = (string?)null },
                new { NameAr = "نيونات",                        NameEn = "نيونات",                        Description = "نيونات",                          IsActive = true,  LegacyCategoryId = 24, ImagePath = (string?)null },
                new { NameAr = "قلوبات",                        NameEn = "قلوبات",                        Description = "قلوبات",                          IsActive = true,  LegacyCategoryId = 24, ImagePath = (string?)null },
                new { NameAr = "علب كهرباء",                    NameEn = "علب كهرباء",                    Description = "علب كهرباء",                      IsActive = true,  LegacyCategoryId = 24, ImagePath = (string?)null },
                new { NameAr = "مناهل",                         NameEn = "مناهل",                         Description = "مناهل",                           IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "كرنيش فوم",                     NameEn = "كرنيش فوم",                     Description = "كرنيش فوم",                       IsActive = true,  LegacyCategoryId = 7,  ImagePath = (string?)null },
                new { NameAr = "رولات تغليف",                   NameEn = "رولات تغليف",                   Description = "رولات تغليف",                     IsActive = true,  LegacyCategoryId = 5,  ImagePath = (string?)null },
                new { NameAr = "فورسيلينق",                     NameEn = "فورسيلينق",                     Description = "فورسيلينق",                       IsActive = true,  LegacyCategoryId = 7,  ImagePath = (string?)null },
                new { NameAr = "كباشي بديل الخشب",              NameEn = "كباشي بديل الخشب",              Description = "كباشي بديل الخشب",                IsActive = true,  LegacyCategoryId = 7,  ImagePath = (string?)null },
                new { NameAr = "بكس",                           NameEn = "بكس",                           Description = "بكس",                             IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "برابيش دش",                    NameEn = "برابيش دش",                    Description = "برابيش دش",                      IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "مرايا حمام",                    NameEn = "مرايا حمام",                    Description = "مرايا حمام",                      IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "مضخات",                         NameEn = "مضخات",                         Description = "مضخات",                           IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "بديل البلاط",                   NameEn = "بديل البلاط",                   Description = "بديل البلاط",                     IsActive = true,  LegacyCategoryId = 13, ImagePath = (string?)null },
                new { NameAr = "سبوتات 3D مكفولة",             NameEn = "سبوتات 3D مكفولة",             Description = "سبوتات 3D مكفولة",               IsActive = true,  LegacyCategoryId = 24, ImagePath = (string?)null },
                new { NameAr = "تفلون",                         NameEn = "تفلون",                         Description = "تفلون",                           IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "كف دش مع بربيش",               NameEn = "كف دش مع بربيش",               Description = "كف دش مع بربيش",                 IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "كيزر اردني",                   NameEn = "كيزر اردني",                   Description = "كيزر اردني",                     IsActive = true,  LegacyCategoryId = 28, ImagePath = (string?)null },
                new { NameAr = "عدد نيجرة",                    NameEn = "عدد نيجرة",                    Description = "عدد نيجرة",                      IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "عوامات",                        NameEn = "عوامات",                        Description = "عوامات",                          IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "ماكنات حف",                     NameEn = "ماكنات حف",                     Description = "ماكنات حف",                       IsActive = true,  LegacyCategoryId = 29, ImagePath = (string?)null },
                new { NameAr = "مناشير",                        NameEn = "مناشير",                        Description = "مناشير",                          IsActive = true,  LegacyCategoryId = 7,  ImagePath = (string?)null },
                new { NameAr = "ليزر",                          NameEn = "ليزر",                          Description = "ليزر",                            IsActive = true,  LegacyCategoryId = 7,  ImagePath = (string?)null },
                new { NameAr = "مضخات غسيل",                    NameEn = "مضخات غسيل",                    Description = "مضخات غسيل",                      IsActive = true,  LegacyCategoryId = 30, ImagePath = (string?)null },
                new { NameAr = "فلاتر",                         NameEn = "فلاتر",                         Description = "فلاتر",                           IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "رجاج باطون",                    NameEn = "رجاج باطون",                    Description = "رجاج باطون",                      IsActive = true,  LegacyCategoryId = 29, ImagePath = (string?)null },
                new { NameAr = "أب داون",                       NameEn = "أب داون",                       Description = "أب داون",                         IsActive = true,  LegacyCategoryId = 24, ImagePath = (string?)null },
                new { NameAr = "فوم جدران رولات شمواه",        NameEn = "فوم جدران رولات شمواه",        Description = "فوم جدران رولات شمواه",          IsActive = true,  LegacyCategoryId = 13, ImagePath = (string?)null },
                new { NameAr = "فوم رولات بديل رخام",          NameEn = "فوم رولات بديل رخام",          Description = "فوم رولات بديل رخام",            IsActive = true,  LegacyCategoryId = 13, ImagePath = (string?)null },
                new { NameAr = "صوبات كهربائية",                NameEn = "صوبات كهربائية",                Description = "صوبات كهربائية",                  IsActive = true,  LegacyCategoryId = 24, ImagePath = (string?)null },
                new { NameAr = "ورق حف",                        NameEn = "ورق حف",                        Description = "ورق حف",                          IsActive = true,  LegacyCategoryId = 9,  ImagePath = (string?)null },
                new { NameAr = "شور جت",                        NameEn = "شور جت",                        Description = "شور جت",                          IsActive = true,  LegacyCategoryId = 4,  ImagePath = (string?)null },
                new { NameAr = "سكاكين حديد",                  NameEn = "سكاكين حديد",                  Description = "سكاكين حديد",                    IsActive = true,  LegacyCategoryId = 9,  ImagePath = (string?)null },
                new { NameAr = "جلد تجزيع",                    NameEn = "جلد تجزيع",                    Description = "جلد تجزيع",                      IsActive = true,  LegacyCategoryId = 9,  ImagePath = (string?)null },
                new { NameAr = "حفافات يدوية",                  NameEn = "حفافات يدوية",                  Description = "حفافات يدوية",                    IsActive = true,  LegacyCategoryId = 9,  ImagePath = (string?)null }
            };

            var subCategoryEntities = subCategories.Select(sc => new SubCategory
            {
                NameAr = sc.NameAr,
                NameEn = sc.NameEn,
                Description = sc.Description,
                ImagePath = sc.ImagePath,
                IsActive = sc.IsActive,
                CategoryId = categoryIdMap[sc.LegacyCategoryId]
            }).ToList();

            _context.SubCategories.AddRange(subCategoryEntities);
            await _context.SaveChangesAsync();
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
            var existingStores = await _context.Stores.AnyAsync();
            if (existingStores)
            {
                Console.WriteLine($"Stores already exist ({existingStores.Count} records). Skipping seeding.");
                return; // Stores already exist
            }

            Console.WriteLine("Seeding Stores...");

            var stores = new List<Store>
            {
                new Store
                {
                    NameEn = "Main Warehouse",
                    NameAr = "المستودع الرئيسي",
                    Description = "Primary storage location",
                    Address = "Main Warehouse Address",
                    Code = "WH-01",
                    IsActive = true,
                    IsMainWarehouse = true
                },
                new Store
                {
                    NameEn = "Secondary Warehouse",
                    NameAr = "المستودع الثانوي",
                    Description = "Secondary storage location",
                    Address = "Secondary Warehouse Address",
                    Code = "WH-02",
                    IsActive = true,
                    IsMainWarehouse = false
                }
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
                NameEn = "Warehousing Solutions Ltd",
                NameAr = "شركة حلول المستودعات",
                AddressEn = "123 Business Street, City Center",
                AddressAr = "شارع الأعمال 123، وسط المدينة",
                Phone = "0000000000",
                Email = "info@warehousing.com",
                Website = "www.warehousing.com",
                TaxNumber = "TAX123456789",
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

            if (adminRole == null || !permissions.Any())
            {
                return;
            }

            var existingPermissionIdsForAdmin = await _context.RolePermissions
                .Where(rp => rp.RoleId == adminRole.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var rolePermissionsToAdd = permissions
                .Where(p => !existingPermissionIdsForAdmin.Contains(p.Id))
                .Select(p => new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = p.Id
                })
                .ToList();

            if (rolePermissionsToAdd.Any())
            {
                _context.RolePermissions.AddRange(rolePermissionsToAdd);
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

            if (adminUser == null || adminRole == null)
            {
                return;
            }

            var hasAdminRole = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);

            if (!hasAdminRole)
            {
                // Check if user role already exists
                var existingUserRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);
                
                if (existingUserRole == null)
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
