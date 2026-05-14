using System.ComponentModel.DataAnnotations;

namespace schedule_service.DTOs
{
    public class SlotCreateDto
    {
        [Required]
        public int ProviderId { get; set; }
        [Required]
        public DateOnly Date { get; set; }
        [Required]
        public TimeOnly StartTime { get; set; }
        [Required]
        public TimeOnly EndTime { get; set; }
        
        public int DurationMinutes { get; set; }
    }
}
