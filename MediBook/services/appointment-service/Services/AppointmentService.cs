using appointment_service.Entities;
using appointment_service.Interfaces;

namespace appointment_service.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repo;
        private readonly IScheduleService _schedSvc;
        private readonly IPaymentService _paySvc;

        private static readonly HashSet<string> ValidStatuses =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Scheduled", "Completed", "Cancelled", "No-Show"
            };

        public AppointmentService(
            IAppointmentRepository repo,
            IScheduleService schedSvc,
            IPaymentService paySvc)
        {
            _repo = repo;
            _schedSvc = schedSvc;
            _paySvc = paySvc;
        }

        public async Task<Appointment> BookAppointmentAsync(Appointment appointment)
        {
            // --- UPDATED: COMPREHENSIVE SLOT VALIDATION ---
            var slot = await _schedSvc.GetSlotDetailsAsync(appointment.SlotId);
            if (slot == null)
            {
                throw new InvalidOperationException($"Slot {appointment.SlotId} not found.");
            }

            // 1. Availability Guard
            bool isBooked = slot.Value.GetProperty("isBooked").GetBoolean();
            bool isBlocked = slot.Value.GetProperty("isBlocked").GetBoolean();
            if (isBooked || isBlocked)
            {
                throw new InvalidOperationException($"Slot {appointment.SlotId} is not available (Booked or Blocked).");
            }

            // 2. Integrity Guard: Verify requested doctor and TIMES match the slot owner
            int slotProviderId = slot.Value.GetProperty("providerId").GetInt32();
            if (slotProviderId != appointment.ProviderId)
            {
                throw new InvalidOperationException(
                    "Integrity Mismatch: Slot belongs to a different Provider.");
            }

            // Verify Time/Date Accuracy
            var slotDate = DateOnly.Parse(slot.Value.GetProperty("date").GetString()!);
            var slotStart = TimeOnly.Parse(slot.Value.GetProperty("startTime").GetString()!);
            var slotEnd = TimeOnly.Parse(slot.Value.GetProperty("endTime").GetString()!);

            if (appointment.AppointmentDate != slotDate || 
                appointment.StartTime != slotStart || 
                appointment.EndTime != slotEnd)
            {
                throw new InvalidOperationException(
                    "Time Mismatch: The requested appointment time does not match the chosen Slot's official schedule.");
            }

            // Guard: prevent double-booking the same slot in OUR database
            var existing = _repo.FindBySlotId(appointment.SlotId);
            if (existing != null && existing.Status != "Cancelled")
                throw new InvalidOperationException(
                    $"Slot {appointment.SlotId} is already booked in our system by appointment {existing.AppointmentId}.");

            appointment.Status = "Scheduled";
            appointment.CreatedAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;

            _repo.Add(appointment);
            if (!_repo.SaveChanges())
                throw new Exception("Failed to persist the appointment.");

            // Notify schedule-service to mark slot as booked
            await _schedSvc.BookSlotAsync(appointment.SlotId);

            return appointment;
        }

        // Compatibility bridge
        public Appointment BookAppointment(Appointment appointment) =>
            BookAppointmentAsync(appointment).GetAwaiter().GetResult();

        public Appointment? GetById(int id) =>
            _repo.GetById(id);

        public List<Appointment> GetByPatient(int patientId) =>
            _repo.FindByPatientId(patientId);

        public List<Appointment> GetByProvider(int providerId) =>
            _repo.FindByProviderId(providerId);

        public List<Appointment> GetByProviderAndDate(int providerId, DateOnly date) =>
            _repo.FindByProviderIdAndAppointmentDate(providerId, date);

        public async Task CancelAppointmentAsync(int id)
        {
            var appt = _repo.GetById(id) ?? throw new KeyNotFoundException("Appointment not found.");

            if (appt.Status == "Completed")
                throw new InvalidOperationException("Cannot cancel a completed appointment.");

            if (appt.Status == "Cancelled")
                throw new InvalidOperationException("Appointment is already cancelled.");

            // --- STEP 1: TEll REMOTE SERVICE FIRST ---
            // If this fails, the local DB stays 'Scheduled' (Consistent)
            await _schedSvc.ReleaseSlotAsync(appt.SlotId);

            // --- STEP 2: UPDATE LOCAL DB ONLY IF REMOTE SUCCEEDED ---
            appt.SetStatus("Cancelled");
            appt.UpdatedAt = DateTime.UtcNow;

            _repo.Update(appt);
            _repo.SaveChanges();

            // Trigger refund via payment-service (stub for now)
            await _paySvc.TriggerRefundAsync(appt.AppointmentId);
        }

        public void CancelAppointment(int id) => 
            CancelAppointmentAsync(id).GetAwaiter().GetResult();

        public async Task<Appointment> RescheduleAppointmentAsync(int id, int newSlotId)
        {
            var appt = _repo.GetById(id) ?? throw new KeyNotFoundException("Appointment not found.");

            if (appt.Status != "Scheduled")
                throw new InvalidOperationException(
                    $"Only 'Scheduled' appointments can be rescheduled. Current status: {appt.Status}.");

            // Verify new slot
            var slot = await _schedSvc.GetSlotDetailsAsync(newSlotId);
            if (slot == null)
                throw new InvalidOperationException($"New slot {newSlotId} not found.");

            // 1. Availability Guard
            if (slot.Value.GetProperty("isBooked").GetBoolean() || 
                slot.Value.GetProperty("isBlocked").GetBoolean())
            {
                throw new InvalidOperationException($"New slot {newSlotId} is not available.");
            }

            // 2. Integrity Guard: Verify doctor
            if (slot.Value.GetProperty("providerId").GetInt32() != appt.ProviderId)
            {
                throw new InvalidOperationException(
                    "Integrity Mismatch: New slot belongs to a different Provider.");
            }

            // Guard: ensure new slot is not already taken in our DB
            var newSlotConflict = _repo.FindBySlotId(newSlotId);
            if (newSlotConflict != null && newSlotConflict.Status != "Cancelled")
                throw new InvalidOperationException($"New slot {newSlotId} is already booked in our system.");

            var oldSlotId = appt.SlotId;

            // --- STEP 1: PREPARE REMOTE SLOTS FIRST ---
            await _schedSvc.BookSlotAsync(newSlotId);      // Book new
            await _schedSvc.ReleaseSlotAsync(oldSlotId);   // Release old

            // --- STEP 2: UPDATE LOCAL DB ONLY IF REMOTE SUCCEEDED ---
            appt.SlotId = newSlotId;
            appt.AppointmentDate = DateOnly.Parse(slot.Value.GetProperty("date").GetString()!);
            appt.StartTime = TimeOnly.Parse(slot.Value.GetProperty("startTime").GetString()!);
            appt.EndTime = TimeOnly.Parse(slot.Value.GetProperty("endTime").GetString()!);
            appt.UpdatedAt = DateTime.UtcNow;

            _repo.Update(appt);
            _repo.SaveChanges();

            return appt;
        }

        public void CompleteAppointment(int id)
        {
            var appt = _repo.GetById(id) ?? throw new KeyNotFoundException("Appointment not found.");

            if (appt.Status != "Scheduled")
                throw new InvalidOperationException(
                    $"Only 'Scheduled' appointments can be marked complete. Current status: {appt.Status}.");

            appt.SetStatus("Completed");
            appt.UpdatedAt = DateTime.UtcNow;

            _repo.Update(appt);
            _repo.SaveChanges();
        }

        public async Task<string> UpdateStatusAsync(int id, string status)
        {
            if (!ValidStatuses.Contains(status))
                throw new ArgumentException(
                    $"Invalid status '{status}'. Valid values: Scheduled, Completed, Cancelled, No-Show.");

            var appt = _repo.GetById(id) ?? throw new KeyNotFoundException("Appointment not found.");

            appt.SetStatus(status);
            appt.UpdatedAt = DateTime.UtcNow;

            _repo.Update(appt);
            _repo.SaveChanges();

            return appt.Status;
        }

        public string UpdateStatus(int id, string status) =>
            UpdateStatusAsync(id, status).GetAwaiter().GetResult();

        public List<Appointment> GetUpcomingByPatient(int patientId) =>
            _repo.FindUpcomingByPatientId(patientId);

        public int GetAppointmentCount(int providerId) =>
            _repo.CountByProviderId(providerId);
    }
}
