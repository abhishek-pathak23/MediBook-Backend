using notification_service.DTOs;
using notification_service.Entities;

namespace notification_service.Interfaces
{
    public interface INotificationService
    {
        Task Send(Notification notification);
        Task SendBulk(List<int> recipientIds, Notification notificationTemplate);
        Task BroadcastDashboardEventAsync(string eventType, int? targetUserId = null, bool broadcastToAdmins = false);
        void MarkAsRead(int notificationId);
        void MarkAllRead(int recipientId);
        List<NotificationResponseDto> GetByRecipient(int recipientId);
        int GetUnreadCount(int recipientId);
        bool DeleteNotification(int notificationId);
        NotificationResponseDto? GetById(int id);
        List<NotificationResponseDto> GetAll();
    }
}
