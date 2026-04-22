using Microsoft.EntityFrameworkCore;
using medical_record_service.Entities;

namespace medical_record_service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<MedicalRecord> MedicalRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One medical record per appointment
            modelBuilder.Entity<MedicalRecord>()
                .HasIndex(r => r.AppointmentId)
                .IsUnique();

            // Index for efficient patient lookups
            modelBuilder.Entity<MedicalRecord>()
                .HasIndex(r => r.PatientId);

            // Index for efficient provider lookups
            modelBuilder.Entity<MedicalRecord>()
                .HasIndex(r => r.ProviderId);
        }
    }
}
