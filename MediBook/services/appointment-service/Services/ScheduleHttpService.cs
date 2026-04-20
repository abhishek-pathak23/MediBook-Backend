using appointment_service.Interfaces;

namespace appointment_service.Services
{
    /// <summary>
    /// Communicates with the Schedule-Service via IHttpClientFactory.
    /// Calls PUT /api/v1/schedule/{id}/book to mark a slot as booked,
    /// and PUT /api/v1/schedule/{id}/release to free a slot on cancellation.
    /// </summary>
    public class ScheduleHttpService : IScheduleService
    {
        private readonly HttpClient _httpClient;

        public ScheduleHttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task BookSlotAsync(int slotId)
        {
            var response = await _httpClient.PutAsync($"api/v1/schedule/{slotId}/book", null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Schedule-Service failed to book slot {slotId}. Status: {response.StatusCode}. Detail: {error}");
            }
        }

        public async Task ReleaseSlotAsync(int slotId)
        {
            var response = await _httpClient.PutAsync($"api/v1/schedule/{slotId}/release", null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Schedule-Service failed to release slot {slotId}. Status: {response.StatusCode}. Detail: {error}");
            }
        }

        public async Task<bool> IsSlotAvailableAsync(int slotId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/schedule/{slotId}");
                if (!response.IsSuccessStatusCode) return false;

                // Use JsonElement for safer dynamic parsing
                var slot = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                
                bool isBooked = slot.GetProperty("isBooked").GetBoolean();
                bool isBlocked = slot.GetProperty("isBlocked").GetBoolean();

                // It's available only if it's NOT booked AND NOT blocked
                return !isBooked && !isBlocked;
            }
            catch (Exception ex)
            {
                // If anything fails (like a missing property), we assume it's "not safe to book"
                return false;
            }
        }
    }
}
