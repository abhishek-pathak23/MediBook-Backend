namespace review_service.Interfaces
{
    public interface INotificationHttpService
    {
        Task BroadcastDashboardEventAsync(string eventType, int? targetUserId = null, bool broadcastToAdmins = false);
    }
}
