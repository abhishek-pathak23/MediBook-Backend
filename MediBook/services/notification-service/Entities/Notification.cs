using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace notification_service.Entities
{
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        [Required]
        public int RecipientId { get; set; } // Patient or Provider ID depending on the context

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // BOOKING, REMINDER, CANCELLATION, PAYMENT, FOLLOWUP

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Channel { get; set; } = string.Empty; // APP, EMAIL, SMS

        public int? RelatedId { get; set; }   // e.g., AppointmentId
        [MaxLength(50)]
        public string? RelatedType { get; set; } // e.g., "Appointment"

        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
