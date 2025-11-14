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
    public class PermissionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PermissionsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetAllPermissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            try
            {
                var list = await _unitOfWork.PermissionRepo.GetAll().ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("AssignPermissionsToRole")]
        public async Task<IActionResult> AssignPermissionsToRole([FromBody] AssignPermissionsDto dto)
        {
            try
            {
                var role = await _unitOfWork.RoleRepo.GetAll()
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.Id == dto.RoleId);

                if (role == null)
                    return NotFound("Role not found.");

                await _unitOfWork.RolePermissionRepo.DeleteRange(role.RolePermissions.ToList());
                await _unitOfWork.SaveAsync(); // Ensure deletion is saved before adding new ones

                if (dto.PermissionCodes?.Any() == true)
                {
                    var permissions = await _unitOfWork.PermissionRepo.GetAll()
                        .Where(p => dto.PermissionCodes.Contains(p.Code))
                        .ToListAsync();

                    // Log the permissions being assigned
                    var permissionCodes = permissions.Select(p => p.Code).ToList();
                    Console.WriteLine($"[AssignPermissionsToRole] Assigning {permissions.Count} permissions to role {dto.RoleId}: {string.Join(", ", permissionCodes)}");
                    
                    // Check if WORK_OUTSIDE_WORKING_HOURS is being assigned
                    var hasWorkOutside = permissionCodes.Any(c => c.Equals("WORK_OUTSIDE_WORKING_HOURS", StringComparison.OrdinalIgnoreCase));
                    if (hasWorkOutside)
                    {
                        Console.WriteLine("[AssignPermissionsToRole] ✓ WORK_OUTSIDE_WORKING_HOURS permission is being assigned");
                    }
                    else
                    {
                        Console.WriteLine("[AssignPermissionsToRole] ✗ WORK_OUTSIDE_WORKING_HOURS permission is NOT in the assignment list");
                    }

                    role.RolePermissions = permissions.Select(p => new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = p.Id
                    }).ToList();
                }

                var result = await _unitOfWork.RoleRepo.UpdateAsync(role);
                await _unitOfWork.SaveAsync(); // Explicitly save changes
                
                // Verify the save
                var savedRole = await _unitOfWork.RoleRepo.GetAll()
                    .Include(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(r => r.Id == dto.RoleId);
                
                if (savedRole != null)
                {
                    var savedPermissionCodes = savedRole.RolePermissions.Select(rp => rp.Permission?.Code).Where(c => c != null).ToList();
                    Console.WriteLine($"[AssignPermissionsToRole] Verified saved permissions: {string.Join(", ", savedPermissionCodes)}");
                    
                    var hasWorkOutsideSaved = savedPermissionCodes.Any(c => c?.Equals("WORK_OUTSIDE_WORKING_HOURS", StringComparison.OrdinalIgnoreCase) == true);
                    if (hasWorkOutsideSaved)
                    {
                        Console.WriteLine("[AssignPermissionsToRole] ✓ WORK_OUTSIDE_WORKING_HOURS permission verified in database");
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AssignPermissionsToRole] Error: {ex.Message}");
                Console.WriteLine($"[AssignPermissionsToRole] StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("{userId}/GetUserPermissions")]
        public async Task<IActionResult> GetUserPermissions(int userId)
        {
            var user = await _unitOfWork.UserRepo.GetAll()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("User not found.");

            var permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => new
                {
                    rp.Permission.Code,
                    rp.Permission.NameEn,
                    rp.Permission.NameAr
                })
                .Distinct()
                .ToList();

            return Ok(permissions);
        }

        [HttpGet("Debug/GetCurrentUserTokenPermissions")]
        public async Task<IActionResult> GetCurrentUserTokenPermissions()
        {
            try
            {
                var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
                var userId = User.FindFirst("UserId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var permissionClaim = User.FindFirst("Permission")?.Value ?? string.Empty;
                var roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
                
                // Get actual permissions from database for comparison
                var dbPermissionCodes = new List<string>();
                if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int userIdInt))
                {
                    var user = await _unitOfWork.UserRepo.GetAll()
                        .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                                .ThenInclude(r => r.RolePermissions)
                                    .ThenInclude(rp => rp.Permission)
                        .FirstOrDefaultAsync(u => u.Id == userIdInt);

                    if (user != null)
                    {
                        dbPermissionCodes = user.UserRoles
                            .SelectMany(ur => ur.Role.RolePermissions)
                            .Where(rp => rp.Permission != null)
                            .Select(rp => rp.Permission!.Code)
                            .Distinct()
                            .ToList();
                    }
                }

                var tokenPermissions = string.IsNullOrEmpty(permissionClaim) 
                    ? new List<string>() 
                    : permissionClaim.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList();

                var hasWorkOutsideInToken = tokenPermissions.Any(p => p.Equals("WORK_OUTSIDE_WORKING_HOURS", StringComparison.OrdinalIgnoreCase));
                var hasWorkOutsideInDb = dbPermissionCodes.Any(p => 
                    string.Equals(p, "WORK_OUTSIDE_WORKING_HOURS", StringComparison.OrdinalIgnoreCase));

                return Ok(new
                {
                    username,
                    userId,
                    roles,
                    tokenPermissions = tokenPermissions,
                    databasePermissionCodes = dbPermissionCodes,
                    hasWorkOutsideInToken,
                    hasWorkOutsideInDb,
                    needsRelogin = !hasWorkOutsideInToken && hasWorkOutsideInDb,
                    message = !hasWorkOutsideInToken && hasWorkOutsideInDb 
                        ? "Permission exists in database but not in token. User needs to log out and log back in."
                        : hasWorkOutsideInToken 
                            ? "Permission is present in token." 
                            : "Permission is not assigned to user's roles."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}