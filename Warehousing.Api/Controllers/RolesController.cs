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
                Console.WriteLine($"[DEBUG] SaveRole called with ID: {dto?.Id}");
                
                if (dto == null)
                {
                    Console.WriteLine("[DEBUG] DTO is null");
                    return BadRequest("Role Model is null!");
                }

                Console.WriteLine($"[DEBUG] DTO received - PermissionCodes: {dto.PermissionCodes?.Count ?? 0}, CategoryIds: {dto.CategoryIds?.Count ?? 0}, ProductIds: {dto.ProductIds?.Count ?? 0}, SubCategoryIds: {dto.SubCategoryIds?.Count ?? 0}");

                // Check if role exists
                Console.WriteLine("[DEBUG] Checking if role exists...");
                var isRoleExist = await _unitOfWork.RoleRepo
                    .GetByCondition(r => (r.Code.Equals(dto.Code) || r.NameEn.Equals(dto.NameEn) || r.NameAr.Equals(dto.NameAr)) && r.Id != dto.Id)
                    .FirstOrDefaultAsync();
                if (isRoleExist != null)
                {
                    Console.WriteLine("[DEBUG] Role already exists");
                    return BadRequest("Role already exists.");
                }

                if (dto.Id > 0)
                {
                    Console.WriteLine("[DEBUG] Updating existing role...");
                    var roleToUpdate = await _unitOfWork.RoleRepo
                        .GetAll()
                        .FirstOrDefaultAsync(r => r.Id == dto.Id);

                    if (roleToUpdate == null)
                    {
                        Console.WriteLine("[DEBUG] Role not found");
                        return NotFound("This role is not exist!");
                    }

                    Console.WriteLine($"[DEBUG] Role found - ID: {roleToUpdate.Id}, Code: {roleToUpdate.Code}");

                    // Update basic role info
                    roleToUpdate.Code = dto.Code;
                    roleToUpdate.NameEn = dto.NameEn;
                    roleToUpdate.NameAr = dto.NameAr;

                    // ✅ OPTIMIZED: Get permission IDs from DTO with null check and performance optimization
                    Console.WriteLine("[DEBUG] Getting permission IDs...");
                    var permissionIds = new List<int>();
                    if (dto.PermissionCodes != null && dto.PermissionCodes.Any())
                    {
                        // Use HashSet for faster Contains() operations
                        var permissionCodeSet = new HashSet<string>(dto.PermissionCodes);
                        permissionIds = await _unitOfWork.PermissionRepo.GetAll()
                            .Where(p => permissionCodeSet.Contains(p.Code))
                            .Select(p => p.Id)
                            .ToListAsync();
                    }
                    Console.WriteLine($"[DEBUG] Found {permissionIds.Count} permission IDs");

                    // ✅ OPTIMIZED: Use direct SQL for large datasets to avoid EF overhead
                    var categoryIds = dto.CategoryIds ?? new List<int>();
                    var productIds = dto.ProductIds ?? new List<int>();
                    var subCategoryIds = dto.SubCategoryIds ?? new List<int>();

                    // Check if we have large datasets that need SQL optimization
                    bool useSqlOptimization = permissionIds.Count > 50 || categoryIds.Count > 50 || 
                                            productIds.Count > 50 || subCategoryIds.Count > 50;

                    Console.WriteLine($"[DEBUG] Use SQL optimization: {useSqlOptimization} (permissions: {permissionIds.Count}, categories: {categoryIds.Count}, products: {productIds.Count}, subcategories: {subCategoryIds.Count})");

                    if (useSqlOptimization)
                    {
                        Console.WriteLine("[DEBUG] Using SQL optimization for large datasets...");
                        // Use direct SQL operations for large datasets
                        await SyncRolePermissionsWithSql(roleToUpdate.Id, permissionIds);
                        Console.WriteLine("[DEBUG] Permissions synced with SQL");
                        await SyncRoleCategoriesWithSql(roleToUpdate.Id, categoryIds);
                        Console.WriteLine("[DEBUG] Categories synced with SQL");
                        await SyncRoleProductsWithSql(roleToUpdate.Id, productIds);
                        Console.WriteLine("[DEBUG] Products synced with SQL");
                        await SyncRoleSubCategoriesWithSql(roleToUpdate.Id, subCategoryIds);
                        Console.WriteLine("[DEBUG] SubCategories synced with SQL");
                    }
                    else
                    {
                        Console.WriteLine("[DEBUG] Using EF for small datasets...");
                        // Use EF for small datasets - pass roleId instead of role object
                        await SyncRolePermissionsWithId(roleToUpdate.Id, permissionIds);
                        await SyncRoleCategoriesWithId(roleToUpdate.Id, categoryIds);
                        await SyncRoleProductsWithId(roleToUpdate.Id, productIds);
                        await SyncRoleSubCategoriesWithId(roleToUpdate.Id, subCategoryIds);
                        
                        // Save changes
                        await _unitOfWork.SaveAsync();
                        Console.WriteLine("[DEBUG] Changes saved with EF");
                    }

                    Console.WriteLine("[DEBUG] SaveRole completed successfully");
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
        // ✅ NEW: EF-based sync methods that work with role ID (no Include needed)
        private async Task SyncRolePermissionsWithId(int roleId, List<int> newPermissionIds)
        {
            // Get current permission IDs directly from database
            var currentPermissionIds = await _unitOfWork.Context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            // Find permissions to add and remove
            var toAdd = newPermissionIds.Except(currentPermissionIds).ToList();
            var toRemove = currentPermissionIds.Except(newPermissionIds).ToList();

            // Add new permissions
            if (toAdd.Any())
            {
                var newRolePermissions = toAdd.Select(permissionId => new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                }).ToList();
                _unitOfWork.Context.RolePermissions.AddRange(newRolePermissions);
            }

            // Remove old permissions
            if (toRemove.Any())
            {
                var toRemoveEntities = await _unitOfWork.Context.RolePermissions
                    .Where(rp => rp.RoleId == roleId && toRemove.Contains(rp.PermissionId))
                    .ToListAsync();
                _unitOfWork.Context.RolePermissions.RemoveRange(toRemoveEntities);
            }
        }

        private async Task SyncRoleCategoriesWithId(int roleId, List<int> newCategoryIds)
        {
            // Get current category IDs directly from database
            var currentCategoryIds = await _unitOfWork.Context.RoleCategories
                .Where(rc => rc.RoleId == roleId)
                .Select(rc => rc.CategoryId)
                .ToListAsync();

            // Find categories to add and remove
            var toAdd = newCategoryIds.Except(currentCategoryIds).ToList();
            var toRemove = currentCategoryIds.Except(newCategoryIds).ToList();

            // Add new categories
            if (toAdd.Any())
            {
                var newRoleCategories = toAdd.Select(categoryId => new RoleCategory
                {
                    RoleId = roleId,
                    CategoryId = categoryId
                }).ToList();
                _unitOfWork.Context.RoleCategories.AddRange(newRoleCategories);
            }

            // Remove old categories
            if (toRemove.Any())
            {
                var toRemoveEntities = await _unitOfWork.Context.RoleCategories
                    .Where(rc => rc.RoleId == roleId && toRemove.Contains(rc.CategoryId))
                    .ToListAsync();
                _unitOfWork.Context.RoleCategories.RemoveRange(toRemoveEntities);
            }
        }

        private async Task SyncRoleProductsWithId(int roleId, List<int> newProductIds)
        {
            // Get current product IDs directly from database
            var currentProductIds = await _unitOfWork.Context.RoleProducts
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.ProductId)
                .ToListAsync();

            // Find products to add and remove
            var toAdd = newProductIds.Except(currentProductIds).ToList();
            var toRemove = currentProductIds.Except(newProductIds).ToList();

            // Add new products
            if (toAdd.Any())
            {
                var newRoleProducts = toAdd.Select(productId => new RoleProduct
                {
                    RoleId = roleId,
                    ProductId = productId
                }).ToList();
                _unitOfWork.Context.RoleProducts.AddRange(newRoleProducts);
            }

            // Remove old products
            if (toRemove.Any())
            {
                var toRemoveEntities = await _unitOfWork.Context.RoleProducts
                    .Where(rp => rp.RoleId == roleId && toRemove.Contains(rp.ProductId))
                    .ToListAsync();
                _unitOfWork.Context.RoleProducts.RemoveRange(toRemoveEntities);
            }
        }

        private async Task SyncRoleSubCategoriesWithId(int roleId, List<int> newSubCategoryIds)
        {
            // Get current subcategory IDs directly from database
            var currentSubCategoryIds = await _unitOfWork.Context.RoleSubCategories
                .Where(rsc => rsc.RoleId == roleId)
                .Select(rsc => rsc.SubCategoryId)
                .ToListAsync();

            // Find subcategories to add and remove
            var toAdd = newSubCategoryIds.Except(currentSubCategoryIds).ToList();
            var toRemove = currentSubCategoryIds.Except(newSubCategoryIds).ToList();

            // Add new subcategories
            if (toAdd.Any())
            {
                var newRoleSubCategories = toAdd.Select(subCategoryId => new RoleSubCategory
                {
                    RoleId = roleId,
                    SubCategoryId = subCategoryId
                }).ToList();
                _unitOfWork.Context.RoleSubCategories.AddRange(newRoleSubCategories);
            }

            // Remove old subcategories
            if (toRemove.Any())
            {
                var toRemoveEntities = await _unitOfWork.Context.RoleSubCategories
                    .Where(rsc => rsc.RoleId == roleId && toRemove.Contains(rsc.SubCategoryId))
                    .ToListAsync();
                _unitOfWork.Context.RoleSubCategories.RemoveRange(toRemoveEntities);
            }
        }

        // ✅ OLD: EF-based sync methods that require loaded related entities (kept for backward compatibility)
        private Task SyncRolePermissions(Role role, List<int> newPermissionIds)
        {
            // ✅ OPTIMIZED: Use HashSet for O(1) lookups instead of O(n) Contains()
            var existingPermissionIds = new HashSet<int>(role.RolePermissions.Select(rp => rp.PermissionId));
            var newPermissionIdSet = new HashSet<int>(newPermissionIds);

            // Find permissions to add (in new set but not in existing)
            var toAdd = newPermissionIds.Where(id => !existingPermissionIds.Contains(id)).ToList();

            // Find permissions to remove (in existing but not in new set)
            var toRemove = role.RolePermissions.Where(rp => !newPermissionIdSet.Contains(rp.PermissionId)).ToList();

            // Add new permissions in batch
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
                // ✅ OPTIMIZED: Remove from collection first, then mark for deletion
                foreach (var item in toRemove)
                {
                    role.RolePermissions.Remove(item);
                }
                // Mark entities for deletion without calling SaveChangesAsync
                _unitOfWork.Context.RolePermissions.RemoveRange(toRemove);
            }
            return Task.CompletedTask;
        }

        private Task SyncRoleCategories(Role role, List<int> newCategoryIds)
        {
            // ✅ OPTIMIZED: Use HashSet for O(1) lookups instead of O(n) Contains()
            var existingCategoryIds = new HashSet<int>(role.RoleCategories.Select(rc => rc.CategoryId));
            var newCategoryIdSet = new HashSet<int>(newCategoryIds);

            // Find categories to add (in new set but not in existing)
            var toAdd = newCategoryIds.Where(id => !existingCategoryIds.Contains(id)).ToList();

            // Find categories to remove (in existing but not in new set)
            var toRemove = role.RoleCategories.Where(rc => !newCategoryIdSet.Contains(rc.CategoryId)).ToList();

            // Add new categories in batch
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
                // ✅ OPTIMIZED: Remove from collection first, then mark for deletion
                foreach (var item in toRemove)
                {
                    role.RoleCategories.Remove(item);
                }
                // Mark entities for deletion without calling SaveChangesAsync
                _unitOfWork.Context.RoleCategories.RemoveRange(toRemove);
            }
            return Task.CompletedTask;
        }

        private Task SyncRoleProducts(Role role, List<int> newProductIds)
        {
            // ✅ OPTIMIZED: Use HashSet for O(1) lookups instead of O(n) Contains()
            var existingProductIds = new HashSet<int>(role.RoleProducts.Select(rp => rp.ProductId));
            var newProductIdSet = new HashSet<int>(newProductIds);

            // Find products to add (in new set but not in existing)
            var toAdd = newProductIds.Where(id => !existingProductIds.Contains(id)).ToList();

            // Find products to remove (in existing but not in new set)
            var toRemove = role.RoleProducts.Where(rp => !newProductIdSet.Contains(rp.ProductId)).ToList();

            // Add new products in batch
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
                // ✅ OPTIMIZED: Remove from collection first, then mark for deletion
                foreach (var item in toRemove)
                {
                    role.RoleProducts.Remove(item);
                }
                // Mark entities for deletion without calling SaveChangesAsync
                _unitOfWork.Context.RoleProducts.RemoveRange(toRemove);
            }
            return Task.CompletedTask;
        }

        private Task SyncRoleSubCategories(Role role, List<int> newSubCategoryIds)
        {
            // ✅ OPTIMIZED: Use HashSet for O(1) lookups instead of O(n) Contains()
            var existingSubCategoryIds = new HashSet<int>(role.RoleSubCategories.Select(rsc => rsc.SubCategoryId));
            var newSubCategoryIdSet = new HashSet<int>(newSubCategoryIds);

            // Find sub-categories to add (in new set but not in existing)
            var toAdd = newSubCategoryIds.Where(id => !existingSubCategoryIds.Contains(id)).ToList();

            // Find sub-categories to remove (in existing but not in new set)
            var toRemove = role.RoleSubCategories.Where(rsc => !newSubCategoryIdSet.Contains(rsc.SubCategoryId)).ToList();

            // Add new sub-categories in batch
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
                // ✅ OPTIMIZED: Remove from collection first, then mark for deletion
                foreach (var item in toRemove)
                {
                    role.RoleSubCategories.Remove(item);
                }
                // Mark entities for deletion without calling SaveChangesAsync
                _unitOfWork.Context.RoleSubCategories.RemoveRange(toRemove);
            }
            return Task.CompletedTask;
        }

        // ✅ SQL-based sync methods for large datasets
        private async Task SyncRolePermissionsWithSql(int roleId, List<int> newPermissionIds)
        {
            Console.WriteLine($"[DEBUG] SyncRolePermissionsWithSql called with roleId: {roleId}, permissionCount: {newPermissionIds.Count}");

            using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
            try
            {
                Console.WriteLine("[DEBUG] Deleting existing permissions...");
                // Always delete existing permissions first
                await _unitOfWork.Context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM RolePermissions WHERE RoleId = {0}", roleId);

                // Only insert new permissions if there are any
                if (newPermissionIds.Any())
                {
                    Console.WriteLine("[DEBUG] Inserting new permissions in batches...");
                    // Insert new permissions in batches to avoid SQL parameter limits
                    const int batchSize = 1000;
                    for (int i = 0; i < newPermissionIds.Count; i += batchSize)
                    {
                        var batch = newPermissionIds.Skip(i).Take(batchSize);
                        var values = string.Join(",", batch.Select(id => $"({roleId}, {id})"));
                        await _unitOfWork.Context.Database.ExecuteSqlRawAsync(
                            $"INSERT INTO RolePermissions (RoleId, PermissionId) VALUES {values}");
                        Console.WriteLine($"[DEBUG] Inserted batch {i / batchSize + 1} of {(newPermissionIds.Count + batchSize - 1) / batchSize}");
                    }
                }
                else
                {
                    Console.WriteLine("[DEBUG] No permissions to insert");
                }

                await transaction.CommitAsync();
                Console.WriteLine("[DEBUG] Permissions sync completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Error in SyncRolePermissionsWithSql: {ex.Message}");
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task SyncRoleCategoriesWithSql(int roleId, List<int> newCategoryIds)
        {
            using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
            try
            {
                // Always delete existing categories first
                await _unitOfWork.Context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM RoleCategories WHERE RoleId = {0}", roleId);

                // Only insert new categories if there are any
                if (newCategoryIds.Any())
                {
                    // Insert new categories in batches
                    const int batchSize = 1000;
                    for (int i = 0; i < newCategoryIds.Count; i += batchSize)
                    {
                        var batch = newCategoryIds.Skip(i).Take(batchSize);
                        var values = string.Join(",", batch.Select(id => $"({roleId}, {id})"));
                        await _unitOfWork.Context.Database.ExecuteSqlRawAsync(
                            $"INSERT INTO RoleCategories (RoleId, CategoryId) VALUES {values}");
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task SyncRoleProductsWithSql(int roleId, List<int> newProductIds)
        {
            using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
            try
            {
                // Always delete existing products first
                await _unitOfWork.Context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM RoleProducts WHERE RoleId = {0}", roleId);

                // Only insert new products if there are any
                if (newProductIds.Any())
                {
                    // Insert new products in batches
                    const int batchSize = 1000;
                    for (int i = 0; i < newProductIds.Count; i += batchSize)
                    {
                        var batch = newProductIds.Skip(i).Take(batchSize);
                        var values = string.Join(",", batch.Select(id => $"({roleId}, {id})"));
                        await _unitOfWork.Context.Database.ExecuteSqlRawAsync(
                            $"INSERT INTO RoleProducts (RoleId, ProductId) VALUES {values}");
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task SyncRoleSubCategoriesWithSql(int roleId, List<int> newSubCategoryIds)
        {
            using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
            try
            {
                // Always delete existing sub-categories first
                await _unitOfWork.Context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM RoleSubCategories WHERE RoleId = {0}", roleId);

                // Only insert new sub-categories if there are any
                if (newSubCategoryIds.Any())
                {
                    // Insert new sub-categories in batches
                    const int batchSize = 1000;
                    for (int i = 0; i < newSubCategoryIds.Count; i += batchSize)
                    {
                        var batch = newSubCategoryIds.Skip(i).Take(batchSize);
                        var values = string.Join(",", batch.Select(id => $"({roleId}, {id})"));
                        await _unitOfWork.Context.Database.ExecuteSqlRawAsync(
                            $"INSERT INTO RoleSubCategories (RoleId, SubCategoryId) VALUES {values}");
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}