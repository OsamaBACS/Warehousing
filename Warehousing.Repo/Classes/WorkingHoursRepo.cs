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
                    .Include(wh => wh.Days)
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

                // Check for exceptions (holidays, special days) first
                var exception = await _context.WorkingHoursExceptions
                    .FirstOrDefaultAsync(e => e.WorkingHoursId == workingHours.Id && 
                                            e.ExceptionDate.Date == dateTime.Date);

                if (exception != null)
                {
                    if (!exception.IsWorkingDay)
                        return false; // It's a holiday
                    
                    // Use exception times if provided, otherwise check the day's configuration
                    if (exception.StartTime.HasValue && exception.EndTime.HasValue)
                    {
                        return currentTime >= exception.StartTime.Value && currentTime <= exception.EndTime.Value;
                    }
                    // If exception doesn't specify times, fall through to check day configuration
                }

                // Check per-day configuration
                if (workingHours.Days != null && workingHours.Days.Any())
                {
                    var dayConfig = workingHours.Days.FirstOrDefault(d => d.DayOfWeek == currentDay);
                    
                    if (dayConfig != null && dayConfig.IsEnabled)
                    {
                        // If day is enabled but has no times set, allow access
                        if (!dayConfig.StartTime.HasValue || !dayConfig.EndTime.HasValue)
                            return true;
                        
                        return currentTime >= dayConfig.StartTime.Value && currentTime <= dayConfig.EndTime.Value;
                    }
                    
                    // Day is not enabled or doesn't exist in configuration
                    // Check if weekends are allowed for Friday/Saturday
                    if (!workingHours.AllowWeekends && (currentDay == DayOfWeek.Friday || currentDay == DayOfWeek.Saturday))
                        return false;
                    
                    return false; // Day is not configured as a working day
                }

                // Fallback to old period-based logic for backward compatibility
                // (in case days haven't been configured yet)
                if (!workingHours.AllowWeekends && (currentDay == DayOfWeek.Friday || currentDay == DayOfWeek.Saturday))
                    return false;

                if (workingHours.StartDay.HasValue && workingHours.EndDay.HasValue)
                {
                    // Check if current day is within working days range
                    if (workingHours.StartDay.Value <= workingHours.EndDay.Value)
                    {
                        if (currentDay < workingHours.StartDay.Value || currentDay > workingHours.EndDay.Value)
                            return false;
                    }
                    else // Weekend spans across Sunday (e.g., Sunday to Thursday)
                    {
                        if (currentDay < workingHours.StartDay.Value && currentDay > workingHours.EndDay.Value)
                            return false;
                    }
                }

                // Check regular working hours (old way)
                if (workingHours.StartTime.HasValue && workingHours.EndTime.HasValue)
                {
                    return currentTime >= workingHours.StartTime.Value && currentTime <= workingHours.EndTime.Value;
                }

                return true; // If no time restrictions, allow access
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
                    StartTime = new TimeSpan(8, 0, 0), // 8:00 AM (deprecated, kept for backward compatibility)
                    EndTime = new TimeSpan(17, 0, 0), // 5:00 PM (deprecated, kept for backward compatibility)
                    StartDay = DayOfWeek.Sunday, // Deprecated
                    EndDay = DayOfWeek.Thursday, // Deprecated
                    AllowWeekends = false,
                    AllowHolidays = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                };

                _context.WorkingHours.Add(defaultWorkingHours);
                await _context.SaveChangesAsync();

                // Create default days configuration (Sunday to Thursday, 8 AM - 5 PM)
                var defaultDays = new List<WorkingHoursDay>();
                var defaultDaysOfWeek = new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday };
                
                foreach (var dayOfWeek in Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>())
                {
                    var isDefaultWorkingDay = defaultDaysOfWeek.Contains(dayOfWeek);
                    defaultDays.Add(new WorkingHoursDay
                    {
                        WorkingHoursId = defaultWorkingHours.Id,
                        DayOfWeek = dayOfWeek,
                        StartTime = isDefaultWorkingDay ? new TimeSpan(8, 0, 0) : null,
                        EndTime = isDefaultWorkingDay ? new TimeSpan(17, 0, 0) : null,
                        IsEnabled = isDefaultWorkingDay,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "system"
                    });
                }

                _context.WorkingHoursDays.AddRange(defaultDays);
                await _context.SaveChangesAsync();

                // Reload to include days
                return await GetActiveWorkingHoursAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default working hours");
                return null;
            }
        }
    }
}
