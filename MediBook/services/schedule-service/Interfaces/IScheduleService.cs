using schedule_service.Entities;

namespace schedule_service.Interfaces
{
    public interface IScheduleService
    {
        AvailabilitySlot AddSlot(AvailabilitySlot slot);
        List<AvailabilitySlot> AddBulkSlots(List<AvailabilitySlot> slots);
        List<AvailabilitySlot> GetSlotsByProvider(int providerId);
        List<AvailabilitySlot> GetAvailableSlots(int providerId, DateOnly date);
        AvailabilitySlot? GetSlotById(int slotId);
        void BookSlot(int slotId);
        void BlockSlot(int slotId);
        void UnblockSlot(int slotId);
        void DeleteSlot(int slotId);
        AvailabilitySlot UpdateSlot(int slotId, AvailabilitySlot updatedSlot);
        List<AvailabilitySlot> GenerateRecurringSlots(int providerId, string recurrence, DateOnly startDate, DateOnly endDate, TimeOnly startTime, TimeOnly endTime, int durationMinutes);
    }
}
