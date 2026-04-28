namespace appointment_service.Interfaces
{
    public interface INotificationService
    {
        Task SendBookingConfirmationAsync(int patientId, int providerId, int appointmentId);
        Task SendCancellationAlertAsync(int patientId, int providerId, int appointmentId);
        Task BroadcastDashboardEventAsync(string eventType, int? targetUserId = null, bool broadcastToAdmins = false);
    }
}
