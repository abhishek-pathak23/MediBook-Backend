using System.ComponentModel.DataAnnotations;

namespace schedule_service.DTOs
{
    public class SlotUpdateDto
    {
        [Required]
        public DateOnly Date { get; set; }
        [Required]
        public TimeOnly StartTime { get; set; }
        [Required]
        public TimeOnly EndTime { get; set; }
        
        public int DurationMinutes { get; set; }
        public string? Recurrence { get; set; }
    }
}
