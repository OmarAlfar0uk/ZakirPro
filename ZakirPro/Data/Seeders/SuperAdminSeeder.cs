using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Data;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Data.Seeders;

public static class SuperAdminSeeder
{
    /// <summary>
    /// Ensures one SuperAdmin account exists. Runs at startup — idempotent.
    /// Credentials are read from appsettings: SuperAdmin:FullName/Email/Password.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db      = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config  = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger  = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            await db.Database.MigrateAsync();

            var email = config["SuperAdmin:Email"];
            if (string.IsNullOrWhiteSpace(email))
            {
                logger.LogWarning("SuperAdmin:Email not configured — seeder skipped.");
                return;
            }

            var exists = await db.SuperAdmins
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Email == email);

            if (exists)
            {
                logger.LogInformation("SuperAdmin already exists — seed skipped.");
                return;
            }

            var hasher = new PasswordHasher<User>();
            var superAdmin = new SuperAdminUser
            {
                FullName = config["SuperAdmin:FullName"] ?? "Super Admin",
                Email    = email,
                Role     = UserRole.SuperAdmin,
                IsActive = true
            };
            superAdmin.PasswordHash = hasher.HashPassword(superAdmin, config["SuperAdmin:Password"]!);

            db.SuperAdmins.Add(superAdmin);
            await db.SaveChangesAsync();

            logger.LogInformation("SuperAdmin seeded: {Email}", email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SuperAdmin seeder failed.");
        }
    }
}
