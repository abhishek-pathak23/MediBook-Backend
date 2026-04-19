using Microsoft.EntityFrameworkCore;
using schedule_service.Data;
using schedule_service.Entities;
using schedule_service.Interfaces;

namespace schedule_service.Repositories
{
    public class SlotRepository : ISlotRepository
    {
        private readonly ApplicationDbContext _context;

        public SlotRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddSlot(AvailabilitySlot slot)
        {
            _context.AvailabilitySlots.Add(slot);
        }

        public void AddBulkSlots(IEnumerable<AvailabilitySlot> slots)
        {
            _context.AvailabilitySlots.AddRange(slots);
        }

        public void UpdateSlot(AvailabilitySlot slot)
        {
            _context.AvailabilitySlots.Update(slot);
        }

        public void DeleteBySlotId(int id)
        {
            var slot = _context.AvailabilitySlots.Find(id);
            if (slot != null)
            {
                _context.AvailabilitySlots.Remove(slot);
            }
        }

        public AvailabilitySlot? GetById(int id)
        {
            return _context.AvailabilitySlots.Find(id);
        }

        public List<AvailabilitySlot> FindByProviderId(int providerId)
        {
            return _context.AvailabilitySlots
                .Where(s => s.ProviderId == providerId)
                .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
                .ToList();
        }

        public List<AvailabilitySlot> FindByProviderIdAndDate(int providerId, DateOnly date)
        {
            return _context.AvailabilitySlots
                .Where(s => s.ProviderId == providerId && s.Date == date)
                .OrderBy(s => s.StartTime)
                .ToList();
        }

        public List<AvailabilitySlot> FindAvailableByProviderAndDate(int providerId, DateOnly date)
        {
            return _context.AvailabilitySlots
                .Where(s => s.ProviderId == providerId && s.Date == date && !s.IsBooked && !s.IsBlocked)
                .OrderBy(s => s.StartTime)
                .ToList();
        }

        public List<AvailabilitySlot> FindByDateBetween(DateOnly startDate, DateOnly endDate)
        {
            return _context.AvailabilitySlots
                .Where(s => s.Date >= startDate && s.Date <= endDate)
                .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
                .ToList();
        }

        public int CountAvailableByProviderId(int providerId)
        {
            return _context.AvailabilitySlots
                .Count(s => s.ProviderId == providerId && !s.IsBooked && !s.IsBlocked);
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() >= 0;
        }
    }
}
