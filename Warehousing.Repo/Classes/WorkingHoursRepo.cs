using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Warehousing.Data.Context;
using Warehousing.Data.Entities;
using Warehousing.Repo.Interfaces;
using Warehousing.Repo.Shared;

namespace Warehousing.Repo.Classes
{
    public class WorkingHoursRepo : RepositoryBase<WorkingHours>, IWorkingHoursRepo
    {
        private new readonly WarehousingContext _context;
        private new readonly ILogger<WorkingHoursRepo> _logger;

        public WorkingHoursRepo(WarehousingContext context, ILogger<WorkingHoursRepo> logger) : base(context, logger, null)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<WorkingHours?> GetActiveWorkingHoursAsync()
        {
            try
            {
                return await _context.WorkingHours
                    .Include(wh => wh.Exceptions)
                    .FirstOrDefaultAsync(wh => wh.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active working hours");
                return null;
            }
        }

        public async Task<bool> IsWithinWorkingHoursAsync(DateTime dateTime)
        {
            try
            {
                var workingHours = await GetActiveWorkingHoursAsync();
                if (workingHours == null)
                    return true; // If no working hours configured, allow access

                var currentTime = dateTime.TimeOfDay;
                var currentDay = dateTime.DayOfWeek;

                // Check if it's a weekend
                if (!workingHours.AllowWeekends && (currentDay == DayOfWeek.Friday || currentDay == DayOfWeek.Saturday))
                    return false;

                // Check if current day is within working days range
                if (workingHours.StartDay <= workingHours.EndDay)
                {
                    if (currentDay < workingHours.StartDay || currentDay > workingHours.EndDay)
                        return false;
                }
                else // Weekend spans across Sunday (e.g., Sunday to Thursday)
                {
                    if (currentDay < workingHours.StartDay && currentDay > workingHours.EndDay)
                        return false;
                }

                // Check for exceptions (holidays, special days)
                var exception = await _context.WorkingHoursExceptions
                    .FirstOrDefaultAsync(e => e.WorkingHoursId == workingHours.Id && 
                                            e.ExceptionDate.Date == dateTime.Date);

                if (exception != null)
                {
                    if (!exception.IsWorkingDay)
                        return false; // It's a holiday
                    
                    // Use exception times if provided
                    var startTime = exception.StartTime ?? workingHours.StartTime;
                    var endTime = exception.EndTime ?? workingHours.EndTime;
                    
                    return currentTime >= startTime && currentTime <= endTime;
                }

                // Check regular working hours
                return currentTime >= workingHours.StartTime && currentTime <= workingHours.EndTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking working hours for {DateTime}", dateTime);
                return true; // Default to allowing access on error
            }
        }

        public async Task<WorkingHours?> CreateDefaultWorkingHoursAsync()
        {
            try
            {
                // Check if there's already an active working hours configuration
                var existing = await GetActiveWorkingHoursAsync();
                if (existing != null)
                    return existing;

                var defaultWorkingHours = new WorkingHours
                {
                    Name = "Default Working Hours",
                    Description = "Standard working hours (Sunday to Thursday, 8:00 AM to 5:00 PM)",
                    StartTime = new TimeSpan(8, 0, 0), // 8:00 AM
                    EndTime = new TimeSpan(17, 0, 0), // 5:00 PM
                    StartDay = DayOfWeek.Sunday,
                    EndDay = DayOfWeek.Thursday,
                    AllowWeekends = false,
                    AllowHolidays = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                };

                _context.WorkingHours.Add(defaultWorkingHours);
                await _context.SaveChangesAsync();

                return defaultWorkingHours;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default working hours");
                return null;
            }
        }
    }
}
