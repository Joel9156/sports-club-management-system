using System.Net;
using System.Net.Http.Json;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

public class PlayerStatsControllerTests
{
    // TC-27: An Admin can record a player's goals/assists for a match, returning 201.
    [Fact]
    public async Task RecordStat_AsAdmin_ReturnsCreated()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var match = await ScheduleTestHelpers.CreateMatchAsync(adminClient);
        var player = await ScheduleTestHelpers.CreatePlayerAsync(adminClient, "scorer@example.com");

        var response = await ScheduleTestHelpers.RecordStatAsync(adminClient, player.Id, match.Id, goals: 2, assists: 1);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var stat = await response.Content.ReadFromJsonAsync<PlayerStat>(ScheduleTestHelpers.Json);
        Assert.NotNull(stat);
        Assert.Equal(2, stat!.Goals);
        Assert.Equal(1, stat.Assists);
    }

    // TC-28: Recording the same player twice for one match is rejected with 409.
    [Fact]
    public async Task RecordStat_DuplicatePlayerAndMatch_ReturnsConflict()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var match = await ScheduleTestHelpers.CreateMatchAsync(adminClient);
        var player = await ScheduleTestHelpers.CreatePlayerAsync(adminClient, "twice@example.com");

        var first = await ScheduleTestHelpers.RecordStatAsync(adminClient, player.Id, match.Id, goals: 1);
        first.EnsureSuccessStatusCode();

        var second = await ScheduleTestHelpers.RecordStatAsync(adminClient, player.Id, match.Id, goals: 3);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    // TC-29: Stats can't be recorded against a training session, returning 400.
    [Fact]
    public async Task RecordStat_AgainstTraining_ReturnsBadRequest()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var training = await ScheduleTestHelpers.CreateTrainingAsync(adminClient);
        var player = await ScheduleTestHelpers.CreatePlayerAsync(adminClient, "trainee@example.com");

        var response = await ScheduleTestHelpers.RecordStatAsync(adminClient, player.Id, training.Id, goals: 1);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // TC-30: Only Coaches and Admins enter stats - a Player is forbidden.
    [Fact]
    public async Task RecordStat_AsPlayer_ReturnsForbidden()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var match = await ScheduleTestHelpers.CreateMatchAsync(adminClient);
        var player = await ScheduleTestHelpers.CreatePlayerAsync(adminClient, "cheater@example.com");

        var playerClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Player);
        var response = await ScheduleTestHelpers.RecordStatAsync(playerClient, player.Id, match.Id, goals: 9);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // TC-36: A Coach can enter a player's goals/assists for a match, returning 201.
    [Fact]
    public async Task RecordStat_AsCoach_ReturnsCreated()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var match = await ScheduleTestHelpers.CreateMatchAsync(adminClient);
        var player = await ScheduleTestHelpers.CreatePlayerAsync(adminClient, "coach-entered@example.com");

        var coachClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Coach);
        var response = await ScheduleTestHelpers.RecordStatAsync(coachClient, player.Id, match.Id, goals: 1, assists: 2);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    // TC-31: Negative goals are rejected with 400.
    [Fact]
    public async Task RecordStat_WithNegativeGoals_ReturnsBadRequest()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var match = await ScheduleTestHelpers.CreateMatchAsync(adminClient);
        var player = await ScheduleTestHelpers.CreatePlayerAsync(adminClient, "negative@example.com");

        var response = await ScheduleTestHelpers.RecordStatAsync(adminClient, player.Id, match.Id, goals: -1);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Negative assists are rejected with 400, same as negative goals.
    [Fact]
    public async Task RecordStat_WithNegativeAssists_ReturnsBadRequest()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var match = await ScheduleTestHelpers.CreateMatchAsync(adminClient);
        var player = await ScheduleTestHelpers.CreatePlayerAsync(adminClient, "negative-assists@example.com");

        var response = await ScheduleTestHelpers.RecordStatAsync(adminClient, player.Id, match.Id, assists: -1);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Recording a stat for a player id that doesn't exist returns 400, not a
    // foreign-key failure.
    [Fact]
    public async Task RecordStat_ForNonExistentPlayer_ReturnsBadRequest()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var match = await ScheduleTestHelpers.CreateMatchAsync(adminClient);

        var response = await ScheduleTestHelpers.RecordStatAsync(adminClient, 999999, match.Id, goals: 1);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Recording a stat against a match id that doesn't exist returns 400.
    [Fact]
    public async Task RecordStat_ForNonExistentMatch_ReturnsBadRequest()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var player = await ScheduleTestHelpers.CreatePlayerAsync(adminClient, "no-match@example.com");

        var response = await ScheduleTestHelpers.RecordStatAsync(adminClient, player.Id, 999999, goals: 1);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // A valid PUT corrects the goals/assists on an existing row.
    [Fact]
    public async Task UpdatePlayerStat_WithValidData_ReturnsNoContentAndPersists()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var match = await ScheduleTestHelpers.CreateMatchAsync(adminClient);
        var player = await ScheduleTestHelpers.CreatePlayerAsync(adminClient, "correction@example.com");
        var created = await (await ScheduleTestHelpers.RecordStatAsync(adminClient, player.Id, match.Id, goals: 1, assists: 0))
            .Content.ReadFromJsonAsync<PlayerStat>(ScheduleTestHelpers.Json);

        var response = await adminClient.PutAsJsonAsync($"/api/playerstats/{created!.Id}", new
        {
            id = created.Id,
            playerId = player.Id,
            scheduledEventId = match.Id,
            goals = 3,
            assists = 2,
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var stats = await adminClient.GetFromJsonAsync<List<PlayerStat>>(
            $"/api/playerstats?playerId={player.Id}&eventId={match.Id}", ScheduleTestHelpers.Json);
        Assert.Equal(3, stats![0].Goals);
        Assert.Equal(2, stats[0].Assists);
    }

    // PUT rejects negative values on the corrected row too.
    [Fact]
    public async Task UpdatePlayerStat_WithNegativeGoals_ReturnsBadRequest()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var match = await ScheduleTestHelpers.CreateMatchAsync(adminClient);
        var player = await ScheduleTestHelpers.CreatePlayerAsync(adminClient, "bad-correction@example.com");
        var created = await (await ScheduleTestHelpers.RecordStatAsync(adminClient, player.Id, match.Id, goals: 1))
            .Content.ReadFromJsonAsync<PlayerStat>(ScheduleTestHelpers.Json);

        var response = await adminClient.PutAsJsonAsync($"/api/playerstats/{created!.Id}", new
        {
            id = created.Id,
            playerId = player.Id,
            scheduledEventId = match.Id,
            goals = -5,
            assists = 0,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Updating an id that doesn't exist returns 404.
    [Fact]
    public async Task UpdatePlayerStat_NotFound_ReturnsNotFound()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var response = await adminClient.PutAsJsonAsync("/api/playerstats/999999", new
        {
            id = 999999,
            playerId = 1,
            scheduledEventId = 1,
            goals = 1,
            assists = 0,
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Deleting an existing row returns 204 and removes it (e.g. a player was
    // ticked off the line-up by mistake).
    [Fact]
    public async Task DeletePlayerStat_WithValidId_ReturnsNoContentAndRemovesRow()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var match = await ScheduleTestHelpers.CreateMatchAsync(adminClient);
        var player = await ScheduleTestHelpers.CreatePlayerAsync(adminClient, "remove-me@example.com");
        var created = await (await ScheduleTestHelpers.RecordStatAsync(adminClient, player.Id, match.Id, goals: 1))
            .Content.ReadFromJsonAsync<PlayerStat>(ScheduleTestHelpers.Json);

        var response = await adminClient.DeleteAsync($"/api/playerstats/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var stats = await adminClient.GetFromJsonAsync<List<PlayerStat>>(
            $"/api/playerstats?playerId={player.Id}&eventId={match.Id}", ScheduleTestHelpers.Json);
        Assert.Empty(stats!);
    }

    // Deleting an id that doesn't exist returns 404.
    [Fact]
    public async Task DeletePlayerStat_NotFound_ReturnsNotFound()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var response = await adminClient.DeleteAsync("/api/playerstats/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
