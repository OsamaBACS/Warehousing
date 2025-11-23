using System.ComponentModel.DataAnnotations;

namespace Warehousing.Data.Entities
{
    public class UserActivityLog : BaseClass
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty; // LOGIN, LOGOUT, CREATE_PRODUCT, EDIT_ORDER, etc.

        [MaxLength(200)]
        public string Description { get; set; } = string.Empty; // Human readable description

        [MaxLength(50)]
        public string EntityType { get; set; } = string.Empty; // Product, Order, User, etc.

        public int? EntityId { get; set; } // ID of the affected entity

        [MaxLength(500)]
        public string OldValues { get; set; } = string.Empty; // JSON of old values (for updates)

        [MaxLength(500)]
        public string NewValues { get; set; } = string.Empty; // JSON of new values (for updates)

        [MaxLength(45)]
        public string IpAddress { get; set; } = string.Empty;

        [MaxLength(500)]
        public string UserAgent { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Module { get; set; } = string.Empty; // Products, Orders, Users, etc.

        [MaxLength(50)]
        public string Severity { get; set; } = "INFO"; // INFO, WARNING, ERROR, CRITICAL

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}








