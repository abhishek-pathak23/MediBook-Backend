using appointment_service.Entities;
using Microsoft.EntityFrameworkCore;

namespace appointment_service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Appointment> Appointments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite index for fast provider-date queries (GetByProviderAndDate)
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.ProviderId, a.AppointmentDate });

            // Index for fast patient-side queries
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.PatientId);

            // Unique constraint: one active booking per slot
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.SlotId)
                .IsUnique(false); // Not unique — cancelled slots can be re-booked
        }
    }
}
