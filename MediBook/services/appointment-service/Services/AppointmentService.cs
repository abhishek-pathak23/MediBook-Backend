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

        public Appointment BookAppointment(Appointment appointment)
        {
            // Guard: prevent double-booking the same slot
            var existing = _repo.FindBySlotId(appointment.SlotId);
            if (existing != null && existing.Status != "Cancelled")
                throw new InvalidOperationException(
                    $"Slot {appointment.SlotId} is already booked by appointment {existing.AppointmentId}.");

            appointment.Status = "Scheduled";
            appointment.CreatedAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;

            _repo.Add(appointment);
            if (!_repo.SaveChanges())
                throw new Exception("Failed to persist the appointment.");

            // Notify schedule-service to mark slot as booked
            _schedSvc.BookSlotAsync(appointment.SlotId).GetAwaiter().GetResult();

            return appointment;
        }

        public Appointment? GetById(int id) =>
            _repo.GetById(id);

        public List<Appointment> GetByPatient(int patientId) =>
            _repo.FindByPatientId(patientId);

        public List<Appointment> GetByProvider(int providerId) =>
            _repo.FindByProviderId(providerId);

        public List<Appointment> GetByProviderAndDate(int providerId, DateOnly date) =>
            _repo.FindByProviderIdAndAppointmentDate(providerId, date);

        public void CancelAppointment(int id)
        {
            var appt = _repo.GetById(id) ?? throw new KeyNotFoundException("Appointment not found.");

            if (appt.Status == "Completed")
                throw new InvalidOperationException("Cannot cancel a completed appointment.");

            if (appt.Status == "Cancelled")
                throw new InvalidOperationException("Appointment is already cancelled.");

            appt.SetStatus("Cancelled");
            appt.UpdatedAt = DateTime.UtcNow;

            _repo.Update(appt);
            _repo.SaveChanges();

            // Release the slot back in schedule-service
            _schedSvc.ReleaseSlotAsync(appt.SlotId).GetAwaiter().GetResult();

            // Trigger refund via payment-service (stub for now)
            _paySvc.TriggerRefundAsync(appt.AppointmentId).GetAwaiter().GetResult();
        }

        public Appointment RescheduleAppointment(int id, int newSlotId)
        {
            var appt = _repo.GetById(id) ?? throw new KeyNotFoundException("Appointment not found.");

            if (appt.Status != "Scheduled")
                throw new InvalidOperationException(
                    $"Only 'Scheduled' appointments can be rescheduled. Current status: {appt.Status}.");

            // Guard: ensure new slot is not already taken
            var newSlotConflict = _repo.FindBySlotId(newSlotId);
            if (newSlotConflict != null && newSlotConflict.Status != "Cancelled")
                throw new InvalidOperationException($"New slot {newSlotId} is already booked.");

            var oldSlotId = appt.SlotId;

            // Update appointment to new slot
            appt.SlotId = newSlotId;
            appt.UpdatedAt = DateTime.UtcNow;

            _repo.Update(appt);
            _repo.SaveChanges();

            // Release old slot and book new slot
            _schedSvc.ReleaseSlotAsync(oldSlotId).GetAwaiter().GetResult();
            _schedSvc.BookSlotAsync(newSlotId).GetAwaiter().GetResult();

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

        public string UpdateStatus(int id, string status)
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

        public List<Appointment> GetUpcomingByPatient(int patientId) =>
            _repo.FindUpcomingByPatientId(patientId);

        public int GetAppointmentCount(int providerId) =>
            _repo.CountByProviderId(providerId);
    }
}
