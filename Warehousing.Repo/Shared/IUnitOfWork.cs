using Warehousing.Repo.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Warehousing.Repo.Shared
{
    public interface IUnitOfWork
    {
        ICategoryRepo CategoryRepo { get; }
        ICustomerRepo CustomerRepo { get; }
        IInventoryTransactionRepo InventoryTransactionRepo { get; }
        IPermissionRepo PermissionRepo { get; }
        IProductRepo ProductRepo { get; }
        IOrderRepo OrderRepo{ get; }
        IOrderItemRepo OrderItemRepo { get; }
        IOrderTypeRepo  OrderTypeRepo { get; }
        IRolePermissionRepo RolePermissionRepo { get; }
        IRoleRepo RoleRepo { get; }
        IStatusRepo StatusRepo { get; }
        ISupplierRepo SupplierRepo { get; }
        ITransactionTypeRepo TransactionTypeRepo { get; }
        IUnitRepo UnitRepo { get; }
        IUserRepo UserRepo { get; }
        IUserRoleRepo UserRoleRepo { get; }
        ICompanyRepo CompanyRepo { get; }
        IStoreRepo StoreRepo{ get; }
        IUserDeviceRepo UserDeviceRepo{ get; }
        IRoleCategoryRepo RoleCategoryRepo{ get; }
        IRoleProductRepo RoleProductRepo{ get; }
        ISubCategoryRepo SubCategoryRepo{ get; }
        IInventoryRepo InventoryRepo { get; }
        IStoreTransferRepo StoreTransferRepo { get; }
        IProductRecipeRepo ProductRecipeRepo { get; }

        Task SaveAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        IExecutionStrategy GetExecutionStrategy();
        
    }
}