using appointment_service.Entities;

namespace appointment_service.Interfaces
{
    public interface IAppointmentService
    {
        Task<Appointment> BookAppointmentAsync(Appointment appointment);
        Appointment BookAppointment(Appointment appointment); // Keeping for compatibility
        Appointment? GetById(int id);
        List<Appointment> GetByPatient(int patientId);
        List<Appointment> GetByProvider(int providerId);
        List<Appointment> GetByProviderAndDate(int providerId, DateOnly date);
        Task CancelAppointmentAsync(int id);
        Task<Appointment> RescheduleAppointmentAsync(int id, int newSlotId);
        void CompleteAppointment(int id);
        Task<string> UpdateStatusAsync(int id, string status);
        List<Appointment> GetUpcomingByPatient(int patientId);
        int GetAppointmentCount(int providerId);
        List<Appointment> GetAllAppointments();
    }
}
