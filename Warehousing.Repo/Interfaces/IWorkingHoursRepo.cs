using Warehousing.Data.Entities;
using Warehousing.Repo.Shared;

namespace Warehousing.Repo.Interfaces
{
    public interface IWorkingHoursRepo : IRepositoryBase<WorkingHours>
    {
        Task<WorkingHours?> GetActiveWorkingHoursAsync();
        Task<bool> IsWithinWorkingHoursAsync(DateTime dateTime);
        Task<WorkingHours?> CreateDefaultWorkingHoursAsync();
    }
}
