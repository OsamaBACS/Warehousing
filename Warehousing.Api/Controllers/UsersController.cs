using System.Security.Cryptography;
using System.Text;
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
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var list = await _unitOfWork.UserRepo.GetAll().ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetUserById")]
        public async Task<IActionResult> GetUserById(int Id)
        {
            try
            {
                var user = await _unitOfWork.UserRepo.GetByCondition(u => u.Id == Id)
                    .Include(r => r.UserRoles)
                    .ThenInclude(r => r.Role)
                    .FirstOrDefaultAsync();
                if (user == null)
                {
                    return NotFound("User Not Found!");
                }
                else
                {
                    return Ok(user);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("GetUsersPagination")]
        public async Task<IActionResult> GetUsersPagination(int pageIndex, int pageSize)
        {
            try
            {
                var list = await _unitOfWork.UserRepo.CustomeGetAllPagination(pageIndex, pageSize, x => x.Id, null);
                var TotalSize = await _unitOfWork.UserRepo.GetTotalCount();
                return Ok(new
                {
                    users = list,
                    totals = TotalSize
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("SearchUsersPagination")]
        public async Task<IActionResult> SearchUsersPagination(int pageIndex, int pageSize, string keyword)
        {
            try
            {
                var list = await _unitOfWork.UserRepo.CustomeSearch(
                                                        pageIndex,
                                                        pageSize,
                                                        x => x.Username.Contains(keyword),
                                                        x => x.Id, null);
                return Ok(new
                {
                    users = list,
                    totals = list.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("SaveUser")]
        public async Task<IActionResult> SaveUser([FromBody] UsersDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(ModelState);

                if (dto.Id > 0)
                {
                    var user = await _unitOfWork.UserRepo.GetByCondition(u => u.Id == dto.Id).Include(u => u.UserRoles).FirstOrDefaultAsync();
                    // Edit existing user
                    if (user == null)
                        return NotFound("User not found!");

                    // Update basic fields
                    _mapper.Map(dto, user);

                    // Handle roles
                    await UpdateUserRoles(user, dto.Roles);

                    var updatedUser = await _unitOfWork.UserRepo.UpdateAsync(user);
                    return Ok(updatedUser);
                }
                else
                {
                    // Create new user
                    var user = new User();
                    _mapper.Map(dto, user);
                    var createdUser = await _unitOfWork.UserRepo.CreateAsync(user);
                    await UpdateUserRoles(createdUser, dto.Roles); // Assign roles before saving
                    return Ok(createdUser);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task UpdateUserRoles(User user, List<int> selectedRoleIds)
        {
            // Load current roles for this user
            var currentRoles = user.UserRoles ?? new List<UserRole>();

            // Get all available roles from database
            var allRoles = await _unitOfWork.RoleRepo.GetAll().ToListAsync();

            // Remove roles that are no longer assigned
            var rolesToRemove = currentRoles.Where(ur => !selectedRoleIds.Contains(ur.RoleId)).ToList();
            foreach (var ur in rolesToRemove)
            {
                await _unitOfWork.UserRoleRepo.DeleteAsync(ur);
            }

            // Add new roles
            var newRoleIds = selectedRoleIds.Except(currentRoles.Select(ur => ur.RoleId));
            foreach (var roleId in newRoleIds)
            {
                var newRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId
                };
                await _unitOfWork.UserRoleRepo.CreateAsync(newRole);
            }
        }

        [HttpGet("ChangePasswordForAdmin")]
        public async Task<IActionResult> ChangePasswordForAdmin(int Id)
        {
            try
            {
                var user = await _unitOfWork.UserRepo.GetByCondition(u => u.Id == Id).FirstOrDefaultAsync();
                if (user != null)
                {
                    using (var hmac = new HMACSHA512())
                    {
                        user.PasswordHash = HelperClass.HashPassword(user.Phone);
                    }
                    var result = await _unitOfWork.UserRepo.UpdateAsync(user);
                    if (result != null)
                    {
                        return Ok(result.Id);
                    }
                    else
                    {
                        return BadRequest("Error in Changing password!");
                    }
                }
                else
                {
                    return BadRequest("There is no user with this Id!");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet]
        [Route("GetUserDevicesPagination")]
        public async Task<IActionResult> GetUserDevices(int pageIndex, int pageSize, int UserId = 0)
        {
            try
            {
                var list = await _unitOfWork.UserDeviceRepo.GetAllPagination(pageIndex, pageSize, x => x.Id, null);
                if (UserId > 0)
                {
                    list = list.Where(d => d.UserId == UserId).ToList();
                }
                var TotalSize = await _unitOfWork.UserDeviceRepo.GetTotalCount();
                return Ok(new
                {
                    devices = list,
                    totals = TotalSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Route("UpdateUserDevice")]
        public async Task<IActionResult> UpdateUserDevice([FromBody] UpdateUserDeviceDto dto)
        {
            try
            {
                var device = await _unitOfWork.UserDeviceRepo.GetByCondition(d => d.Id == dto.DeviceId).FirstOrDefaultAsync();
                if (device != null)
                {
                    device.IsApproved = dto.IsApproved;
                    var result = await _unitOfWork.UserDeviceRepo.UpdateAsync(device);
                    return Ok(result);
                }
                else
                {
                    return BadRequest("No Device found!");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}