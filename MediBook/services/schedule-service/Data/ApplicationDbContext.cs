using Microsoft.EntityFrameworkCore;
using schedule_service.Entities;

namespace schedule_service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AvailabilitySlot> AvailabilitySlots { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Ensures fast querying for available slots by date.
            modelBuilder.Entity<AvailabilitySlot>()
                .HasIndex(s => new { s.ProviderId, s.Date });
        }
    }
}
