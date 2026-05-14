using medical_record_service.DTOs;
using medical_record_service.Entities;
using medical_record_service.Interfaces;

namespace medical_record_service.Services
{
    public class RecordService : IRecordService
    {
        private readonly IRecordRepository _repo;
        private readonly EncryptionService _encryption;
        private readonly ILogger<RecordService> _logger;

        public RecordService(IRecordRepository repo, EncryptionService encryption, ILogger<RecordService> logger)
        {
            _repo = repo;
            _encryption = encryption;
            _logger = logger;
        }

        public MedicalRecord CreateRecord(MedicalRecord record)
        {
            // Check if a record already exists for this appointment
            var existing = _repo.FindByAppointmentId(record.AppointmentId);
            if (existing != null)
                throw new InvalidOperationException($"A medical record already exists for Appointment {record.AppointmentId}.");

            // Encrypt sensitive fields before saving
            record.Diagnosis = _encryption.Encrypt(record.Diagnosis);
            record.Prescription = _encryption.Encrypt(record.Prescription);
            record.Notes = record.Notes != null ? _encryption.Encrypt(record.Notes) : null;
            record.CreatedAt = DateTime.UtcNow;
            record.UpdatedAt = DateTime.UtcNow;

            _repo.Add(record);
            _repo.SaveChanges();

            _logger.LogInformation($"Medical record created for Appointment {record.AppointmentId} by Provider {record.ProviderId}.");
            return record;
        }

        public RecordResponseDto? GetRecordByAppointment(int appointmentId)
        {
            var record = _repo.FindByAppointmentId(appointmentId);
            return record == null ? null : MapToDto(record);
        }

        public List<RecordResponseDto> GetRecordsByPatient(int patientId)
        {
            return _repo.FindByPatientIdOrderByCreatedAtDesc(patientId)
                .Select(MapToDto)
                .ToList();
        }

        public List<RecordResponseDto> GetRecordsByProvider(int providerId)
        {
            return _repo.FindByProviderId(providerId)
                .Select(MapToDto)
                .ToList();
        }

        public RecordResponseDto? UpdateRecord(int recordId, RecordUpdateDto dto)
        {
            var record = _repo.GetById(recordId);
            if (record == null) return null;

            // Encrypt updated fields
            record.Diagnosis = _encryption.Encrypt(dto.Diagnosis);
            record.Prescription = _encryption.Encrypt(dto.Prescription);
            record.Notes = dto.Notes != null ? _encryption.Encrypt(dto.Notes) : null;
            record.AttachmentUrl = dto.AttachmentUrl;
            record.FollowUpDate = dto.FollowUpDate;
            record.UpdatedAt = DateTime.UtcNow;

            _repo.Update(record);
            _repo.SaveChanges();

            _logger.LogInformation($"Medical record {recordId} updated.");
            return MapToDto(record);
        }

        public bool DeleteRecord(int recordId)
        {
            var record = _repo.GetById(recordId);
            if (record == null) return false;

            _repo.DeleteByRecordId(recordId);
            _repo.SaveChanges();

            _logger.LogInformation($"Medical record {recordId} deleted.");
            return true;
        }

        public RecordResponseDto? GetRecordById(int recordId)
        {
            var record = _repo.GetById(recordId);
            return record == null ? null : MapToDto(record);
        }

        public List<RecordResponseDto> GetFollowUpRecords(DateOnly date)
        {
            return _repo.FindByFollowUpDate(date)
                .Select(MapToDto)
                .ToList();
        }

        public int GetRecordCount(int patientId)
        {
            return _repo.CountByPatientId(patientId);
        }

        public bool AttachDocument(int recordId, string attachmentUrl)
        {
            var record = _repo.GetById(recordId);
            if (record == null) return false;

            record.AttachmentUrl = attachmentUrl;
            record.UpdatedAt = DateTime.UtcNow;

            _repo.Update(record);
            _repo.SaveChanges();

            _logger.LogInformation($"Document attached to record {recordId}: {attachmentUrl}");
            return true;
        }

        /// <summary>
        /// Maps entity to response DTO with decrypted sensitive fields.
        /// </summary>
        private RecordResponseDto MapToDto(MedicalRecord record)
        {
            return new RecordResponseDto
            {
                RecordId = record.RecordId,
                AppointmentId = record.AppointmentId,
                PatientId = record.PatientId,
                ProviderId = record.ProviderId,
                Diagnosis = _encryption.Decrypt(record.Diagnosis),
                Prescription = _encryption.Decrypt(record.Prescription),
                Notes = record.Notes != null ? _encryption.Decrypt(record.Notes) : null,
                AttachmentUrl = record.AttachmentUrl,
                FollowUpDate = record.FollowUpDate,
                CreatedAt = record.CreatedAt,
                UpdatedAt = record.UpdatedAt
            };
        }
    }
}
