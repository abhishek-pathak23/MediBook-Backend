namespace appointment_service.Interfaces
{
    /// <summary>
    /// HTTP client abstraction for communicating with the Payment-Service.
    /// Stub implementation — Payment-Service will be built in a future sprint.
    /// </summary>
    public interface IPaymentService
    {
        Task TriggerRefundAsync(int appointmentId);
    }
}
