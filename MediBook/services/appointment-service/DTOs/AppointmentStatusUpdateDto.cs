using System.ComponentModel.DataAnnotations;

namespace appointment_service.DTOs
{
    public class AppointmentStatusUpdateDto
    {
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;
    }
}
