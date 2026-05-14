using review_service.Interfaces;

namespace review_service.Services
{
    public class ProviderHttpService : IProviderHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProviderHttpService> _logger;
        private const string InternalServiceKey = "medibook-internal-service-key-2024";

        public ProviderHttpService(HttpClient httpClient, ILogger<ProviderHttpService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task UpdateProviderRatingAsync(int providerId, double avgRating)
        {
            SetInternalKey();

            try
            {
                var response = await _httpClient.PutAsJsonAsync(
                    $"api/v1/Provider/{providerId}/rating",
                    new { AvgRating = avgRating });

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Updated Provider {ProviderId} rating to {AvgRating}",
                        providerId, avgRating);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "Failed to update Provider {ProviderId} rating. Status: {StatusCode}. Detail: {Error}",
                        providerId, response.StatusCode, error);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "Failed to connect to Provider-Service to update rating for Provider {ProviderId}",
                    providerId);
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
