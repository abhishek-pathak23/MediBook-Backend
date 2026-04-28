namespace notification_service.DTOs
{
    public class NotificationResponseDto
    {
        public int NotificationId { get; set; }
        public int RecipientId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public int? RelatedId { get; set; }
        public string? RelatedType { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
    }

    public class NotificationCreateDto
    {
        public int RecipientId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public int? RelatedId { get; set; }
        public string? RelatedType { get; set; }
    }

    public class BulkNotificationDto
    {
        public List<int> RecipientIds { get; set; } = new List<int>();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
    }

    public class DashboardEventDto
    {
        public string EventType { get; set; } = string.Empty;
        public int? TargetUserId { get; set; }
        public bool BroadcastToAdmins { get; set; }
    }
}
