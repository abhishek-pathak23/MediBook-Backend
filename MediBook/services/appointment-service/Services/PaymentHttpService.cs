using appointment_service.Interfaces;

namespace appointment_service.Services
{
    /// <summary>
    /// Stub implementation of IPaymentService.
    /// Payment-Service will be built in a future sprint.
    /// Replace this with a real HTTP client once available.
    /// </summary>
    public class PaymentHttpService : IPaymentService
    {
        private readonly ILogger<PaymentHttpService> _logger;

        public PaymentHttpService(ILogger<PaymentHttpService> logger)
        {
            _logger = logger;
        }

        public Task TriggerRefundAsync(int appointmentId)
        {
            // TODO: Replace with real HTTP call to Payment-Service when available.
            _logger.LogInformation(
                "[STUB] TriggerRefundAsync called for AppointmentId={AppointmentId}. Payment-Service not yet implemented.",
                appointmentId);

            return Task.CompletedTask;
        }
    }
}
