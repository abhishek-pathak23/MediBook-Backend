using appointment_service.Interfaces;
using System.Net.Http.Headers;

namespace appointment_service.Services
{
    public class NotificationHttpService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationHttpService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SendBookingConfirmationAsync(int patientId, int providerId, int appointmentId)
        {
            PropagateAuthorizationHeader();

            var payload = new
            {
                RecipientId = patientId,
                Type = "BOOKING",
                Title = "Appointment Confirmed",
                Message = $"Your appointment (ID: {appointmentId}) has been successfully booked.",
                Channel = "APP",
                RelatedId = appointmentId,
                RelatedType = "Appointment"
            };

            await _httpClient.PostAsJsonAsync("api/v1/notifications", payload);
        }

        public async Task SendCancellationAlertAsync(int patientId, int providerId, int appointmentId)
        {
            PropagateAuthorizationHeader();

            var payload = new
            {
                RecipientId = patientId,
                Type = "CANCELLATION",
                Title = "Appointment Cancelled",
                Message = $"Your appointment (ID: {appointmentId}) has been cancelled.",
                Channel = "APP",
                RelatedId = appointmentId,
                RelatedType = "Appointment"
            };

            await _httpClient.PostAsJsonAsync("api/v1/notifications", payload);
        }

        private void PropagateAuthorizationHeader()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString().Replace("Bearer ", "");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
