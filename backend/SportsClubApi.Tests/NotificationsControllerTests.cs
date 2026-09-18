using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using SportsClubApi.Data;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

public class NotificationsControllerTests
{
    // TC-20: Recording attendance for a player whose email matches a User
    // account creates a notification that user can retrieve as their own.
    [Fact]
    public async Task RecordAttendance_ForPlayerWithMatchingUser_CreatesNotification()
    {
        using var factory = new SportsClubApiFactory();
        var playerUser = await TestHelpers.SeedUserAsync(factory, UserRole.Player, "notify-me@example.com");

        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var playerResponse = await adminClient.PostAsJsonAsync("/api/players", new
        {
            fullName = "Notify Me",
            dateOfBirth = "2012-05-04",
            email = playerUser.Email,
            phone = "555-0101",
            registrationDate = "2026-01-15",
            isActive = true,
        });
        var player = (await playerResponse.Content.ReadFromJsonAsync<Player>())!;

        await adminClient.PostAsJsonAsync("/api/attendance", new
        {
            playerId = player.Id,
            sessionDate = "2026-02-01",
            isPresent = true,
            notes = "",
        });

        var loginResponse = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new
        {
            email = playerUser.Email,
            password = TestHelpers.DefaultPassword,
        });
        var auth = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();

        var playerClient = factory.CreateClient();
        playerClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!["token"]);

        var notifications = await playerClient.GetFromJsonAsync<List<Notification>>("/api/notifications");

        Assert.NotNull(notifications);
        Assert.Contains(notifications!, n => n.Message.Contains("present"));
    }

    // TC-21: A user cannot mark another user's notification as read.
    [Fact]
    public async Task MarkAsRead_ForAnotherUsersNotification_ReturnsNotFound()
    {
        using var factory = new SportsClubApiFactory();
        var owner = await TestHelpers.SeedUserAsync(factory, UserRole.Player, "owner@example.com");

        int notificationId;
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var notification = new Notification { UserId = owner.Id, Message = "Test notification" };
            context.Notifications.Add(notification);
            await context.SaveChangesAsync();
            notificationId = notification.Id;
        }

        var otherClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Player);
        var response = await otherClient.PostAsync($"/api/notifications/{notificationId}/read", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
