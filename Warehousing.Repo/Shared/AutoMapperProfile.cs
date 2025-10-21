using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using AutoMapper;

namespace Warehousing.Repo.Shared
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, LoginDto>();
            CreateMap<UsersDto, User>().ReverseMap();
            CreateMap<UserDevice, UserDeviceDto>().ReverseMap();

            // DTO -> Entity
            CreateMap<RoleCreateUpdateDto, Role>()
                .ForMember(dest => dest.RolePermissions, opt => opt.MapFrom(src =>
                    src.RolePermissionIds.Select(id => new RolePermission
                    {
                        Permission = new Permission { Id = id } // or just assign PermissionId if known
                    })
                ))
                .ForMember(dest => dest.RoleCategories, opt => opt.MapFrom(src => src.CategoryIds.Select(c => new RoleCategory
                {
                    CategoryId = c,
                    RoleId = src.Id
                })))
                .ForMember(dest => dest.RoleProducts, opt => opt.MapFrom(src => src.ProductIds.Select(p => new RoleProduct
                {
                    ProductId = p,
                    RoleId = src.Id
                })));

            CreateMap<User, UsersDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role)));

            CreateMap<Role, RoleDto>();

            CreateMap<Order, OrderDto>().ReverseMap();
            CreateMap<OrderItem, OrderItemDto>().ReverseMap();
            CreateMap<OrderType, OrderTypeDto>().ReverseMap();

            CreateMap<CustomerDto, Customer>();
            CreateMap<SupplierDto, Supplier>();

            CreateMap<StoreDto, Store>().ReverseMap();
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Product, ProductDto>();
            CreateMap<UserDevice, UserDeviceDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<SubCategory, SubCategoryDto>().ReverseMap();
            CreateMap<Unit, UnitDto>().ReverseMap();

            // New entity mappings
            CreateMap<Inventory, InventoryDto>().ReverseMap();
            CreateMap<InventoryTransaction, InventoryTransactionDto>()
                .ForMember(dest => dest.ProductNameAr, opt => opt.MapFrom(src => src.Product.NameAr))
                .ForMember(dest => dest.ProductNameEn, opt => opt.MapFrom(src => src.Product.NameEn))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product.Code))
                .ForMember(dest => dest.TransactionTypeNameAr, opt => opt.MapFrom(src => src.TransactionType.NameAr))
                .ForMember(dest => dest.TransactionTypeNameEn, opt => opt.MapFrom(src => src.TransactionType.NameEn))
                .ForMember(dest => dest.TransactionTypeCode, opt => opt.MapFrom(src => src.TransactionType.Code))
                .ForMember(dest => dest.StoreNameAr, opt => opt.MapFrom(src => src.Store.NameAr))
                .ForMember(dest => dest.StoreNameEn, opt => opt.MapFrom(src => src.Store.NameEn))
                .ForMember(dest => dest.StoreCode, opt => opt.MapFrom(src => src.Store.Code))
                .ReverseMap();

            CreateMap<StoreTransfer, StoreTransferDto>()
                .ForMember(dest => dest.FromStoreNameAr, opt => opt.MapFrom(src => src.FromStore.NameAr))
                .ForMember(dest => dest.FromStoreNameEn, opt => opt.MapFrom(src => src.FromStore.NameEn))
                .ForMember(dest => dest.FromStoreCode, opt => opt.MapFrom(src => src.FromStore.Code))
                .ForMember(dest => dest.ToStoreNameAr, opt => opt.MapFrom(src => src.ToStore.NameAr))
                .ForMember(dest => dest.ToStoreNameEn, opt => opt.MapFrom(src => src.ToStore.NameEn))
                .ForMember(dest => dest.ToStoreCode, opt => opt.MapFrom(src => src.ToStore.Code))
                .ForMember(dest => dest.StatusNameAr, opt => opt.MapFrom(src => src.Status.NameAr))
                .ForMember(dest => dest.StatusNameEn, opt => opt.MapFrom(src => src.Status.NameEn))
                .ForMember(dest => dest.StatusCode, opt => opt.MapFrom(src => src.Status.Code))
                .ReverseMap();

            CreateMap<StoreTransferItem, StoreTransferItemDto>()
                .ForMember(dest => dest.ProductNameAr, opt => opt.MapFrom(src => src.Product.NameAr))
                .ForMember(dest => dest.ProductNameEn, opt => opt.MapFrom(src => src.Product.NameEn))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product.Code))
                .ReverseMap();

            CreateMap<ProductRecipe, ProductRecipeDto>()
                .ForMember(dest => dest.ParentProductNameAr, opt => opt.MapFrom(src => src.ParentProduct.NameAr))
                .ForMember(dest => dest.ParentProductNameEn, opt => opt.MapFrom(src => src.ParentProduct.NameEn))
                .ForMember(dest => dest.ParentProductCode, opt => opt.MapFrom(src => src.ParentProduct.Code))
                .ForMember(dest => dest.ComponentProductNameAr, opt => opt.MapFrom(src => src.ComponentProduct.NameAr))
                .ForMember(dest => dest.ComponentProductNameEn, opt => opt.MapFrom(src => src.ComponentProduct.NameEn))
                .ForMember(dest => dest.ComponentProductCode, opt => opt.MapFrom(src => src.ComponentProduct.Code))
                .ReverseMap();
        }
    }
}