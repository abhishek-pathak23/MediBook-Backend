using Microsoft.EntityFrameworkCore;
using review_service.Entities;

namespace review_service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Per requirement: One review is permitted per appointment
            modelBuilder.Entity<Review>()
                .HasIndex(r => r.AppointmentId)
                .IsUnique();

            // Ensure rating is between 1 and 5 (Database level check)
            modelBuilder.Entity<Review>()
                .HasCheckConstraint("CK_Review_Rating", "[Rating] >= 1 AND [Rating] <= 5");
        }
    }
}
