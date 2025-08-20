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
        }
    }
}