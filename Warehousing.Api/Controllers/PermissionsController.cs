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
            var role = await _unitOfWork.RoleRepo.GetAll()
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == dto.RoleId);

            if (role == null)
                return NotFound("Role not found.");

            await _unitOfWork.RolePermissionRepo.DeleteRange(role.RolePermissions.ToList());

            if (dto.PermissionCodes?.Any() == true)
            {
                var permissions = await _unitOfWork.PermissionRepo.GetAll()
                    .Where(p => dto.PermissionCodes.Contains(p.Code))
                    .ToListAsync();

                role.RolePermissions = permissions.Select(p => new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = p.Id
                }).ToList();
            }

            var result = await _unitOfWork.RoleRepo.UpdateAsync(role);
            return Ok(result);
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
    }
}