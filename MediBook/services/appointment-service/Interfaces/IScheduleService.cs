namespace appointment_service.Interfaces
{
    /// <summary>
    /// HTTP client abstraction for communicating with the Schedule-Service.
    /// Used by AppointmentService to mark slots as booked/released via IHttpClientFactory.
    /// </summary>
    public interface IScheduleService
    {
        Task BookSlotAsync(int slotId);
        Task ReleaseSlotAsync(int slotId);
    }
}
