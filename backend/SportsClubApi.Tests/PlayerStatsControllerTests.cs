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

    // TC-30: Only Admins enter stats - a Player is forbidden.
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
}
