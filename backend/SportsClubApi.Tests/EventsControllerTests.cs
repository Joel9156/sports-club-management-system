using System.Net;
using System.Net.Http.Json;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

public class EventsControllerTests
{
    // TC-22: An Admin can add a match to the schedule, returning 201.
    [Fact]
    public async Task CreateEvent_AsAdmin_ReturnsCreated()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var response = await adminClient.PostAsJsonAsync("/api/events", new
        {
            type = "Match",
            date = "2026-10-05",
            location = "Lloyd Elsmore Park",
            opponent = "Eden Rovers",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<ScheduledEvent>(ScheduleTestHelpers.Json);
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal(EventType.Match, created.Type);
        Assert.Equal("Eden Rovers", created.Opponent);
    }

    // TC-34: A Coach can add training to the schedule, returning 201.
    [Fact]
    public async Task CreateEvent_AsCoach_ReturnsCreated()
    {
        using var factory = new SportsClubApiFactory();
        var coachClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Coach);

        var response = await coachClient.PostAsJsonAsync("/api/events", new
        {
            type = "Training",
            date = "2026-10-02",
            location = "Nixon Park",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // TC-35: Deleting an event is Admin-only - a Coach gets 403, an Admin 204.
    [Fact]
    public async Task DeleteEvent_CoachForbidden_AdminAllowed()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var coachClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Coach);
        var match = await ScheduleTestHelpers.CreateMatchAsync(adminClient);

        var asCoach = await coachClient.DeleteAsync($"/api/events/{match.Id}");
        var asAdmin = await adminClient.DeleteAsync($"/api/events/{match.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, asCoach.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, asAdmin.StatusCode);
    }

    // TC-23: Only Coaches and Admins manage the schedule - a Player is forbidden.
    [Fact]
    public async Task CreateEvent_AsPlayer_ReturnsForbidden()
    {
        using var factory = new SportsClubApiFactory();
        var playerClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Player);

        var response = await playerClient.PostAsJsonAsync("/api/events", new
        {
            type = "Training",
            date = "2026-10-02",
            location = "Nixon Park",
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // TC-24: A match without an opponent is rejected with 400.
    [Fact]
    public async Task CreateEvent_MatchWithoutOpponent_ReturnsBadRequest()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var response = await adminClient.PostAsJsonAsync("/api/events", new
        {
            type = "Match",
            date = "2026-10-05",
            location = "Lloyd Elsmore Park",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // TC-25: A training session can't carry a match score, returning 400.
    [Fact]
    public async Task CreateEvent_TrainingWithScore_ReturnsBadRequest()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var response = await adminClient.PostAsJsonAsync("/api/events", new
        {
            type = "Training",
            date = "2026-10-02",
            location = "Nixon Park",
            goalsFor = 2,
            goalsAgainst = 1,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // TC-26: Every role can read the schedule - a Player sees what an Admin added.
    [Fact]
    public async Task GetEvents_AsPlayer_ReturnsScheduledEvents()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        await ScheduleTestHelpers.CreateMatchAsync(adminClient);
        await ScheduleTestHelpers.CreateTrainingAsync(adminClient);

        var playerClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Player);
        var events = await playerClient.GetFromJsonAsync<List<ScheduledEvent>>("/api/events", ScheduleTestHelpers.Json);

        Assert.NotNull(events);
        Assert.Equal(2, events!.Count);
        Assert.Equal(EventType.Training, events[0].Type); // sorted soonest first (2 Oct before 5 Oct)
    }
}
