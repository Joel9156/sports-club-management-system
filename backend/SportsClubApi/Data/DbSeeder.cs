using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportsClubApi.Models;

namespace SportsClubApi.Data;

// Coach and Admin accounts can't be created through /api/auth/register (it
// only allows Player/Volunteer, by design - see AuthController). Without
// this seeder there was previously no way to get a Coach/Admin account on a
// fresh database at all short of editing the DB by hand. Idempotent: it only
// creates an account if that role doesn't already exist, so it's safe to run
// on every startup.
public static class DbSeeder
{
    public const string DefaultAdminEmail = "admin@mtedenfc.test";
    public const string DefaultCoachEmail = "coach@mtedenfc.test";
    public const string DefaultPassword = "Password123!";

    public static async Task SeedAsync(AppDbContext context)
    {
        var hasher = new PasswordHasher<User>();

        if (!await context.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            var admin = new User
            {
                Email = DefaultAdminEmail,
                FullName = "Default Admin",
                Role = UserRole.Admin,
            };
            admin.PasswordHash = hasher.HashPassword(admin, DefaultPassword);
            context.Users.Add(admin);
        }

        if (!await context.Users.AnyAsync(u => u.Role == UserRole.Coach))
        {
            var coach = new User
            {
                Email = DefaultCoachEmail,
                FullName = "Default Coach",
                Role = UserRole.Coach,
            };
            coach.PasswordHash = hasher.HashPassword(coach, DefaultPassword);
            context.Users.Add(coach);
        }

        if (!await context.Teams.AnyAsync())
        {
            context.Teams.Add(new Team
            {
                Name = "Mt Eden FC",
                AgeGroup = "Open",
                CoachName = "Default Coach",
                Season = "2026",
            });
        }

        await context.SaveChangesAsync();
    }
}
