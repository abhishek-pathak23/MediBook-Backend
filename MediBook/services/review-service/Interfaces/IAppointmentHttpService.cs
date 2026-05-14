using review_service.DTOs;

namespace review_service.Interfaces
{
    public interface IAppointmentHttpService
    {
        Task<AppointmentResponseDto?> GetAppointmentDetailsAsync(int appointmentId);
    }
}
