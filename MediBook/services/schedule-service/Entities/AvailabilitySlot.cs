using System.ComponentModel.DataAnnotations;

namespace schedule_service.Entities
{
    public class AvailabilitySlot
    {
        [Key]
        public int SlotId { get; set; }
        public int ProviderId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsBooked { get; set; }
        public bool IsBlocked { get; set; }
        public string? Recurrence { get; set; } = "None"; // e.g., "None", "Daily", "Weekly"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Custom Methods strictly adhering to diagram
        public int GetSlotId() => SlotId;
        public bool GetIsBooked() => IsBooked;
        public bool GetIsBlocked() => IsBlocked;
    }
}
