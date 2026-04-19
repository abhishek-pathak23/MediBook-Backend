using schedule_service.Entities;

namespace schedule_service.Interfaces
{
    public interface ISlotRepository
    {
        void AddSlot(AvailabilitySlot slot);
        void AddBulkSlots(IEnumerable<AvailabilitySlot> slots);
        void UpdateSlot(AvailabilitySlot slot);
        void DeleteBySlotId(int id);
        
        AvailabilitySlot? GetById(int id);
        List<AvailabilitySlot> FindByProviderId(int providerId);
        List<AvailabilitySlot> FindByProviderIdAndDate(int providerId, DateOnly date);
        List<AvailabilitySlot> FindAvailableByProviderAndDate(int providerId, DateOnly date);
        List<AvailabilitySlot> FindByDateBetween(DateOnly startDate, DateOnly endDate);
        int CountAvailableByProviderId(int providerId);
        
        bool SaveChanges();
    }
}
