using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Shared;

namespace Warehousing.Repo.Classes
{
    public class UserActivityLogRepo : RepositoryBase<UserActivityLog>, IUserActivityLogRepo
    {
        private new readonly WarehousingContext _context;
        private new readonly ILogger<UserActivityLogRepo> _logger;

        public UserActivityLogRepo(WarehousingContext context, ILogger<UserActivityLogRepo> logger) : base(context, logger, null)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<UserActivityLog>> GetUserActivityLogsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.UserActivityLogs
                    .Include(log => log.User)
                    .Where(log => log.UserId == userId)
                    .AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(log => log.Timestamp >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(log => log.Timestamp <= endDate.Value);

                return await query
                    .OrderByDescending(log => log.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user activity logs for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<UserActivityLog>> GetAllActivityLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string? action = null, string? module = null, int page = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.UserActivityLogs
                    .Include(log => log.User)
                    .AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(log => log.Timestamp >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(log => log.Timestamp <= endDate.Value);

                if (!string.IsNullOrEmpty(action))
                    query = query.Where(log => log.Action.Contains(action));

                if (!string.IsNullOrEmpty(module))
                    query = query.Where(log => log.Module.Contains(module));

                return await query
                    .OrderByDescending(log => log.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all activity logs");
                throw;
            }
        }

        public async Task LogUserActivityAsync(int userId, string action, string description, string entityType = "", int? entityId = null, string oldValues = "", string newValues = "", string ipAddress = "", string userAgent = "", string module = "", string severity = "INFO")
        {
            try
            {
                var log = new UserActivityLog
                {
                    UserId = userId,
                    Action = action,
                    Description = description,
                    EntityType = entityType,
                    EntityId = entityId,
                    OldValues = oldValues,
                    NewValues = newValues,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Module = module,
                    Severity = severity,
                    Timestamp = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId.ToString()
                };

                _context.UserActivityLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging user activity for user {UserId}, action {Action}", userId, action);
                // Don't throw here to avoid breaking the main operation
            }
        }

        public async Task<int> GetActivityLogCountAsync(int? userId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.UserActivityLogs.AsQueryable();

                if (userId.HasValue)
                    query = query.Where(log => log.UserId == userId.Value);

                if (startDate.HasValue)
                    query = query.Where(log => log.Timestamp >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(log => log.Timestamp <= endDate.Value);

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activity log count");
                return 0;
            }
        }
    }
}
