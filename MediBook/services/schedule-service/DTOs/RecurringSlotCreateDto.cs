using System.ComponentModel.DataAnnotations;

namespace schedule_service.DTOs
{
    public class RecurringSlotCreateDto
    {
        [Required]
        public int ProviderId { get; set; }
        
        [Required]
        public string Recurrence { get; set; } = "Weekly";
        
        [Required]
        public DateOnly StartDate { get; set; }
        
        [Required]
        public DateOnly EndDate { get; set; }
        
        [Required]
        public TimeOnly StartTime { get; set; }
        
        [Required]
        public TimeOnly EndTime { get; set; }
        
        public int DurationMinutes { get; set; }
    }
}
