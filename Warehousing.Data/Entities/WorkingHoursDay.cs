using System.ComponentModel.DataAnnotations;

namespace Warehousing.Data.Entities
{
    public class WorkingHoursDay : BaseClass
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WorkingHoursId { get; set; }
        public virtual WorkingHours WorkingHours { get; set; } = null!;

        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan? StartTime { get; set; } // Null means the day is not a working day

        public TimeSpan? EndTime { get; set; } // Null means the day is not a working day

        public bool IsEnabled { get; set; } = true; // Whether this day is enabled as a working day
    }
}



