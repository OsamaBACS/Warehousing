using Warehousing.Data.Entities;
using Warehousing.Repo.Shared;

namespace Warehousing.Repo.Interfaces
{
    public interface IUserActivityLogRepo : IRepositoryBase<UserActivityLog>
    {
        Task<IEnumerable<UserActivityLog>> GetUserActivityLogsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 50);
        Task<IEnumerable<UserActivityLog>> GetAllActivityLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string? action = null, string? module = null, int page = 1, int pageSize = 50);
        Task LogUserActivityAsync(int userId, string action, string description, string entityType = "", int? entityId = null, string oldValues = "", string newValues = "", string ipAddress = "", string userAgent = "", string module = "", string severity = "INFO");
        Task<int> GetActivityLogCountAsync(int? userId = null, DateTime? startDate = null, DateTime? endDate = null);
    }
}
