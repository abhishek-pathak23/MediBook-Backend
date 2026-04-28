using appointment_service.Interfaces;

namespace appointment_service.Services
{
    /// <summary>
    /// Communicates with the Payment-Service via IHttpClientFactory.
    /// Calls POST /api/v1/payments/internal/refund/{appointmentId} to trigger a refund
    /// when an appointment is cancelled.
    /// </summary>
    public class PaymentHttpService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentHttpService> _logger;
        private const string InternalServiceKey = "medibook-internal-service-key-2024";

        public PaymentHttpService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<PaymentHttpService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task TriggerRefundAsync(int appointmentId)
        {
            _logger.LogInformation(
                "Triggering refund via Payment-Service for AppointmentId={AppointmentId}",
                appointmentId);

            SetInternalKey();

            try
            {
                var response = await _httpClient.PostAsync(
                    $"api/v1/payments/internal/refund/{appointmentId}", null);

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
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "Failed to connect to Payment-Service for refund. AppointmentId={AppointmentId}",
                    appointmentId);
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

