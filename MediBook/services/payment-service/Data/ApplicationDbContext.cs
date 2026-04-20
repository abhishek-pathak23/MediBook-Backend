using Microsoft.EntityFrameworkCore;
using payment_service.Entities;

namespace payment_service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add indexes for performance as per common query patterns
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.AppointmentId)
                .IsUnique();

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.PatientId);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.ProviderId);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.TransactionId)
                .IsUnique();
        }
    }
}
