using System.ComponentModel.DataAnnotations;

namespace appointment_service.DTOs
{
    public class AppointmentRescheduleDto
    {
        [Required]
        public int NewSlotId { get; set; }
    }
}
