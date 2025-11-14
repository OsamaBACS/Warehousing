using System.ComponentModel.DataAnnotations;

namespace Warehousing.Data.Entities
{
    public class WorkingHours : BaseClass
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty; // "Default Working Hours", "Holiday Schedule", etc.

        // Deprecated: Kept for backward compatibility, use Days collection instead
        public TimeSpan? StartTime { get; set; } // e.g., 08:00

        // Deprecated: Kept for backward compatibility, use Days collection instead
        public TimeSpan? EndTime { get; set; } // e.g., 17:00

        // Deprecated: Kept for backward compatibility, use Days collection instead
        public DayOfWeek? StartDay { get; set; } = DayOfWeek.Sunday; // 0 = Sunday

        // Deprecated: Kept for backward compatibility, use Days collection instead
        public DayOfWeek? EndDay { get; set; } = DayOfWeek.Thursday; // 4 = Thursday

        public bool IsActive { get; set; } = true;

        public bool AllowWeekends { get; set; } = false;

        public bool AllowHolidays { get; set; } = false;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<WorkingHoursException> Exceptions { get; set; } = new List<WorkingHoursException>();
        public virtual ICollection<WorkingHoursDay> Days { get; set; } = new List<WorkingHoursDay>();
    }

    public class WorkingHoursException : BaseClass
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WorkingHoursId { get; set; }
        public virtual WorkingHours WorkingHours { get; set; } = null!;

        [Required]
        public DateTime ExceptionDate { get; set; }

        public TimeSpan? StartTime { get; set; } // If null, uses default start time

        public TimeSpan? EndTime { get; set; } // If null, uses default end time

        [MaxLength(200)]
        public string Reason { get; set; } = string.Empty; // "Holiday", "Maintenance", etc.

        public bool IsWorkingDay { get; set; } = true; // false for holidays, true for special working days
    }
}


