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
        public IActionResult Book([FromBody] AppointmentCreateDto dto)
        {
            var appointment = new Appointment
            {
                PatientId        = dto.PatientId,
                ProviderId       = dto.ProviderId,
                SlotId           = dto.SlotId,
                ServiceType      = dto.ServiceType,
                AppointmentDate  = dto.AppointmentDate,
                StartTime        = dto.StartTime,
                EndTime          = dto.EndTime,
                Notes            = dto.Notes,
                ModeOfConsultation = dto.ModeOfConsultation
            };

            var result = _apptService.BookAppointment(appointment);
            return CreatedAtAction(nameof(GetById), new { id = result.AppointmentId }, result);
        }

        // GET /api/v1/appointments/{id}
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetById(int id)
        {
            var appt = _apptService.GetById(id);
            if (appt == null) return NotFound("Appointment not found.");
            return Ok(appt);
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
        public IActionResult Cancel(int id)
        {
            _apptService.CancelAppointment(id);
            return NoContent();
        }

        // PUT /api/v1/appointments/{id}/reschedule
        [HttpPut("{id}/reschedule")]
        [Authorize]
        public IActionResult Reschedule(int id, [FromBody] AppointmentRescheduleDto dto)
        {
            var updated = _apptService.RescheduleAppointment(id, dto.NewSlotId);
            return Ok(updated);
        }

        // PUT /api/v1/appointments/{id}/complete
        [HttpPut("{id}/complete")]
        [Authorize]
        public IActionResult Complete(int id)
        {
            _apptService.CompleteAppointment(id);
            return NoContent();
        }

        // PUT /api/v1/appointments/{id}/status
        [HttpPut("{id}/status")]
        [Authorize]
        public IActionResult UpdateStatus(int id, [FromBody] AppointmentStatusUpdateDto dto)
        {
            var newStatus = _apptService.UpdateStatus(id, dto.Status);
            return Ok(new { id, status = newStatus });
        }
    }
}
