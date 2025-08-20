using System.Linq.Expressions;
using System.Security.Claims;
using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Models;
using Warehousing.Repo.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Warehousing.Repo.Classes
{
    public class UserRepo : RepositoryBase<User>, IUserRepo
    {
        private readonly ILogger<UserRepo> _logger;
        private readonly IConfiguration _config;
        public UserRepo(WarehousingContext context, ILogger<UserRepo> logger, IConfiguration config) : base(context, logger, config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<LoginResult> Login(LoginDto dto)
        {
            try
            {
                // Check working hours
                // var currentHour = DateTime.UtcNow.AddHours(3).Hour; // adjust to your timezone
                // if (currentHour < 8 || currentHour > 17)
                //     return Unauthorized("Login only allowed during work hours.");

                // Check IP whitelist
                // var allowedIps = _config.GetSection("AllowedIPs").Get<string[]>() ?? [];
                // if (!allowedIps.Contains(ip))
                //     return Unauthorized("Access not allowed from this network.");

                var hashedPassword = HelperClass.HashPassword(dto.Password);
                var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RoleCategories)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RoleProducts)
                .FirstOrDefaultAsync(u => u.Username == dto.Username && u.PasswordHash == hashedPassword);

                if (user == null)
                {
                    _logger.LogWarning("Login failed for user: {Username}", dto.Username);
                    return new LoginResult { Success = false, ErrorMessage = "Invalid credentials" };
                }

                // ✅ Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user attempted to log in: {Username}", dto.Username);
                    return new LoginResult { Success = false, ErrorMessage = "This account is inactive." };
                }

                // Check fingerprint
                var trustedDevice = _context.UserDevices
                    .FirstOrDefault(d => d.UserId == user.Id && d.Fingerprint == dto.Fingerprint);

                if (trustedDevice == null)
                {
                    if (!user.Username.Equals("admin"))
                    {
                        // Optionally save this new device for manual approval
                        _context.UserDevices.Add(new UserDevice
                        {
                            UserId = user.Id,
                            Fingerprint = dto.Fingerprint,
                            FirstSeen = DateTime.UtcNow,
                            IPAddress = dto.ip,
                            IsApproved = false
                        });
                        _context.SaveChanges();

                        return new LoginResult { Success = true, ErrorMessage = "Unrecognized device. Contact admin to approve it.", Status = "NotApproved" };
                    }
                }
                else
                {
                    if (!trustedDevice.IsApproved)
                    {
                        return new LoginResult { Success = true, ErrorMessage = "Unrecognized device. Contact admin to approve it.", Status = "NotApproved" };
                    }
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("UserName", user.Username.ToString()),
                };

                var roles = user.UserRoles.Select(ur => ur.Role.NameEn).ToList();
                foreach (var role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));

                var permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Code)
                .Distinct();

                foreach (var permission in permissions)
                    claims.Add(new Claim("Permission", permission));

                // ✅ RoleCategories
                var categoryIds = user.UserRoles
                    .SelectMany(ur => ur.Role.RoleCategories)
                    .Select(rc => rc.CategoryId)
                    .Distinct();

                foreach (var categoryId in categoryIds)
                    claims.Add(new Claim("Category", categoryId.ToString()));

                // ✅ RoleProducts
                var productIds = user.UserRoles
                    .SelectMany(ur => ur.Role.RoleProducts)
                    .Select(rp => rp.ProductId)
                    .Distinct();

                foreach (var productId in productIds)
                    claims.Add(new Claim("Product", productId.ToString()));

                var helper = new HelperClass(_config);
                var token = helper.GenerateJwtToken(claims.ToArray());
                return new LoginResult { Success = true, Token = token };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for user: {Username}", dto.Username);
                return new LoginResult { Success = false, ErrorMessage = "An error occurred while processing the request." };
            }
        }

        public async Task<IList<User>> CustomeGetAllPagination(
        int pageIndex,
        int pageSize,
        Expression<Func<User, int>> orderBy,
        params Expression<Func<User, object>>[]? includes)
        {
            var query = _context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query
                .Skip(pageSize * (pageIndex - 1))
                .Take(pageSize)
                .OrderBy(orderBy)
                .ToListAsync();
        }

        public async Task<IList<User>> CustomeSearch(int pageIndex, int pageSize, Expression<Func<User, bool>> expression, Expression<Func<User, int>> orderBy, params Expression<Func<User, object>>[]? includes)
        {
            var query = _context.Users
                .Where(expression)
                .AsNoTracking()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query
                .Skip(pageSize * (pageIndex - 1))
                .Take(pageSize)
                .OrderBy(orderBy)
                .ToListAsync();
        }
    }
}