using Microsoft.EntityFrameworkCore;
using provider_service.Entities;

namespace provider_service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Provider> Providers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Can add more specific EF config here, like unique constraints if needed
        }
    }
}
