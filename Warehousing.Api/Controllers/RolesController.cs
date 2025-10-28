using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Warehousing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RolesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetRoles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                // ✅ OPTIMIZED: Load only basic role information to prevent massive data loading
                var rolesQuery = _unitOfWork.RoleRepo
                    .GetAll()
                    .Select(r => new
                    {
                        r.Id,
                        r.Code,
                        r.NameEn,
                        r.NameAr,
                        r.IsActive,
                        // Load counts instead of full data to prevent performance issues
                        PermissionCount = r.RolePermissions.Count,
                        CategoryCount = r.RoleCategories.Count,
                        ProductCount = r.RoleProducts.Count,
                        SubCategoryCount = r.RoleSubCategories.Count
                    });

                if (rolesQuery == null)
                    return StatusCode(500, "Roles source is null.");

                var list = await rolesQuery.ToListAsync();

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetRoleById")]
        public async Task<IActionResult> GetRoleById(int Id)
        {
            try
            {
                // ✅ OPTIMIZED: Load role with only essential data to prevent performance issues
                var role = await _unitOfWork.RoleRepo.GetByCondition(u => u.Id == Id)
                    .FirstOrDefaultAsync();
                    
                if (role == null)
                {
                    return NotFound("Role Not Found!");
                }

                // ✅ OPTIMIZED: Load permissions separately with limit
                var permissions = await _unitOfWork.RolePermissionRepo
                    .GetByCondition(rp => rp.RoleId == Id)
                    .Include(rp => rp.Permission)
                    .Select(rp => new RolePermissionDto
                    {
                        Id = rp.Id,
                        RoleId = rp.RoleId,
                        PermissionId = rp.PermissionId,
                        NameEn = rp.Permission!.NameEn,
                        NameAr = rp.Permission!.NameAr,
                        Code = rp.Permission!.Code,
                    })
                    .Take(100) // Limit to 100 permissions to prevent massive responses
                    .ToListAsync();

                // ✅ OPTIMIZED: Load category IDs separately (not full category objects)
                var categoryIds = await _unitOfWork.RoleCategoryRepo
                    .GetByCondition(rc => rc.RoleId == Id)
                    .Select(rc => rc.CategoryId)
                    .Take(1000) // Limit to 1000 categories
                    .ToListAsync();

                // ✅ OPTIMIZED: Load product IDs separately (not full product objects)
                var productIds = await _unitOfWork.RoleProductRepo
                    .GetByCondition(rp => rp.RoleId == Id)
                    .Select(rp => rp.ProductId)
                    .Take(5000) // Limit to 5000 products
                    .ToListAsync();

                // ✅ OPTIMIZED: Load subcategory IDs separately (not full subcategory objects)
                var subCategoryIds = await _unitOfWork.RoleSubCategoryRepo
                    .GetByCondition(rsc => rsc.RoleId == Id)
                    .Select(rsc => rsc.SubCategoryId)
                    .Take(1000) // Limit to 1000 subcategories
                    .ToListAsync();

                var roleDto = new RoleDto
                {
                    Id = role.Id,
                    Code = role.Code,
                    NameAr = role.NameAr,
                    NameEn = role.NameEn,
                    Permissions = permissions,
                    CategoryIds = categoryIds,
                    ProductIds = productIds,
                    SubCategoryIds = subCategoryIds
                };

                return Ok(roleDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost]
        [Route("SaveRole")]
        public async Task<IActionResult> SaveRole([FromBody] RoleCreateUpdateDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Role Model is null!");
                }

                // Check if role exists
                var isRoleExist = await _unitOfWork.RoleRepo
                    .GetByCondition(r => (r.Code.Equals(dto.Code) || r.NameEn.Equals(dto.NameEn) || r.NameAr.Equals(dto.NameAr)) && r.Id != dto.Id)
                    .FirstOrDefaultAsync();
                if (isRoleExist != null)
                    return BadRequest("Role already exists.");

                if (dto.Id > 0)
                {
                    var roleToUpdate = await _unitOfWork.RoleRepo
                        .GetAll()
                        .Include(r => r.RolePermissions)
                        .Include(r => r.RoleCategories)
                        .Include(r => r.RoleProducts)
                        .Include(r => r.RoleSubCategories)
                        .FirstOrDefaultAsync(r => r.Id == dto.Id);

                    if (roleToUpdate == null)
                        return NotFound("This role is not exist!");

                    // Update basic role info
                    roleToUpdate.Code = dto.Code;
                    roleToUpdate.NameEn = dto.NameEn;
                    roleToUpdate.NameAr = dto.NameAr;

                    // Get permission IDs from DTO
                    var permissionIds = await _unitOfWork.PermissionRepo.GetAll()
                        .Where(p => dto.PermissionCodes.Contains(p.Code))
                        .Select(p => p.Id)
                        .ToListAsync();

                    // 🔁 Sync RolePermissions
                    await SyncRolePermissions(roleToUpdate, permissionIds);

                    // Get category IDs from DTO
                    var categoryIds = dto.CategoryIds ?? new List<int>();

                    // 🔁 Sync RoleCategories
                    await SyncRoleCategories(roleToUpdate, categoryIds);

                    // Get product IDs from DTO
                    var productIds = dto.ProductIds ?? new List<int>();

                    // 🔁 Sync RoleProducts
                    await SyncRoleProducts(roleToUpdate, productIds);

                    // Get sub-category IDs from DTO
                    var subCategoryIds = dto.SubCategoryIds ?? new List<int>();

                    // 🔁 Sync RoleSubCategories
                    await SyncRoleSubCategories(roleToUpdate, subCategoryIds);

                    // Save changes
                    await _unitOfWork.RoleRepo.UpdateAsync(roleToUpdate);
                    await _unitOfWork.SaveAsync();

                    return Ok(roleToUpdate);
                }
                else
                {
                    var role = new Role
                    {
                        Code = dto.Code,
                        NameEn = dto.NameEn,
                        NameAr = dto.NameAr
                    };

                    // Add permissions
                    if (dto.PermissionCodes?.Any() == true)
                    {
                        var permissions = await _unitOfWork.PermissionRepo.GetAll()
                            .Where(p => dto.PermissionCodes.Contains(p.Code))
                            .ToListAsync();

                        role.RolePermissions = permissions.Select(p => new RolePermission
                        {
                            PermissionId = p.Id
                        }).ToList();
                    }

                    // Add categories
                    if (dto.CategoryIds?.Any() == true)
                    {
                        role.RoleCategories = dto.CategoryIds.Select(c => new RoleCategory
                        {
                            CategoryId = c
                        }).ToList();
                    }

                    // Add products
                    if (dto.ProductIds?.Any() == true)
                    {
                        role.RoleProducts = dto.ProductIds.Select(p => new RoleProduct
                        {
                            ProductId = p
                        }).ToList();
                    }

                    // Add sub-categories
                    if (dto.SubCategoryIds?.Any() == true)
                    {
                        role.RoleSubCategories = dto.SubCategoryIds.Select(sc => new RoleSubCategory
                        {
                            SubCategoryId = sc
                        }).ToList();
                    }

                    var result = await _unitOfWork.RoleRepo.CreateAsync(role);
                    if (result != null)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        return BadRequest("Error while adding Role");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _unitOfWork.RoleRepo.GetAll()
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.Id == id);
            if (role == null) return NotFound();

            var isAssignedToAnyUser = await _unitOfWork.UserRoleRepo.GetAll().AnyAsync(ur => ur.RoleId == id);
            if (isAssignedToAnyUser)
                return BadRequest("Cannot delete a role assigned to users.");

            await _unitOfWork.RolePermissionRepo.DeleteRange(role.RolePermissions.ToList());
            await _unitOfWork.RoleRepo.DeleteAsync(role);

            return Ok();
        }

        [HttpPost("AssignRoleToUser")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleDto dto)
        {
            var user = await _unitOfWork.UserRepo.GetAll()
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == dto.UserId);

            if (user == null) return NotFound("User not found.");

            if (user.UserRoles.Any(ur => ur.RoleId == dto.RoleId))
                return BadRequest("User already has this role.");

            user.UserRoles.Add(new UserRole
            {
                RoleId = dto.RoleId,
                UserId = user.Id
            });

            var result = await _unitOfWork.UserRepo.UpdateAsync(user);
            return Ok(result);
        }


        //---------------Helpers
        private async Task SyncRolePermissions(Role role, List<int> newPermissionIds)
        {
            var existingPermissions = role.RolePermissions.Select(rp => rp.PermissionId).ToList();

            var toAdd = newPermissionIds
                .Where(id => !existingPermissions.Contains(id))
                .ToList();

            var toRemove = role.RolePermissions
                .Where(rp => !newPermissionIds.Contains(rp.PermissionId))
                .ToList();

            // Add new permissions
            if (toAdd.Any())
            {
                var newRolePermissions = toAdd.Select(id => new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = id
                }).ToList();

                role.RolePermissions.AddRange(newRolePermissions);
            }

            // Remove outdated permissions
            if (toRemove.Any())
            {
                await _unitOfWork.RolePermissionRepo.DeleteRange(toRemove);
            }
        }

        private async Task SyncRoleCategories(Role role, List<int> newCategoryIds)
        {
            var existingCategories = role.RoleCategories.Select(rc => rc.CategoryId).ToList();

            var toAdd = newCategoryIds
                .Where(id => !existingCategories.Contains(id))
                .ToList();

            var toRemove = role.RoleCategories
                .Where(rc => !newCategoryIds.Contains(rc.CategoryId))
                .ToList();

            // Add new categories
            if (toAdd.Any())
            {
                var newRoleCategories = toAdd.Select(id => new RoleCategory
                {
                    RoleId = role.Id,
                    CategoryId = id
                }).ToList();

                role.RoleCategories.AddRange(newRoleCategories);
            }

            // Remove outdated categories
            if (toRemove.Any())
            {
                await _unitOfWork.RoleCategoryRepo.DeleteRange(toRemove);
            }
        }

        private async Task SyncRoleProducts(Role role, List<int> newProductIds)
        {
            var existingProducts = role.RoleProducts.Select(rp => rp.ProductId).ToList();

            var toAdd = newProductIds
                .Where(id => !existingProducts.Contains(id))
                .ToList();

            var toRemove = role.RoleProducts
                .Where(rp => !newProductIds.Contains(rp.ProductId))
                .ToList();

            // Add new products
            if (toAdd.Any())
            {
                var newRoleProducts = toAdd.Select(id => new RoleProduct
                {
                    RoleId = role.Id,
                    ProductId = id
                }).ToList();

                role.RoleProducts.AddRange(newRoleProducts);
            }

            // Remove outdated products
            if (toRemove.Any())
            {
                await _unitOfWork.RoleProductRepo.DeleteRange(toRemove);
            }
        }

        private async Task SyncRoleSubCategories(Role role, List<int> newSubCategoryIds)
        {
            var existingSubCategories = role.RoleSubCategories.Select(rsc => rsc.SubCategoryId).ToList();

            var toAdd = newSubCategoryIds
                .Where(id => !existingSubCategories.Contains(id))
                .ToList();

            var toRemove = role.RoleSubCategories
                .Where(rsc => !newSubCategoryIds.Contains(rsc.SubCategoryId))
                .ToList();

            // Add new sub-categories
            if (toAdd.Any())
            {
                var newRoleSubCategories = toAdd.Select(id => new RoleSubCategory
                {
                    RoleId = role.Id,
                    SubCategoryId = id
                }).ToList();

                role.RoleSubCategories.AddRange(newRoleSubCategories);
            }

            // Remove outdated sub-categories
            if (toRemove.Any())
            {
                await _unitOfWork.RoleSubCategoryRepo.DeleteRange(toRemove);
            }
        }
    }
}