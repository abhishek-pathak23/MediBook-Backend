using appointment_service.Interfaces;

using System.Text.Json;

namespace appointment_service.Services
{
    public class NotificationHttpService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private const string InternalServiceKey = "medibook-internal-service-key-2024";

        public NotificationHttpService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task SendBookingConfirmationAsync(int patientId, int providerId, int appointmentId)
        {
            SetInternalKey();
            int providerUserId = await GetProviderUserIdAsync(providerId);

            var patientPayload = new
            {
                RecipientId = patientId,
                Type = "BOOKING",
                Title = "Appointment Booked",
                Message = $"Your appointment (ID: {appointmentId}) has been successfully booked.",
                Channel = "APP",
                RelatedId = appointmentId,
                RelatedType = "Appointment"
            };

            var providerPayload = new
            {
                RecipientId = providerUserId,
                Type = "NEW_APPOINTMENT",
                Title = "New Appointment",
                Message = $"A new appointment (ID: {appointmentId}) has been booked with you.",
                Channel = "APP",
                RelatedId = appointmentId,
                RelatedType = "Appointment"
            };

            await _httpClient.PostAsJsonAsync("api/v1/notifications", patientPayload);
            await _httpClient.PostAsJsonAsync("api/v1/notifications", providerPayload);
        }

        public async Task SendCancellationAlertAsync(int patientId, int providerId, int appointmentId)
        {
            SetInternalKey();
            int providerUserId = await GetProviderUserIdAsync(providerId);

            var patientPayload = new
            {
                RecipientId = patientId,
                Type = "CANCELLATION",
                Title = "Appointment Cancelled",
                Message = $"Your appointment (ID: {appointmentId}) has been cancelled.",
                Channel = "APP",
                RelatedId = appointmentId,
                RelatedType = "Appointment"
            };

            var providerPayload = new
            {
                RecipientId = providerUserId,
                Type = "CANCELLATION",
                Title = "Appointment Cancelled",
                Message = $"An appointment (ID: {appointmentId}) scheduled with you has been cancelled.",
                Channel = "APP",
                RelatedId = appointmentId,
                RelatedType = "Appointment"
            };

            await _httpClient.PostAsJsonAsync("api/v1/notifications", patientPayload);
            await _httpClient.PostAsJsonAsync("api/v1/notifications", providerPayload);
        }

        public async Task BroadcastDashboardEventAsync(string eventType, int? targetUserId = null, bool broadcastToAdmins = false)
        {
            SetInternalKey();
            var payload = new { EventType = eventType, TargetUserId = targetUserId, BroadcastToAdmins = broadcastToAdmins };
            await _httpClient.PostAsJsonAsync("api/v1/notifications/broadcast-event", payload);
        }

        private void SetInternalKey()
        {
            if (!_httpClient.DefaultRequestHeaders.Contains("X-Internal-Service-Key"))
            {
                _httpClient.DefaultRequestHeaders.Add("X-Internal-Service-Key", InternalServiceKey);
            }
        }

        private async Task<int> GetProviderUserIdAsync(int providerId)
        {
            try
            {
                using var client = new HttpClient();
                var gatewayUrl = _config["ServiceUrls:ApiGateway"] ?? "http://localhost:5000";
                // Alternatively, hit provider service directly
                var response = await client.GetAsync($"http://localhost:5117/api/v1/Provider/{providerId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("userId", out var userIdElement))
                    {
                        return userIdElement.GetInt32();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Provider UserId: {ex.Message}");
            }
            return providerId; // Fallback, though likely wrong
        }
    }
}
