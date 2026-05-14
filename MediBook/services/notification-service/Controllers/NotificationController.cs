using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using notification_service.DTOs;
using notification_service.Entities;
using notification_service.Interfaces;
using System.Security.Claims;

namespace notification_service.Controllers
{
    [ApiController]
    [Route("api/v1/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notifService;

        public NotificationController(INotificationService notifService)
        {
            _notifService = notifService;
        }

        [HttpPost("send")]
        [Authorize(Roles = "Admin")]
        // Restricted to Admin. In production, other services use a service-account Admin token.
        public async Task<IActionResult> Send([FromBody] NotificationCreateDto dto)
        {
            var notif = new Notification
            {
                RecipientId = dto.RecipientId,
                Type = dto.Type,
                Title = dto.Title,
                Message = dto.Message,
                Channel = dto.Channel,
                RelatedId = dto.RelatedId,
                RelatedType = dto.RelatedType
            };

            await _notifService.Send(notif);
            return StatusCode(201, new { message = "Notification sent successfully." });
        }

        /// <summary>
        /// Internal-only endpoint for service-to-service notification creation.
        /// Secured via X-Internal-Service-Key header instead of JWT.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> InternalSend([FromBody] NotificationCreateDto dto)
        {
            const string expectedKey = "medibook-internal-service-key-2024";
            if (!Request.Headers.TryGetValue("X-Internal-Service-Key", out var key) || key != expectedKey)
                return StatusCode(403, new { message = "Invalid or missing internal service key." });

            var notif = new Notification
            {
                RecipientId = dto.RecipientId,
                Type = dto.Type,
                Title = dto.Title,
                Message = dto.Message,
                Channel = dto.Channel,
                RelatedId = dto.RelatedId,
                RelatedType = dto.RelatedType
            };

            await _notifService.Send(notif);
            return StatusCode(201, new { message = "Notification sent successfully." });
        }

        /// <summary>
        /// Internal-only endpoint for service-to-service SignalR dashboard pushes.
        /// </summary>
        [HttpPost("broadcast-event")]
        [AllowAnonymous]
        public async Task<IActionResult> BroadcastEvent([FromBody] DashboardEventDto dto)
        {
            const string expectedKey = "medibook-internal-service-key-2024";
            if (!Request.Headers.TryGetValue("X-Internal-Service-Key", out var key) || key != expectedKey)
                return StatusCode(403, new { message = "Invalid or missing internal service key." });

            await _notifService.BroadcastDashboardEventAsync(dto.EventType, dto.TargetUserId, dto.BroadcastToAdmins);
            return Ok(new { message = $"Event '{dto.EventType}' broadcasted." });
        }


        [HttpPost("send/bulk")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendBulk([FromBody] BulkNotificationDto dto)
        {
            var template = new Notification
            {
                Type = "PLATFORM_ANNOUNCEMENT",
                Title = dto.Title,
                Message = dto.Message,
                Channel = dto.Channel
            };
            
            await _notifService.SendBulk(dto.RecipientIds, template);
            return Ok(new { message = $"Dispatched {dto.RecipientIds.Count} notifications." });
        }

        [HttpGet]
        public IActionResult GetMyNotifications()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            return Ok(_notifService.GetByRecipient(userId));
        }

        [HttpGet("{recipientId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetByRecipient(int recipientId)
        {
            return Ok(_notifService.GetByRecipient(recipientId));
        }

        [HttpGet("unreadCount")]
        public IActionResult GetMyUnreadCount()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var count = _notifService.GetUnreadCount(userId);
            return Ok(new { unreadCount = count });
        }

        [HttpPut("{id}/read")]
        public IActionResult MarkAsRead(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            // Fetch the notification first
            var notification = _notifService.GetById(id);
            if (notification == null)
                return NotFound(new { message = $"Notification with ID {id} not found." });

            // Ownership check: only the owner OR an Admin can mark it as read
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && notification.RecipientId != userId)
                return StatusCode(403, new { message = "You are not allowed to mark another user's notification as read." });

            _notifService.MarkAsRead(id);
            return Ok(new { message = "Marked as read." });
        }

        [HttpPut("readAll")]
        public IActionResult MarkAllRead()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            _notifService.MarkAllRead(userId);
            return Ok(new { message = "All notifications marked as read." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var deleted = _notifService.DeleteNotification(id);
            if (!deleted) return NotFound(new { message = $"Notification with ID {id} not found." });
            return NoContent();
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            return Ok(_notifService.GetAll());
        }
    }
}
