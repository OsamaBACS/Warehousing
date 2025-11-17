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
                new Permission { Id = 1012, Code = "WORK_OUTSIDE_WORKING_HOURS", NameEn = "Work Outside Working Hours", NameAr = "العمل خارج ساعات العمل" },
                
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
                // Ensure all new permissions have Id = 0 to let database auto-generate IDs
                // (Identity column requires no explicit ID values)
                foreach (var perm in permissionsToAdd)
                {
                    perm.Id = 0;
                }
                
                _context.Permissions.AddRange(permissionsToAdd);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Seeded {permissionsToAdd.Count} new permissions.");
            }
            else
            {
                Console.WriteLine("All permissions already exist in database.");
            }
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
            // If categories already contain the new data (identified by a key category), skip reseeding
            var hasNewData = await _context.Categories.AnyAsync(c => c.NameAr == "بديل الخشب");
            if (hasNewData)
            {
                return;
            }

            // Remove old seeded data (from initial migration) so we can insert the new dataset
            var existingSubCategories = await _context.SubCategories.ToListAsync();
            if (existingSubCategories.Any())
            {
                _context.SubCategories.RemoveRange(existingSubCategories);
            }

            var existingCategories = await _context.Categories.ToListAsync();
            if (existingCategories.Any())
            {
                _context.Categories.RemoveRange(existingCategories);
            }

            await _context.SaveChangesAsync();

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
            var existingStores = await _context.Stores.AnyAsync();
            if (existingStores)
            {
                return; // Stores already exist
            }

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
                var userRole = new UserRole
                {
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
                return; // Printer configurations already exist
            }

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

            // Assign A4 config to Admin and Warehouse Manager roles
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "ADMIN");
            var warehouseManagerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "WAREHOUSE_MANAGER");
            
            if (adminRole != null)
            {
                adminRole.PrinterConfigurationId = a4Config.Id;
            }
            if (warehouseManagerRole != null)
            {
                warehouseManagerRole.PrinterConfigurationId = a4Config.Id;
            }

            // Assign POS config to Sales Manager role
            var salesManagerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "SALES_MANAGER");
            if (salesManagerRole != null)
            {
                salesManagerRole.PrinterConfigurationId = posConfig.Id;
            }

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
