using System.ComponentModel.DataAnnotations;

namespace medical_record_service.DTOs
{
    public class RecordCreateDto
    {
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
    }

    public class RecordUpdateDto
    {
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
    }

    public class AttachDocumentDto
    {
        [Required]
        [MaxLength(500)]
        public string AttachmentUrl { get; set; } = string.Empty;
    }

    public class RecordResponseDto
    {
        public int RecordId { get; set; }
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int ProviderId { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Prescription { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? AttachmentUrl { get; set; }
        public DateOnly? FollowUpDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
