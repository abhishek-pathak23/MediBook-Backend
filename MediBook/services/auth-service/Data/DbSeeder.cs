using auth_service.Data;
using auth_service.Entities;
using Microsoft.EntityFrameworkCore;

namespace auth_service.Data
{
    public static class DbSeeder
    {
        public static void SeedAdminUser(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Ensure the database is created/migrated before seeding
            context.Database.Migrate();

            // Check if our specific Admin account already exists
            var adminEmail = "abhipathak4546@gmail.com";
            if (!context.Users.Any(u => u.Email == adminEmail))
            {
                var adminUser = new User
                {
                    FullName = "Abhishek Pathak (Admin)",
                    Email = adminEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Phone = "1234567890",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                context.SaveChanges();
            }
        }
    }
}
