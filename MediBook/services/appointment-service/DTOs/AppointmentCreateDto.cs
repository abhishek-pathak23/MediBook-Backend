using System.ComponentModel.DataAnnotations;

namespace appointment_service.DTOs
{
    public class AppointmentCreateDto
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public int ProviderId { get; set; }

        [Required]
        public int SlotId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ServiceType { get; set; } = string.Empty;

        [Required]
        public DateOnly AppointmentDate { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // "In-Person" or "Teleconsultation"
        [MaxLength(30)]
        public string ModeOfConsultation { get; set; } = "In-Person";
    }
}
