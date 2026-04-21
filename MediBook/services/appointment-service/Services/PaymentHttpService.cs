using appointment_service.Interfaces;
using System.Net.Http.Headers;

namespace appointment_service.Services
{
    /// <summary>
    /// Communicates with the Payment-Service via IHttpClientFactory.
    /// Calls POST /api/v1/payments/refund/{appointmentId} to trigger a refund
    /// when an appointment is cancelled.
    /// </summary>
    public class PaymentHttpService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PaymentHttpService> _logger;

        public PaymentHttpService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PaymentHttpService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task TriggerRefundAsync(int appointmentId)
        {
            _logger.LogInformation(
                "Triggering refund via Payment-Service for AppointmentId={AppointmentId}",
                appointmentId);

            PropagateAuthorizationHeader();

            try
            {
                var response = await _httpClient.PostAsync(
                    $"api/v1/payments/refund/{appointmentId}", null);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Refund successfully triggered for AppointmentId={AppointmentId}",
                        appointmentId);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "Payment-Service refund returned {StatusCode} for AppointmentId={AppointmentId}. Detail: {Error}",
                        response.StatusCode, appointmentId, error);
                    // Non-critical: log but don't throw — appointment cancellation should still succeed
                    // The refund can be retried manually via Payment-Service
                }
            }
            catch (HttpRequestException ex)
            {
                // Payment-Service might be down — log but don't block the cancellation
                _logger.LogError(ex,
                    "Failed to connect to Payment-Service for refund. AppointmentId={AppointmentId}",
                    appointmentId);
            }
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
