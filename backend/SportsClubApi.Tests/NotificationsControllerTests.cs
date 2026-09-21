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

    // TC-42: A Coach can send a notice to all players; each player account gets
    // its own copy and volunteers get none.
    [Fact]
    public async Task Send_AsCoach_ToPlayers_ReachesPlayerAccountsOnly()
    {
        using var factory = new SportsClubApiFactory();
        var playerOne = await TestHelpers.SeedUserAsync(factory, UserRole.Player, "p1@example.com");
        await TestHelpers.SeedUserAsync(factory, UserRole.Player, "p2@example.com");
        var volunteer = await TestHelpers.SeedUserAsync(factory, UserRole.Volunteer, "v1@example.com");
        var coachClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Coach);

        var response = await coachClient.PostAsJsonAsync("/api/notifications/send", new
        {
            message = "Training moved to 6pm",
            audience = "Players",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        Assert.Equal(2, result!["sent"]);

        var playerClient = await TestHelpers.LoginAsync(factory, playerOne);
        var mine = await playerClient.GetFromJsonAsync<List<Notification>>("/api/notifications");
        Assert.Single(mine!);
        Assert.Contains("Training moved to 6pm", mine![0].Message);

        var volunteerClient = await TestHelpers.LoginAsync(factory, volunteer);
        var theirs = await volunteerClient.GetFromJsonAsync<List<Notification>>("/api/notifications");
        Assert.Empty(theirs!);
    }

    // TC-43: A Player cannot send notifications, returning 403.
    [Fact]
    public async Task Send_AsPlayer_ReturnsForbidden()
    {
        using var factory = new SportsClubApiFactory();
        var playerClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Player);

        var response = await playerClient.PostAsJsonAsync("/api/notifications/send", new
        {
            message = "Hello everyone",
            audience = "Everyone",
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // TC-44: A blank message is rejected with 400.
    [Fact]
    public async Task Send_WithBlankMessage_ReturnsBadRequest()
    {
        using var factory = new SportsClubApiFactory();
        var coachClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Coach);

        var response = await coachClient.PostAsJsonAsync("/api/notifications/send", new
        {
            message = "   ",
            audience = "Players",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
