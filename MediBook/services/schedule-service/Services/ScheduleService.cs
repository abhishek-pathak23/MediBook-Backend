using schedule_service.Entities;
using schedule_service.Interfaces;

namespace schedule_service.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly ISlotRepository _repo;

        public ScheduleService(ISlotRepository repo)
        {
            _repo = repo;
        }

        private void ValidateNoOverlap(int providerId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
        {
            var existingSlots = _repo.FindByProviderIdAndDate(providerId, date);
            foreach (var slot in existingSlots)
            {
                // Overlap occurs if NewStart < OldEnd AND NewEnd > OldStart
                if (startTime < slot.EndTime && endTime > slot.StartTime)
                {
                    throw new InvalidOperationException($"Time slot overlap detected on {date} between {slot.StartTime} and {slot.EndTime}.");
                }
            }
        }

        public AvailabilitySlot AddSlot(AvailabilitySlot slot)
        {
            ValidateNoOverlap(slot.ProviderId, slot.Date, slot.StartTime, slot.EndTime);
            
            _repo.AddSlot(slot);
            if (!_repo.SaveChanges())
            {
                throw new Exception("Failed to save the new slot to the database.");
            }
            return slot;
        }

        public List<AvailabilitySlot> AddBulkSlots(List<AvailabilitySlot> slots)
        {
            if (slots == null || !slots.Any()) return new List<AvailabilitySlot>();

            foreach (var slot in slots)
            {
                ValidateNoOverlap(slot.ProviderId, slot.Date, slot.StartTime, slot.EndTime);
            }

            _repo.AddBulkSlots(slots);
            if (!_repo.SaveChanges())
            {
                 throw new Exception("Failed to save bulk slots to the database.");
            }
            return slots;
        }

        public List<AvailabilitySlot> GetSlotsByProvider(int providerId)
        {
            return _repo.FindByProviderId(providerId);
        }

        public List<AvailabilitySlot> GetAvailableSlots(int providerId, DateOnly date)
        {
            return _repo.FindAvailableByProviderAndDate(providerId, date);
        }

        public AvailabilitySlot? GetSlotById(int slotId)
        {
            return _repo.GetById(slotId);
        }

        public void BookSlot(int slotId)
        {
            var slot = _repo.GetById(slotId) ?? throw new KeyNotFoundException("Slot not found.");
            if (slot.IsBlocked) throw new InvalidOperationException("Cannot book a blocked slot.");
            if (slot.IsBooked) throw new InvalidOperationException("Slot is already booked.");
            
            slot.IsBooked = true;
            _repo.UpdateSlot(slot);
            _repo.SaveChanges();
        }

        public void BlockSlot(int slotId)
        {
            var slot = _repo.GetById(slotId) ?? throw new KeyNotFoundException("Slot not found.");
            if (slot.IsBooked) throw new InvalidOperationException("Cannot block a booked slot.");
            
            slot.IsBlocked = true;
            _repo.UpdateSlot(slot);
            _repo.SaveChanges();
        }

        public void UnblockSlot(int slotId)
        {
            var slot = _repo.GetById(slotId) ?? throw new KeyNotFoundException("Slot not found.");
            slot.IsBlocked = false;
            _repo.UpdateSlot(slot);
            _repo.SaveChanges();
        }

        public void ReleaseSlot(int slotId)
        {
            var slot = _repo.GetById(slotId) ?? throw new KeyNotFoundException("Slot not found.");
            slot.IsBooked = false;
            _repo.UpdateSlot(slot);
            _repo.SaveChanges();
        }

        public void DeleteSlot(int slotId)
        {
            var slot = _repo.GetById(slotId) ?? throw new KeyNotFoundException("Slot not found.");
            if (slot.IsBooked) throw new InvalidOperationException("Cannot delete a booked slot.");
            
            _repo.DeleteBySlotId(slotId);
            _repo.SaveChanges();
        }

        public AvailabilitySlot UpdateSlot(int slotId, AvailabilitySlot updatedSlot)
        {
            var existingSlot = _repo.GetById(slotId) ?? throw new KeyNotFoundException("Slot not found.");
            if (existingSlot.IsBooked) throw new InvalidOperationException("Cannot update time of a booked slot.");

            // If time or date changed, check overlap excluding self
            if (existingSlot.Date != updatedSlot.Date || existingSlot.StartTime != updatedSlot.StartTime || existingSlot.EndTime != updatedSlot.EndTime)
            {
               var slotsOnDate = _repo.FindByProviderIdAndDate(updatedSlot.ProviderId, updatedSlot.Date);
               foreach (var s in slotsOnDate)
               {
                   if (s.SlotId != slotId && updatedSlot.StartTime < s.EndTime && updatedSlot.EndTime > s.StartTime)
                   {
                        throw new InvalidOperationException("Updated time overlaps with an existing slot.");
                   }
               }
            }

            existingSlot.Date = updatedSlot.Date;
            // Support for TimeOnly
            existingSlot.StartTime = updatedSlot.StartTime;
            existingSlot.EndTime = updatedSlot.EndTime;
            existingSlot.DurationMinutes = updatedSlot.DurationMinutes;
            existingSlot.Recurrence = updatedSlot.Recurrence;

            _repo.UpdateSlot(existingSlot);
            _repo.SaveChanges();
            return existingSlot;
        }

        public List<AvailabilitySlot> GenerateRecurringSlots(int providerId, string recurrence, DateOnly startDate, DateOnly endDate, TimeOnly startTime, TimeOnly endTime, int durationMinutes)
        {
            if (startDate > endDate) throw new ArgumentException("Start date must be before or equal to End date.");
            
            var generatedSlots = new List<AvailabilitySlot>();
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                var slot = new AvailabilitySlot
                {
                    ProviderId = providerId,
                    Date = currentDate,
                    StartTime = startTime,
                    EndTime = endTime,
                    DurationMinutes = durationMinutes,
                    IsBooked = false,
                    IsBlocked = false,
                    Recurrence = recurrence
                };

                // Avoid collision by softly skipping overlapping slots during bulk generation
                bool hasOverlap = false;
                var existing = _repo.FindByProviderIdAndDate(providerId, currentDate);
                foreach (var s in existing)
                {
                    if (startTime < s.EndTime && endTime > s.StartTime)
                    {
                        hasOverlap = true; break;
                    }
                }

                if (!hasOverlap)
                {
                    generatedSlots.Add(slot);
                }

                // Increment based on recurrence type string
                if (recurrence.Equals("Daily", StringComparison.OrdinalIgnoreCase))
                {
                    currentDate = currentDate.AddDays(1);
                }
                else if (recurrence.Equals("Weekly", StringComparison.OrdinalIgnoreCase))
                {
                    currentDate = currentDate.AddDays(7);
                }
                else
                {
                    // If not Daily or Weekly, it's a one-off (or user submitted bad recurrence type). 
                    // Break loop since it shouldn't generate endlessly.
                    break;
                }
            }

            if (generatedSlots.Any())
            {
                _repo.AddBulkSlots(generatedSlots);
                _repo.SaveChanges();
            }

            return generatedSlots;
        }
    }
}
