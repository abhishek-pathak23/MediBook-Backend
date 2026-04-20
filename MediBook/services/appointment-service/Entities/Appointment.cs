using System.ComponentModel.DataAnnotations;

namespace appointment_service.Entities
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        public int PatientId { get; set; }

        public int ProviderId { get; set; }

        public int SlotId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ServiceType { get; set; } = string.Empty;

        public DateOnly AppointmentDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        // Scheduled / Completed / Cancelled / No-Show
        [MaxLength(20)]
        public string Status { get; set; } = "Scheduled";

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // In-Person / Teleconsultation
        [MaxLength(30)]
        public string ModeOfConsultation { get; set; } = "In-Person";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Custom methods strictly adhering to class diagram
        public int GetAppointmentId() => AppointmentId;
        public string GetStatus() => Status;
        public void SetStatus(string status) => Status = status;
    }
}
