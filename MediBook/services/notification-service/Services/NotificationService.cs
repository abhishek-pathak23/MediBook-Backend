using MailKit.Net.Smtp;
using Microsoft.AspNetCore.SignalR;
using MimeKit;
using notification_service.DTOs;
using notification_service.Entities;
using notification_service.Hubs;
using notification_service.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace notification_service.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IHubContext<NotifHub> _hub;
        private readonly IConfiguration _config;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository repo, 
            IHubContext<NotifHub> hub, 
            IConfiguration config,
            ILogger<NotificationService> logger)
        {
            _repo = repo;
            _hub = hub;
            _config = config;
            _logger = logger;
        }

        public async Task Send(Notification notification)
        {
            // Always save to database for history
            _repo.Add(notification);
            _repo.SaveChanges();

            // Dispatch based on channel
            try
            {
                switch (notification.Channel.ToUpper())
                {
                    case "APP":
                        await SendInApp(notification);
                        break;
                    case "EMAIL":
                        await SendEmail(notification);
                        break;
                    case "SMS":
                        await SendSms(notification);
                        break;
                    default:
                        _logger.LogWarning($"Unknown channel: {notification.Channel}. Defaulting to APP.");
                        await SendInApp(notification);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send {notification.Channel} notification to Recipient {notification.RecipientId}.");
                // We catch to avoid crashing the transaction. The notification is safely logged in the DB.
            }
        }

        public async Task SendBulk(List<int> recipientIds, Notification notificationTemplate)
        {
            foreach (var recipientId in recipientIds)
            {
                var notif = new Notification
                {
                    RecipientId = recipientId,
                    Type = notificationTemplate.Type,
                    Title = notificationTemplate.Title,
                    Message = notificationTemplate.Message,
                    Channel = notificationTemplate.Channel,
                    RelatedId = notificationTemplate.RelatedId,
                    RelatedType = notificationTemplate.RelatedType
                };
                await Send(notif);
            }
        }

        private async Task SendInApp(Notification notification)
        {
            // Using SignalR to push to the connected user. The UserId matches RecipientId.
            await _hub.Clients.User(notification.RecipientId.ToString()).SendAsync("ReceiveNotification", new 
            {
                notification.NotificationId,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.SentAt
            });
            _logger.LogInformation($"(MOCK) In-App Notification sent to {notification.RecipientId}: {notification.Title}");
        }

        private async Task SendEmail(Notification notification)
        {
            // Note: In a real environment, you look up the Recipient's Email address from Auth-Service.
            // For architecture demonstration, we assume a placeholder email.
            var toEmail = $"user{notification.RecipientId}@medibook.com";

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("MediBook Alerts", _config["EmailSettings:From"] ?? "noreply@medibook.com"));
            emailMessage.To.Add(new MailboxAddress("MediBook User", toEmail));
            emailMessage.Subject = notification.Title;
            emailMessage.Body = new TextPart("plain") { Text = notification.Message };

            var host = _config["EmailSettings:Host"];
            var portStr = _config["EmailSettings:Port"];
            var user = _config["EmailSettings:Username"];
            var pass = _config["EmailSettings:Password"];

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                _logger.LogInformation($"(MOCK) Email dispatched to {toEmail}. Title: {notification.Title}");
                return;
            }

            using var client = new SmtpClient();
            await client.ConnectAsync(host, int.Parse(portStr!), MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(user, pass);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
            
            _logger.LogInformation($"Email dispatched to {toEmail}");
        }

        private Task SendSms(Notification notification)
        {
            // Note: Recipient phone lookups would happen here.
            var toPhone = $"+1555000{notification.RecipientId.ToString().PadLeft(4, '0')}";

            var accountSid = _config["TwilioSettings:AccountSid"];
            var authToken = _config["TwilioSettings:AuthToken"];
            var fromPhone = _config["TwilioSettings:FromPhone"];

            if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
            {
                 _logger.LogInformation($"(MOCK) SMS dispatched to {toPhone}. Message: {notification.Message}");
                 return Task.CompletedTask;
            }

            TwilioClient.Init(accountSid, authToken);
            MessageResource.Create(
                body: notification.Message,
                from: new PhoneNumber(fromPhone),
                to: new PhoneNumber(toPhone)
            );

            _logger.LogInformation($"SMS dispatched to {toPhone}");
            return Task.CompletedTask;
        }

        public void MarkAsRead(int notificationId)
        {
            var notif = _repo.GetById(notificationId);
            if (notif != null && !notif.IsRead)
            {
                notif.IsRead = true;
                _repo.Update(notif);
                _repo.SaveChanges();
            }
        }

        public void MarkAllRead(int recipientId)
        {
            var unread = _repo.FindByRecipientIdAndIsRead(recipientId, false);
            foreach (var notif in unread)
            {
                notif.IsRead = true;
                _repo.Update(notif);
            }
            if (unread.Any())
            {
                _repo.SaveChanges();
            }
        }

        public List<NotificationResponseDto> GetByRecipient(int recipientId)
        {
            return _repo.FindByRecipientId(recipientId).Select(MapToDto).ToList();
        }

        public int GetUnreadCount(int recipientId)
        {
            return _repo.CountByRecipientIdAndIsRead(recipientId, false);
        }

        public bool DeleteNotification(int notificationId)
        {
            var notif = _repo.GetById(notificationId);
            if (notif == null) return false;

            _repo.DeleteByNotificationId(notificationId);
            _repo.SaveChanges();
            return true;
        }

        public List<NotificationResponseDto> GetAll()
        {
            return _repo.GetAll().Select(MapToDto).ToList();
        }

        public NotificationResponseDto? GetById(int id)
        {
            var notif = _repo.GetById(id);
            return notif == null ? null : MapToDto(notif);
        }

        private NotificationResponseDto MapToDto(Notification notif)
        {
            return new NotificationResponseDto
            {
                NotificationId = notif.NotificationId,
                RecipientId = notif.RecipientId,
                Type = notif.Type,
                Title = notif.Title,
                Message = notif.Message,
                Channel = notif.Channel,
                RelatedId = notif.RelatedId,
                RelatedType = notif.RelatedType,
                IsRead = notif.IsRead,
                SentAt = notif.SentAt
            };
        }
    }
}
