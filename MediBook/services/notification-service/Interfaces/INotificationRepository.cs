using notification_service.Entities;

namespace notification_service.Interfaces
{
    public interface INotificationRepository
    {
        List<Notification> FindByRecipientId(int recipientId);
        List<Notification> FindByRecipientIdAndIsRead(int recipientId, bool isRead);
        int CountByRecipientIdAndIsRead(int recipientId, bool isRead);
        List<Notification> FindByType(string type);
        List<Notification> FindByRelatedId(int relatedId);
        void DeleteByNotificationId(int notificationId);
        void Add(Notification notification);
        Notification? GetById(int id);
        void Update(Notification notification);
        bool SaveChanges();
        List<Notification> GetAll();
    }
}
