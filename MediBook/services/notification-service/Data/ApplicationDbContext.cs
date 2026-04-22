using Microsoft.EntityFrameworkCore;
using notification_service.Entities;

namespace notification_service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // To make querying fast since we will often query by RecipientId and IsRead
            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.RecipientId, n.IsRead });
        }
    }
}
