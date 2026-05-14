using review_service.Interfaces;

namespace review_service.Services
{
    public class NotificationHttpService : INotificationHttpService
    {
        private readonly HttpClient _httpClient;
        private const string InternalServiceKey = "medibook-internal-service-key-2024";

        public NotificationHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task BroadcastDashboardEventAsync(string eventType, int? targetUserId = null, bool broadcastToAdmins = false)
        {
            SetInternalKey();
            var payload = new { EventType = eventType, TargetUserId = targetUserId, BroadcastToAdmins = broadcastToAdmins };
            try
            {
                await _httpClient.PostAsJsonAsync("api/v1/notifications/broadcast-event", payload);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to broadcast dashboard event '{eventType}'. {ex.Message}");
            }
        }

        private void SetInternalKey()
        {
            if (!_httpClient.DefaultRequestHeaders.Contains("X-Internal-Service-Key"))
            {
                _httpClient.DefaultRequestHeaders.Add("X-Internal-Service-Key", InternalServiceKey);
            }
        }
    }
}
