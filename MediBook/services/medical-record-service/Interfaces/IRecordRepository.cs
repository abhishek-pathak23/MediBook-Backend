using medical_record_service.Entities;

namespace medical_record_service.Interfaces
{
    public interface IRecordRepository
    {
        MedicalRecord? FindByAppointmentId(int appointmentId);
        List<MedicalRecord> FindByPatientId(int patientId);
        List<MedicalRecord> FindByProviderId(int providerId);
        List<MedicalRecord> FindByPatientIdOrderByCreatedAtDesc(int patientId);
        List<MedicalRecord> FindByFollowUpDate(DateOnly date);
        int CountByPatientId(int patientId);
        MedicalRecord? GetById(int recordId);
        List<MedicalRecord> GetAll();
        void Add(MedicalRecord record);
        void Update(MedicalRecord record);
        void DeleteByRecordId(int recordId);
        void SaveChanges();
    }
}
