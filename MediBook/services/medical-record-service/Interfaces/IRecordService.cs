using medical_record_service.DTOs;
using medical_record_service.Entities;

namespace medical_record_service.Interfaces
{
    public interface IRecordService
    {
        MedicalRecord CreateRecord(MedicalRecord record);
        RecordResponseDto? GetRecordByAppointment(int appointmentId);
        List<RecordResponseDto> GetRecordsByPatient(int patientId);
        List<RecordResponseDto> GetRecordsByProvider(int providerId);
        RecordResponseDto? UpdateRecord(int recordId, RecordUpdateDto dto);
        bool DeleteRecord(int recordId);
        RecordResponseDto? GetRecordById(int recordId);
        List<RecordResponseDto> GetFollowUpRecords(DateOnly date);
        int GetRecordCount(int patientId);
        bool AttachDocument(int recordId, string attachmentUrl);
    }
}
