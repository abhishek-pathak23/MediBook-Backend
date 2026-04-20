using appointment_service.Entities;

namespace appointment_service.Interfaces
{
    public interface IAppointmentRepository
    {
        // Query methods (from class diagram)
        List<Appointment> FindByPatientId(int patientId);
        List<Appointment> FindByProviderId(int providerId);
        Appointment? FindBySlotId(int slotId);
        List<Appointment> FindByStatus(string status);
        List<Appointment> FindByProviderIdAndAppointmentDate(int providerId, DateOnly date);
        List<Appointment> FindUpcomingByPatientId(int patientId);
        int CountByProviderId(int providerId);

        // Standard CRUD
        void Add(Appointment appointment);
        void Update(Appointment appointment);
        void Delete(Appointment appointment);
        Appointment? GetById(int appointmentId);
        bool SaveChanges();
    }
}
