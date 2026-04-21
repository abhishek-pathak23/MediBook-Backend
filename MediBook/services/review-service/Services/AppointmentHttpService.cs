using System.Net.Http.Headers;
using review_service.DTOs;
using review_service.Interfaces;

namespace review_service.Services
{
    public class AppointmentHttpService : IAppointmentHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppointmentHttpService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            
            var baseUrl = config["ServiceUrls:AppointmentService"] ?? "http://localhost:5003/";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<AppointmentResponseDto?> GetAppointmentDetailsAsync(int appointmentId)
        {
            AddAuthorizationHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/appointments/{appointmentId}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<AppointmentResponseDto>();
            }
            catch
            {
                return null;
            }
        }

        private void AddAuthorizationHeader()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString().Replace("Bearer ", "");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
