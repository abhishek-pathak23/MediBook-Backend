using appointment_service.Data;
using appointment_service.Entities;
using appointment_service.Interfaces;

namespace appointment_service.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(Appointment appointment) =>
            _context.Appointments.Add(appointment);

        public void Update(Appointment appointment) =>
            _context.Appointments.Update(appointment);

        public void Delete(Appointment appointment) =>
            _context.Appointments.Remove(appointment);

        public Appointment? GetById(int appointmentId) =>
            _context.Appointments.Find(appointmentId);

        public bool SaveChanges() =>
            _context.SaveChanges() >= 0;

        public List<Appointment> FindByPatientId(int patientId) =>
            _context.Appointments
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToList();

        public List<Appointment> FindByProviderId(int providerId) =>
            _context.Appointments
                .Where(a => a.ProviderId == providerId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToList();

        public Appointment? FindBySlotId(int slotId) =>
            _context.Appointments
                .FirstOrDefault(a => a.SlotId == slotId);

        public List<Appointment> FindByStatus(string status) =>
            _context.Appointments
                .Where(a => a.Status == status)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

        public List<Appointment> FindByProviderIdAndAppointmentDate(int providerId, DateOnly date) =>
            _context.Appointments
                .Where(a => a.ProviderId == providerId && a.AppointmentDate == date)
                .OrderBy(a => a.StartTime)
                .ToList();

        public List<Appointment> FindUpcomingByPatientId(int patientId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            return _context.Appointments
                .Where(a => a.PatientId == patientId
                         && a.AppointmentDate >= today
                         && a.Status == "Scheduled")
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToList();
        }

        public int CountByProviderId(int providerId) =>
            _context.Appointments.Count(a => a.ProviderId == providerId);
    }
}
