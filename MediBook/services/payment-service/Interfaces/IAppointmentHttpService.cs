using payment_service.DTOs;

namespace payment_service.Interfaces
{
    public interface IAppointmentHttpService
    {
        Task<AppointmentResponseDto?> GetAppointmentDetailsAsync(int appointmentId);
        Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string status);
    }
}
