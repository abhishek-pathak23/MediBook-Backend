using notification_service.Data;
using notification_service.Entities;
using notification_service.Interfaces;

namespace notification_service.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(Notification notification)
        {
            _context.Notifications.Add(notification);
        }

        public int CountByRecipientIdAndIsRead(int recipientId, bool isRead)
        {
            return _context.Notifications.Count(n => n.RecipientId == recipientId && n.IsRead == isRead);
        }

        public void DeleteByNotificationId(int notificationId)
        {
            var notification = _context.Notifications.Find(notificationId);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
            }
        }

        public List<Notification> FindByRecipientId(int recipientId)
        {
            return _context.Notifications
                .Where(n => n.RecipientId == recipientId)
                .OrderByDescending(n => n.SentAt)
                .ToList();
        }

        public List<Notification> FindByRecipientIdAndIsRead(int recipientId, bool isRead)
        {
            return _context.Notifications
                .Where(n => n.RecipientId == recipientId && n.IsRead == isRead)
                .OrderByDescending(n => n.SentAt)
                .ToList();
        }

        public List<Notification> FindByRelatedId(int relatedId)
        {
            return _context.Notifications.Where(n => n.RelatedId == relatedId).ToList();
        }

        public List<Notification> FindByType(string type)
        {
            return _context.Notifications.Where(n => n.Type == type).ToList();
        }

        public Notification? GetById(int id)
        {
            return _context.Notifications.Find(id);
        }

        public void Update(Notification notification)
        {
            _context.Notifications.Update(notification);
        }

        public List<Notification> GetAll()
        {
            return _context.Notifications.OrderByDescending(n => n.SentAt).ToList();
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() >= 0;
        }
    }
}
