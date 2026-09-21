using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportsClubApi.Models;

namespace SportsClubApi.Data;

// Coach and Admin accounts can't be created through /api/auth/register (it
// only allows Player/Volunteer, by design - see AuthController). Without
// this seeder there was previously no way to get a Coach/Admin account on a
// fresh database at all short of editing the DB by hand. Idempotent: it only
// creates a default account if its email doesn't already exist, so it's safe to run
// on every startup.
public static class DbSeeder
{
    public const string DefaultAdminEmail = "admin@mtedenfc.test";
    public const string DefaultCoachEmail = "coach@mtedenfc.test";
    public const string DefaultPassword = "Password123!";

    // Sample notifications so the Notifications page has realistic content on a
    // fresh database. (message, hours ago, already read). Matches and training
    // sessions line up with the schedule in DemoDataSeeder.
    private static readonly (string Message, int HoursAgo, bool IsRead)[] SharedNotifications =
    {
        ("Mt Eden FC: Next match is Saturday 27 September vs Kingsland FC at Lloyd Elsmore Park. Kick-off 2:00 PM, please arrive by 1:15 PM to warm up.", 6, false),
        ("Mt Eden FC: Reminder - training this Wednesday 24 September, 6:00 PM at Nixon Park. Bring shin pads and a water bottle.", 20, false),
        ("Mt Eden FC: Fixture update - Sunday 4 October vs Sandringham has been confirmed for Lloyd Elsmore Park, 1:00 PM kick-off.", 52, true),
        ("Mt Eden FC: Thanks to everyone who helped run the sausage sizzle at the Ponsonby United game - we raised $412 for new match balls.", 77, true),
        ("Mt Eden FC: Club AGM will be held on Thursday 9 October at 7:00 PM in the clubrooms. All players, volunteers and families are welcome.", 100, true),
        ("Mt Eden FC: Training on Wednesday 1 October is moved to Nixon Park's lower pitch while the main pitch is resurfaced.", 130, true),
    };

    private static readonly (string Message, int HoursAgo, bool IsRead)[] CoachNotifications =
    {
        ("Mt Eden FC: Team selection for the Kingsland FC match will be confirmed after Wednesday's training - please have your squad list ready by Thursday evening.", 3, false),
        ("Mt Eden FC: Attendance reminder - the 16 September training session has not been marked yet. Please record it on the Attendance page.", 30, false),
    };

    private static readonly (string Message, int HoursAgo, bool IsRead)[] AdminNotifications =
    {
        ("Mt Eden FC: Registration reminder - some new players have not yet completed their date of birth and phone details. Please follow up before the Kingsland FC match.", 8, false),
        ("Mt Eden FC: Attendance for the Ponsonby United week has been recorded. Review the Attendance History page to check nobody was missed.", 60, true),
    };

    public static async Task SeedAsync(AppDbContext context)
    {
        var hasher = new PasswordHasher<User>();

        if (!await context.Users.AnyAsync(u => u.Email == DefaultAdminEmail))
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

        if (!await context.Users.AnyAsync(u => u.Email == DefaultCoachEmail))
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

        await SeedNotificationsAsync(context, DefaultAdminEmail, SharedNotifications.Concat(AdminNotifications));
        await SeedNotificationsAsync(context, DefaultCoachEmail, SharedNotifications.Concat(CoachNotifications));
    }

    // Adds each sample notification to the account unless that exact message is
    // already there, so restarting the app never creates duplicates.
    private static async Task SeedNotificationsAsync(
        AppDbContext context,
        string email,
        IEnumerable<(string Message, int HoursAgo, bool IsRead)> samples)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return;
        }

        var existing = await context.Notifications
            .Where(n => n.UserId == user.Id)
            .Select(n => n.Message)
            .ToListAsync();

        foreach (var sample in samples.Where(x => !existing.Contains(x.Message)))
        {
            context.Notifications.Add(new Notification
            {
                UserId = user.Id,
                Message = sample.Message,
                CreatedAt = DateTime.UtcNow.AddHours(-sample.HoursAgo),
                IsRead = sample.IsRead,
            });
        }

        await context.SaveChangesAsync();
    }
}
