using appointment_service.Entities;

namespace appointment_service.Interfaces
{
    public interface IAppointmentService
    {
        Appointment BookAppointment(Appointment appointment);
        Appointment? GetById(int id);
        List<Appointment> GetByPatient(int patientId);
        List<Appointment> GetByProvider(int providerId);
        List<Appointment> GetByProviderAndDate(int providerId, DateOnly date);
        void CancelAppointment(int id);
        Appointment RescheduleAppointment(int id, int newSlotId);
        void CompleteAppointment(int id);
        string UpdateStatus(int id, string status);
        List<Appointment> GetUpcomingByPatient(int patientId);
        int GetAppointmentCount(int providerId);
    }
}
