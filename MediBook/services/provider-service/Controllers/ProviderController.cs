using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using provider_service.DTOs;
using provider_service.Entities;
using provider_service.Interfaces;

namespace provider_service.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProviderController : ControllerBase
    {
        private readonly IProviderService _providerService;

        public ProviderController(IProviderService providerService)
        {
            _providerService = providerService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Provider")]
        public IActionResult Register([FromBody] ProviderRegistrationDto dto)
        {
            var provider = _providerService.RegisterProvider(dto);
            return CreatedAtAction(nameof(GetById), new { id = provider.ProviderId }, provider);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var provider = _providerService.GetProviderById(id);
            if (provider == null) return NotFound("Provider not found");
            return Ok(provider);
        }

        [HttpGet("specialization/{specialization}")]
        public IActionResult GetBySpecialization(string specialization)
        {
            var providers = _providerService.GetBySpecialization(specialization);
            return Ok(providers);
        }

        [HttpGet("search")]
        public IActionResult Search([FromQuery] string query)
        {
            var providers = _providerService.SearchProviders(query);
            return Ok(providers);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var providers = _providerService.GetAllProviders();
            return Ok(providers);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Provider,Admin")]
        public IActionResult Update(int id, [FromBody] ProviderUpdateDto dto)
        {
            var provider = _providerService.UpdateProvider(id, dto);
            return Ok(provider);
        }

        [HttpPut("{id}/verify")]
        [Authorize(Roles = "Admin")]
        public IActionResult Verify(int id)
        {
            _providerService.VerifyProvider(id);
            return Ok("Provider verified successfully.");
        }

        [HttpPut("{id}/availability")]
        [Authorize(Roles = "Provider,Admin")]
        public IActionResult SetAvailability(int id, [FromQuery] bool isAvailable)
        {
            _providerService.SetAvailability(id, isAvailable);
            return Ok($"Availability set to {isAvailable}");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            _providerService.DeleteProvider(id);
            return NoContent();
        }

        /// <summary>
        /// Internal-only endpoint for review-service to update provider's average rating.
        /// </summary>
        [HttpPut("{id}/rating")]
        [AllowAnonymous]
        public IActionResult UpdateRating(int id, [FromBody] RatingUpdateDto dto)
        {
            const string expectedKey = "medibook-internal-service-key-2024";
            if (!Request.Headers.TryGetValue("X-Internal-Service-Key", out var key) || key != expectedKey)
                return StatusCode(403, new { message = "Invalid or missing internal service key." });

            try
            {
                _providerService.UpdateRating(id, dto.AvgRating);
                return Ok(new { message = $"Provider {id} rating updated to {dto.AvgRating:F1}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Internal-only endpoint for auth-service to sync user suspension status.
        /// </summary>
        [HttpPut("{id}/status")]
        [AllowAnonymous]
        public IActionResult UpdateStatus(int id, [FromBody] StatusUpdateDto dto)
        {
            const string expectedKey = "medibook-internal-service-key-2024";
            if (!Request.Headers.TryGetValue("X-Internal-Service-Key", out var key) || key != expectedKey)
                return StatusCode(403, new { message = "Invalid or missing internal service key." });

            try
            {
                var provider = _providerService.GetProviderById(id);
                if (provider == null) return NotFound("Provider not found");
                
                // Directly update the DB or use a service method. We'll use service.
                _providerService.SetProviderActiveStatus(id, dto.IsActive);
                return Ok(new { message = $"Provider {id} IsActive set to {dto.IsActive}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class RatingUpdateDto
    {
        public double AvgRating { get; set; }
    }

    public class StatusUpdateDto
    {
        public bool IsActive { get; set; }
    }
}
