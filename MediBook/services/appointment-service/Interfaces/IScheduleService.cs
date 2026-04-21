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
        
        /// <summary>
        /// Returns slot details (ProviderId, IsBooked, IsBlocked) or null if not found.
        /// </summary>
        Task<System.Text.Json.JsonElement?> GetSlotDetailsAsync(int slotId);
    }
}
