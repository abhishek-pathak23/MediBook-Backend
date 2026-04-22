using medical_record_service.Data;
using medical_record_service.Entities;
using medical_record_service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace medical_record_service.Repositories
{
    public class RecordRepository : IRecordRepository
    {
        private readonly ApplicationDbContext _context;

        public RecordRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public MedicalRecord? FindByAppointmentId(int appointmentId)
        {
            return _context.MedicalRecords.FirstOrDefault(r => r.AppointmentId == appointmentId);
        }

        public List<MedicalRecord> FindByPatientId(int patientId)
        {
            return _context.MedicalRecords.Where(r => r.PatientId == patientId).ToList();
        }

        public List<MedicalRecord> FindByProviderId(int providerId)
        {
            return _context.MedicalRecords.Where(r => r.ProviderId == providerId).ToList();
        }

        public List<MedicalRecord> FindByPatientIdOrderByCreatedAtDesc(int patientId)
        {
            return _context.MedicalRecords
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public List<MedicalRecord> FindByFollowUpDate(DateOnly date)
        {
            return _context.MedicalRecords
                .Where(r => r.FollowUpDate == date)
                .ToList();
        }

        public int CountByPatientId(int patientId)
        {
            return _context.MedicalRecords.Count(r => r.PatientId == patientId);
        }

        public MedicalRecord? GetById(int recordId)
        {
            return _context.MedicalRecords.FirstOrDefault(r => r.RecordId == recordId);
        }

        public List<MedicalRecord> GetAll()
        {
            return _context.MedicalRecords.OrderByDescending(r => r.CreatedAt).ToList();
        }

        public void Add(MedicalRecord record)
        {
            _context.MedicalRecords.Add(record);
        }

        public void Update(MedicalRecord record)
        {
            _context.MedicalRecords.Update(record);
        }

        public void DeleteByRecordId(int recordId)
        {
            var record = GetById(recordId);
            if (record != null)
            {
                _context.MedicalRecords.Remove(record);
            }
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
