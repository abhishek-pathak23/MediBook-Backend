using appointment_service.Interfaces;
using System.Net.Http.Headers;

namespace appointment_service.Services
{
    /// <summary>
    /// Communicates with the Schedule-Service via IHttpClientFactory.
    /// Calls PUT /api/v1/schedule/{id}/book to mark a slot as booked,
    /// PUT /api/v1/schedule/{id}/release to free a slot on cancellation,
    /// and GET /api/v1/schedule/{id} to verify slot availability.
    /// </summary>
    public class ScheduleHttpService : IScheduleService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ScheduleHttpService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task BookSlotAsync(int slotId)
        {
            PropagateAuthorizationHeader();

            var response = await _httpClient.PutAsync($"api/v1/Schedule/{slotId}/book", null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Schedule-Service failed to book slot {slotId}. Status: {response.StatusCode}. Detail: {error}");
            }
        }

        public async Task ReleaseSlotAsync(int slotId)
        {
            PropagateAuthorizationHeader();

            var response = await _httpClient.PutAsync($"api/v1/Schedule/{slotId}/release", null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Schedule-Service failed to release slot {slotId}. Status: {response.StatusCode}. Detail: {error}");
            }
        }

        public async Task<System.Text.Json.JsonElement?> GetSlotDetailsAsync(int slotId)
        {
            PropagateAuthorizationHeader();

            try
            {
                var response = await _httpClient.GetAsync($"api/v1/Schedule/{slotId}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            }
            catch (Exception)
            {
                return null;
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
