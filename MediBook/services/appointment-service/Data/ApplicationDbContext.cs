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

            // UNIQUE GUARD: Only one active booking allowed per slot.
            // This prevents "Ghost Appointments" even if two users click 'Book' at the same time.
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.SlotId)
                .IsUnique()
                .HasFilter("\"Status\" != 'Cancelled'");
        }
    }
}
