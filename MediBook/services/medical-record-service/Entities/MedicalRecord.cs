using System.ComponentModel.DataAnnotations;

namespace medical_record_service.Entities
{
    public class MedicalRecord
    {
        [Key]
        public int RecordId { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int ProviderId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Diagnosis { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Prescription { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }

        public DateOnly? FollowUpDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
