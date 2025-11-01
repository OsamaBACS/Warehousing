namespace Warehousing.Repo.Dtos
{
    public class WorkingHoursDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DayOfWeek StartDay { get; set; }
        public DayOfWeek EndDay { get; set; }
        public bool IsActive { get; set; }
        public bool AllowWeekends { get; set; }
        public bool AllowHolidays { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<WorkingHoursExceptionDto> Exceptions { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class WorkingHoursExceptionDto
    {
        public int Id { get; set; }
        public int WorkingHoursId { get; set; }
        public DateTime ExceptionDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool IsWorkingDay { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class WorkingHoursStatusDto
    {
        public bool IsWithinWorkingHours { get; set; }
        public string CurrentTime { get; set; } = string.Empty;
        public string WorkingHoursDescription { get; set; } = string.Empty;
        public TimeSpan? TimeUntilNextWorkingDay { get; set; }
        public string NextWorkingDay { get; set; } = string.Empty;
    }
}


