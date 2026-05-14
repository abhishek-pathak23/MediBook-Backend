using payment_service.DTOs;
using payment_service.Interfaces;
using System.Net.Http.Headers;

namespace payment_service.Services
{
    public class AppointmentHttpService : IAppointmentHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AppointmentHttpService> _logger;

        public AppointmentHttpService(
            HttpClient httpClient, 
            IHttpContextAccessor httpContextAccessor, 
            ILogger<AppointmentHttpService> logger,
            IConfiguration config)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

            var baseUrl = config["ServiceUrls:AppointmentService"] 
                          ?? throw new InvalidOperationException("AppointmentService URL not found");
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<AppointmentResponseDto?> GetAppointmentDetailsAsync(int appointmentId)
        {
            _logger.LogInformation("Calling Appointment-Service for ID: {Id}", appointmentId);
            
            AddAuthorizationHeader();

            try
            {
                var response = await _httpClient.GetAsync($"/api/v1/appointments/{appointmentId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Appointment {Id} not found or error occurred. Status: {Status}", 
                        appointmentId, response.StatusCode);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<AppointmentResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Appointment-Service.");
                throw;
            }
        }

        public async Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string status)
        {
            _logger.LogInformation("Updating Appointment {Id} status to {Status}", appointmentId, status);
            
            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/appointments/internal/{appointmentId}/status");
            request.Headers.Add("X-Internal-Service-Key", "MediBookInternalSync");
            request.Content = JsonContent.Create(new { status });

            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        private void AddAuthorizationHeader()
        {
            // Propagate the JWT token from the current request to the inter-service call
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString().Replace("Bearer ", "");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
