using Warehousing.Data.Context;
using Warehousing.Repo.Classes;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Services;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Warehousing.Repo.Shared
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly WarehousingContext _context;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _config;
        private readonly IActivityLoggingService _activityLoggingService;
        private readonly IFileStorageService? _fileStorageService;

        public UnitOfWork(WarehousingContext context, ILoggerFactory loggerFactory, IConfiguration config, IActivityLoggingService activityLoggingService, IFileStorageService? fileStorageService = null)
        {
            _context = context;
            _loggerFactory = loggerFactory;
            _config = config;
            _activityLoggingService = activityLoggingService;
            _fileStorageService = fileStorageService;
        }

        public ICategoryRepo CategoryRepo => new CategoryRepo(_context, _loggerFactory.CreateLogger<CategoryRepo>(), _config);

        public ICustomerRepo CustomerRepo => new CustomerRepo(_context, _loggerFactory.CreateLogger<CustomerRepo>(), _config);

        public IInventoryTransactionRepo InventoryTransactionRepo => new InventoryTransactionRepo(_context, _loggerFactory.CreateLogger<InventoryTransactionRepo>(), _config);

        public IPermissionRepo PermissionRepo => new PermissionRepo(_context, _loggerFactory.CreateLogger<PermissionRepo>(), _config);

        public IProductRepo ProductRepo => new ProductRepo(_context, _loggerFactory.CreateLogger<ProductRepo>(), _config, _fileStorageService);

        public IOrderRepo OrderRepo => new OrderRepo(_context, _loggerFactory.CreateLogger<OrderRepo>(), _config);

        public IOrderItemRepo OrderItemRepo => new OrderItemRepo(_context, _loggerFactory.CreateLogger<OrderItemRepo>(), _config);
        
        public IOrderTypeRepo OrderTypeRepo => new OrderTypeRepo(_context, _loggerFactory.CreateLogger<OrderTypeRepo>(), _config);

        public IRolePermissionRepo RolePermissionRepo => new RolePermissionRepo(_context, _loggerFactory.CreateLogger<RolePermissionRepo>(), _config);

        public IRoleRepo RoleRepo => new RoleRepo(_context, _loggerFactory.CreateLogger<RoleRepo>(), _config);

        public IStatusRepo StatusRepo => new StatusRepo(_context, _loggerFactory.CreateLogger<StatusRepo>(), _config);

        public ITransactionTypeRepo TransactionTypeRepo => new TransactionTypeRepo(_context, _loggerFactory.CreateLogger<TransactionTypeRepo>(), _config);

        public IUnitRepo UnitRepo => new UnitRepo(_context, _loggerFactory.CreateLogger<UnitRepo>(), _config);

        public IUserRepo UserRepo => new UserRepo(_context, _loggerFactory.CreateLogger<UserRepo>(), _config, _activityLoggingService, new WorkingHoursRepo(_context, _loggerFactory.CreateLogger<WorkingHoursRepo>()));

        public IUserRoleRepo UserRoleRepo => new UserRoleRepo(_context, _loggerFactory.CreateLogger<UserRoleRepo>(), _config);

        public ISupplierRepo SupplierRepo => new SupplierRepo(_context, _loggerFactory.CreateLogger<SupplierRepo>(), _config);

        public ICompanyRepo CompanyRepo => new CompanyRepo(_context, _loggerFactory.CreateLogger<CompanyRepo>(), _config, _fileStorageService);

        public IStoreRepo StoreRepo => new StoreRepo(_context, _loggerFactory.CreateLogger<StoreRepo>(), _config);

        public IUserDeviceRepo UserDeviceRepo => new UserDeviceRepo(_context, _loggerFactory.CreateLogger<UserDeviceRepo>(), _config);

        public IRoleCategoryRepo RoleCategoryRepo => new RoleCategoryRepo(_context, _loggerFactory.CreateLogger<RoleCategoryRepo>(), _config);

        public IRoleProductRepo RoleProductRepo => new RoleProductRepo(_context, _loggerFactory.CreateLogger<RoleProductRepo>(), _config);
        
        public IRoleSubCategoryRepo RoleSubCategoryRepo => new RoleSubCategoryRepo(_context, _loggerFactory.CreateLogger<RoleSubCategoryRepo>(), _config);
        
        public ISubCategoryRepo SubCategoryRepo => new SubCategoryRepo(_context, _loggerFactory.CreateLogger<SubCategoryRepo>(), _config);

        public IInventoryRepo InventoryRepo => new InventoryRepo(_context, _loggerFactory.CreateLogger<InventoryRepo>(), _config);

        public IStoreTransferRepo StoreTransferRepo => new StoreTransferRepo(_context, _loggerFactory.CreateLogger<StoreTransferRepo>(), _config);

        public IProductRecipeRepo ProductRecipeRepo => new ProductRecipeRepo(_context, _loggerFactory.CreateLogger<ProductRecipeRepo>(), _config);
        
        // Variants and Modifiers
        public IProductVariantRepo ProductVariantRepo => new ProductVariantRepo(_context, _loggerFactory.CreateLogger<ProductVariantRepo>(), _config);
        public IProductModifierRepo ProductModifierRepo => new ProductModifierRepo(_context, _loggerFactory.CreateLogger<ProductModifierRepo>(), _config);
        public IProductModifierOptionRepo ProductModifierOptionRepo => new ProductModifierOptionRepo(_context, _loggerFactory.CreateLogger<ProductModifierOptionRepo>(), _config);
        public IProductModifierGroupRepo ProductModifierGroupRepo => new ProductModifierGroupRepo(_context, _loggerFactory.CreateLogger<ProductModifierGroupRepo>(), _config);
        public IOrderItemModifierRepo OrderItemModifierRepo => new OrderItemModifierRepo(_context, _loggerFactory.CreateLogger<OrderItemModifierRepo>(), _config);
        
        // User Activity Logging
        public IUserActivityLogRepo UserActivityLogRepo => new UserActivityLogRepo(_context, _loggerFactory.CreateLogger<UserActivityLogRepo>());
        
        // Working Hours Configuration
        public IWorkingHoursRepo WorkingHoursRepo => new WorkingHoursRepo(_context, _loggerFactory.CreateLogger<WorkingHoursRepo>());

        // Context for direct database access
        public WarehousingContext Context => _context;

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync(); // <-- This will use your overridden SaveChangesAsync
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public IExecutionStrategy GetExecutionStrategy()
        {
            return _context.Database.CreateExecutionStrategy();
        }
    }
}