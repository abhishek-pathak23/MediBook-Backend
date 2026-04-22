using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using medical_record_service.DTOs;
using medical_record_service.Entities;
using medical_record_service.Interfaces;
using System.Security.Claims;

namespace medical_record_service.Controllers
{
    [ApiController]
    [Route("api/v1/records")]
    [Authorize]
    public class RecordController : ControllerBase
    {
        private readonly IRecordService _recordService;

        public RecordController(IRecordService recordService)
        {
            _recordService = recordService;
        }

        /// <summary>
        /// Create a new medical record after a completed appointment.
        /// Only Providers and Admins can create records.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Provider,Admin")]
        public IActionResult CreateRecord([FromBody] RecordCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var record = new MedicalRecord
                {
                    AppointmentId = dto.AppointmentId,
                    PatientId = dto.PatientId,
                    ProviderId = dto.ProviderId,
                    Diagnosis = dto.Diagnosis,
                    Prescription = dto.Prescription,
                    Notes = dto.Notes,
                    AttachmentUrl = dto.AttachmentUrl,
                    FollowUpDate = dto.FollowUpDate
                };

                var created = _recordService.CreateRecord(record);
                return StatusCode(201, new { message = "Medical record created successfully.", recordId = created.RecordId });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get medical record by appointment ID.
        /// </summary>
        [HttpGet("appointment/{appointmentId}")]
        public IActionResult GetByAppointment(int appointmentId)
        {
            var record = _recordService.GetRecordByAppointment(appointmentId);
            if (record == null)
                return NotFound(new { message = $"No medical record found for Appointment {appointmentId}." });

            return Ok(record);
        }

        /// <summary>
        /// Get all medical records for a patient (ordered by most recent first).
        /// Patients can only view their own records.
        /// </summary>
        [HttpGet("patient/{patientId}")]
        public IActionResult GetByPatient(int patientId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var role = User.FindFirstValue(ClaimTypes.Role);

            // Patients can only see their own records
            if (role == "Patient" && userId != patientId)
                return StatusCode(403, new { message = "You can only view your own medical records." });

            var records = _recordService.GetRecordsByPatient(patientId);
            return Ok(records);
        }

        /// <summary>
        /// Get all medical records created by a specific provider.
        /// Only the provider themselves or an Admin can access this.
        /// </summary>
        [HttpGet("provider/{providerId}")]
        [Authorize(Roles = "Provider,Admin")]
        public IActionResult GetByProvider(int providerId)
        {
            var records = _recordService.GetRecordsByProvider(providerId);
            return Ok(records);
        }

        /// <summary>
        /// Get a single medical record by ID.
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var record = _recordService.GetRecordById(id);
            if (record == null)
                return NotFound(new { message = $"Medical record with ID {id} not found." });

            // Ownership check: Patients can only see their own records
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
            {
                var role = User.FindFirstValue(ClaimTypes.Role);
                if (role == "Patient" && record.PatientId != userId)
                    return StatusCode(403, new { message = "You can only view your own medical records." });
            }

            return Ok(record);
        }

        /// <summary>
        /// Update a medical record. Only the creating Provider or an Admin can update.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Provider,Admin")]
        public IActionResult UpdateRecord(int id, [FromBody] RecordUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = _recordService.UpdateRecord(id, dto);
            if (updated == null)
                return NotFound(new { message = $"Medical record with ID {id} not found." });

            return Ok(updated);
        }

        /// <summary>
        /// Delete a medical record. Admin only.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteRecord(int id)
        {
            var deleted = _recordService.DeleteRecord(id);
            if (!deleted)
                return NotFound(new { message = $"Medical record with ID {id} not found." });

            return NoContent();
        }

        /// <summary>
        /// Get all records with a follow-up date matching the given date.
        /// </summary>
        [HttpGet("followUps")]
        [Authorize(Roles = "Provider,Admin")]
        public IActionResult GetFollowUps([FromQuery] DateOnly date)
        {
            var records = _recordService.GetFollowUpRecords(date);
            return Ok(records);
        }

        /// <summary>
        /// Get total record count for a patient.
        /// </summary>
        [HttpGet("patient/{patientId}/count")]
        public IActionResult GetRecordCount(int patientId)
        {
            var count = _recordService.GetRecordCount(patientId);
            return Ok(new { patientId, recordCount = count });
        }

        /// <summary>
        /// Attach a document URL (e.g., AWS S3 link) to an existing record.
        /// </summary>
        [HttpPut("{id}/attachDocument")]
        [Authorize(Roles = "Provider,Admin")]
        public IActionResult AttachDocument(int id, [FromBody] AttachDocumentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = _recordService.AttachDocument(id, dto.AttachmentUrl);
            if (!success)
                return NotFound(new { message = $"Medical record with ID {id} not found." });

            return Ok(new { message = "Document attached successfully." });
        }
    }
}
