using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using schedule_service.DTOs;
using schedule_service.Entities;
using schedule_service.Interfaces;

namespace schedule_service.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _schedService;

        public ScheduleController(IScheduleService schedService)
        {
            _schedService = schedService;
        }

        [HttpPost]
        [Authorize(Roles = "Provider,Admin")]
        public IActionResult AddSlot([FromBody] SlotCreateDto dto)
        {
            var slot = new AvailabilitySlot
            {
                ProviderId = dto.ProviderId,
                Date = dto.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                DurationMinutes = dto.DurationMinutes
            };
            
            var result = _schedService.AddSlot(slot);
            return CreatedAtAction(nameof(GetById), new { id = result.SlotId }, result);
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Provider,Admin")]
        public IActionResult AddBulk([FromBody] List<SlotCreateDto> dtos)
        {
            var slots = dtos.Select(dto => new AvailabilitySlot
            {
                ProviderId = dto.ProviderId,
                Date = dto.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                DurationMinutes = dto.DurationMinutes
            }).ToList();

            var result = _schedService.AddBulkSlots(slots);
            return Ok(result);
        }

        [HttpPost("recurring")]
        [Authorize(Roles = "Provider,Admin")]
        public IActionResult GenerateRecurring([FromBody] RecurringSlotCreateDto dto)
        {
            var generatedSlots = _schedService.GenerateRecurringSlots(
                dto.ProviderId, 
                dto.Recurrence, 
                dto.StartDate, 
                dto.EndDate, 
                dto.StartTime, 
                dto.EndTime, 
                dto.DurationMinutes
            );
            return Ok(generatedSlots);
        }

        [HttpGet("provider/{providerId}")]
        [Authorize]
        public IActionResult GetByProvider(int providerId)
        {
            var slots = _schedService.GetSlotsByProvider(providerId);
            return Ok(slots);
        }

        [HttpGet("available/{providerId}")]
        // Open to public/patients to see when a provider is free
        public IActionResult GetAvailable(int providerId, [FromQuery] DateOnly date)
        {
            var slots = _schedService.GetAvailableSlots(providerId, date);
            return Ok(slots);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var slot = _schedService.GetSlotById(id);
            if (slot == null) return NotFound("Slot not found");
            return Ok(slot);
        }

        [HttpPut("{id}/block")]
        [Authorize(Roles = "Provider,Admin")]
        public IActionResult BlockSlot(int id)
        {
            _schedService.BlockSlot(id);
            return NoContent();
        }

        [HttpPut("{id}/unblock")]
        [Authorize(Roles = "Provider,Admin")]
        public IActionResult UnblockSlot(int id)
        {
            _schedService.UnblockSlot(id);
            return NoContent();
        }

        // Called by Appointment-Service via IHttpClientFactory to mark a slot as booked
        [HttpPut("{id}/book")]
        [Authorize]
        public IActionResult BookSlot(int id)
        {
            _schedService.BookSlot(id);
            return NoContent();
        }

        // Called by Appointment-Service on cancellation/reschedule to free the slot
        [HttpPut("{id}/release")]
        [Authorize]
        public IActionResult ReleaseSlot(int id)
        {
            _schedService.ReleaseSlot(id);
            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Provider,Admin")]
        public IActionResult UpdateSlot(int id, [FromBody] SlotUpdateDto dto)
        {
            var slotState = new AvailabilitySlot
            {
                Date = dto.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                DurationMinutes = dto.DurationMinutes,
                Recurrence = dto.Recurrence
            };

            var updated = _schedService.UpdateSlot(id, slotState);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Provider,Admin")]
        public IActionResult DeleteSlot(int id)
        {
            _schedService.DeleteSlot(id);
            return NoContent();
        }
    }
}
