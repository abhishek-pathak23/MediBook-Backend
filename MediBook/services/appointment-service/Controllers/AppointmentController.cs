using appointment_service.DTOs;
using appointment_service.Entities;
using appointment_service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace appointment_service.Controllers
{
    [ApiController]
    [Route("api/v1/appointments")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _apptService;

        public AppointmentController(IAppointmentService apptService)
        {
            _apptService = apptService;
        }

        // POST /api/v1/appointments — Book a new appointment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Book([FromBody] AppointmentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var appointment = new Appointment
                {
                    PatientId = dto.PatientId,
                    ProviderId = dto.ProviderId,
                    SlotId = dto.SlotId,
                    ServiceType = dto.ServiceType,
                    AppointmentDate = dto.AppointmentDate,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    Notes = dto.Notes,
                    ModeOfConsultation = dto.ModeOfConsultation
                };

                var result = await _apptService.BookAppointmentAsync(appointment);
                return CreatedAtAction(nameof(GetById), new { id = result.AppointmentId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var appointment = _apptService.GetById(id);
            if (appointment == null)
                return NotFound();

            return Ok(appointment);
        }

        // GET /api/v1/appointments/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        [Authorize]
        public IActionResult GetByPatient(int patientId)
        {
            var appointments = _apptService.GetByPatient(patientId);
            return Ok(appointments);
        }

        // GET /api/v1/appointments/patient/{patientId}/upcoming
        [HttpGet("patient/{patientId}/upcoming")]
        [Authorize]
        public IActionResult GetUpcoming(int patientId)
        {
            var appointments = _apptService.GetUpcomingByPatient(patientId);
            return Ok(appointments);
        }

        // GET /api/v1/appointments/provider/{providerId}
        [HttpGet("provider/{providerId}")]
        [Authorize]
        public IActionResult GetByProvider(int providerId)
        {
            var appointments = _apptService.GetByProvider(providerId);
            return Ok(appointments);
        }

        // GET /api/v1/appointments/provider/{providerId}/date?date=2026-04-20
        [HttpGet("provider/{providerId}/date")]
        [Authorize]
        public IActionResult GetByProviderAndDate(int providerId, [FromQuery] DateOnly date)
        {
            var appointments = _apptService.GetByProviderAndDate(providerId, date);
            return Ok(appointments);
        }

        // GET /api/v1/appointments/provider/{providerId}/count
        [HttpGet("provider/{providerId}/count")]
        [Authorize]
        public IActionResult GetCount(int providerId)
        {
            var count = _apptService.GetAppointmentCount(providerId);
            return Ok(new { providerId, count });
        }

        // PUT /api/v1/appointments/{id}/cancel
        [HttpPut("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _apptService.CancelAppointmentAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/v1/appointments/{id}/reschedule
        [HttpPut("{id}/reschedule")]
        [Authorize]
        public async Task<IActionResult> Reschedule(int id, [FromBody] AppointmentRescheduleDto dto)
        {
            try
            {
                var updated = await _apptService.RescheduleAppointmentAsync(id, dto.NewSlotId);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/v1/appointments/{id}/complete
        [HttpPut("{id}/complete")]
        [Authorize]
        public IActionResult Complete(int id)
        {
            try
            {
                _apptService.CompleteAppointment(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/v1/appointments/{id}/status
        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] AppointmentStatusUpdateDto dto)
        {
            try
            {
                var newStatus = await _apptService.UpdateStatusAsync(id, dto.Status);
                return Ok(new { id, status = newStatus });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
