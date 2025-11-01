using System.Linq.Expressions;
using System.Security.Claims;
using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Dtos;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Models;
using Warehousing.Repo.Shared;
using Warehousing.Repo.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Warehousing.Repo.Classes
{
    public class UserRepo : RepositoryBase<User>, IUserRepo
    {
        private new readonly ILogger<UserRepo> _logger;
        private readonly IConfiguration _config;
        private readonly IActivityLoggingService _activityLoggingService;
        private readonly IWorkingHoursRepo _workingHoursRepo;
        
        public UserRepo(WarehousingContext context, ILogger<UserRepo> logger, IConfiguration config, IActivityLoggingService activityLoggingService, IWorkingHoursRepo workingHoursRepo) : base(context, logger, config)
        {
            _logger = logger;
            _config = config;
            _activityLoggingService = activityLoggingService;
            _workingHoursRepo = workingHoursRepo;
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

                // ⛔ Working hours are no longer enforced at login time.
                // Enforcement should happen on sensitive actions (e.g., order creation) for non-admins,
                // optionally honoring per-user exceptions.

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("UserName", user.Username.ToString()),
                    new Claim("NameEn", user.NameEn ?? user.Username),
                    new Claim("NameAr", user.NameAr ?? user.Username),
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

                // ✅ Add claims with smaller size limits to prevent token explosion
                // Apply the same limits to all users (including TestUser) to prevent token size issues
                claims.Add(new Claim("Permission", string.Join(",", permissions.Take(50) ?? new List<string>()))); // Limit to 50 permissions
                claims.Add(new Claim("Category", string.Join(",", categoryIds.Take(100) ?? new List<int>()))); // Limit to 100 categories
                claims.Add(new Claim("Product", string.Join(",", productIds.Take(500) ?? new List<int>()))); // Limit to 500 products
                claims.Add(new Claim("SubCategory", string.Join(",", subCategoryIds.Take(100) ?? new List<int>()))); // Limit to 100 subcategories
                
                _logger.LogInformation("Claims added for user {Username} - Permissions: {PermissionCount}, Categories: {CategoryCount}, Products: {ProductCount}, SubCategories: {SubCategoryCount}", 
                    user.Username, permissions.Take(50).Count(), categoryIds.Take(100).Count(), productIds.Take(500).Count(), subCategoryIds.Take(100).Count());

                // ✅ Add special claim for admin users to indicate they have all permissions
                if (user.Username.Equals("admin"))
                {
                    claims.Add(new Claim("IsAdmin", "true"));
                }

                _logger.LogInformation("Generating JWT token with {ClaimsCount} claims for user: {Username}", claims.Count, user.Username);
                
                // Log each claim for debugging
                foreach (var claim in claims)
                {
                    _logger.LogInformation("Claim: {Type} = {Value} (length: {Length})", claim.Type, 
                        claim.Value.Length > 100 ? claim.Value.Substring(0, 100) + "..." : claim.Value, 
                        claim.Value.Length);
                }
                
                var helper = new HelperClass(_config);
                var token = helper.GenerateJwtToken(claims.ToArray());
                _logger.LogInformation("JWT token generated successfully for user: {Username}, Token length: {TokenLength}", user.Username, token.Length);
                
                // Log token structure for debugging
                var tokenParts = token.Split('.');
                if (tokenParts.Length == 3)
                {
                    _logger.LogInformation("Token structure - Header length: {HeaderLength}, Payload length: {PayloadLength}, Signature length: {SignatureLength}", 
                        tokenParts[0].Length, tokenParts[1].Length, tokenParts[2].Length);
                    
                    // Try to decode the payload to see if it's valid
                    try
                    {
                        var payloadBytes = Convert.FromBase64String(tokenParts[1]);
                        var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);
                        _logger.LogInformation("Payload JSON preview: {PayloadPreview}", payloadJson.Length > 200 ? payloadJson.Substring(0, 200) + "..." : payloadJson);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to decode JWT payload for user: {Username}", user.Username);
                    }
                }
                else
                {
                    _logger.LogError("Invalid JWT token structure for user: {Username}, expected 3 parts, got {PartsCount}", user.Username, tokenParts.Length);
                }
                
                // Log successful login activity
                await _activityLoggingService.LogLoginAsync(user.Id, dto.ip ?? "");
                
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