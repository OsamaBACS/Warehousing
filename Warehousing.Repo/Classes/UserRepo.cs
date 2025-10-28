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
        private new readonly ILogger<UserRepo> _logger;
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
                var hashedPassword = HelperClass.HashPassword(dto.Password);
                _logger.LogInformation("Attempting login for user: {Username}", dto.Username);
                
                // ✅ OPTIMIZED QUERY: Load user with minimal data first
                var user = await _context.Users
                    .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Username == dto.Username && u.PasswordHash == hashedPassword);

                if (user == null)
                {
                    _logger.LogWarning("Login failed for user: {Username} - User not found or invalid password", dto.Username);
                    return new LoginResult { Success = false, ErrorMessage = "Invalid credentials" };
                }

                _logger.LogInformation("User found: {Username}, IsActive: {IsActive}", user.Username, user.IsActive);

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
                            Fingerprint = dto.Fingerprint ?? "",
                            FirstSeen = DateTime.UtcNow,
                            IPAddress = dto.ip ?? "",
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

                var roles = user.UserRoles.Select(ur => ur.Role?.NameEn).Where(name => !string.IsNullOrEmpty(name)).ToList();
                foreach (var role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role!));

                // ✅ OPTIMIZED: Load permissions separately to avoid massive joins
                var roleIds = user.UserRoles?.Select(ur => ur.RoleId).ToList() ?? new List<int>();
                _logger.LogInformation("User roles: {RoleIds}", string.Join(",", roleIds));
                
                List<string> permissions = new List<string>();
                List<int> categoryIds = new List<int>();
                List<int> productIds = new List<int>();
                List<int> subCategoryIds = new List<int>();

                try
                {
                    permissions = await _context.RolePermissions
                        .Where(rp => roleIds.Contains(rp.RoleId))
                        .Include(rp => rp.Permission)
                        .Select(rp => rp.Permission!.Code)
                        .Distinct()
                        .ToListAsync();
                    _logger.LogInformation("Permissions loaded: {Count}", permissions.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to load permissions: {Error}", ex.Message);
                }

                try
                {
                    categoryIds = await _context.RoleCategories
                        .Where(rc => roleIds.Contains(rc.RoleId))
                        .Select(rc => rc.CategoryId)
                        .Distinct()
                        .ToListAsync();
                    _logger.LogInformation("Categories loaded: {Count}", categoryIds.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to load categories: {Error}", ex.Message);
                }

                try
                {
                    productIds = await _context.RoleProducts
                        .Where(rp => roleIds.Contains(rp.RoleId))
                        .Select(rp => rp.ProductId)
                        .Distinct()
                        .ToListAsync();
                    _logger.LogInformation("Products loaded: {Count}", productIds.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to load products: {Error}", ex.Message);
                }

                try
                {
                    subCategoryIds = await _context.RoleSubCategories
                        .Where(rsc => roleIds.Contains(rsc.RoleId))
                        .Select(rsc => rsc.SubCategoryId)
                        .Distinct()
                        .ToListAsync();
                    _logger.LogInformation("SubCategories loaded: {Count}", subCategoryIds.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to load subcategories: {Error}", ex.Message);
                }

                // ✅ Add claims with size limits to prevent token explosion
                claims.Add(new Claim("Permission", string.Join(",", permissions.Take(100) ?? new List<string>()))); // Limit to 100 permissions
                claims.Add(new Claim("Category", string.Join(",", categoryIds.Take(1000) ?? new List<int>()))); // Limit to 1000 categories
                claims.Add(new Claim("Product", string.Join(",", productIds.Take(5000) ?? new List<int>()))); // Limit to 5000 products
                claims.Add(new Claim("SubCategory", string.Join(",", subCategoryIds.Take(1000) ?? new List<int>()))); // Limit to 1000 subcategories

                // ✅ Add special claim for admin users to indicate they have all permissions
                if (user.Username.Equals("admin"))
                {
                    claims.Add(new Claim("IsAdmin", "true"));
                }

                _logger.LogInformation("Generating JWT token with {ClaimsCount} claims", claims.Count);
                var helper = new HelperClass(_config);
                var token = helper.GenerateJwtToken(claims.ToArray());
                _logger.LogInformation("JWT token generated successfully for user: {Username}", user.Username);
                return new LoginResult { Success = true, Token = token };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for user: {Username}. Error: {ErrorMessage}", dto.Username, ex.Message);
                return new LoginResult { Success = false, ErrorMessage = $"An error occurred while processing the request: {ex.Message}" };
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