using System.Net.Http.Json;
using SportsClubApi.Dtos;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

public class StatsControllerTests
{
    // TC-32: A player's totals add up goals, assists and matches across matches,
    // and the top scorer is listed first.
    [Fact]
    public async Task GetPlayerTotals_SumsAcrossMatches_AndSortsTopScorerFirst()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var matchOne = await ScheduleTestHelpers.CreateMatchAsync(adminClient, date: "2026-10-05");
        var matchTwo = await ScheduleTestHelpers.CreateMatchAsync(adminClient, date: "2026-10-12", opponent: "Grey Lynn");
        var striker = await ScheduleTestHelpers.CreatePlayerAsync(adminClient, "striker@example.com");
        var winger = await ScheduleTestHelpers.CreatePlayerAsync(adminClient, "winger@example.com");

        (await ScheduleTestHelpers.RecordStatAsync(adminClient, striker.Id, matchOne.Id, goals: 2, assists: 0)).EnsureSuccessStatusCode();
        (await ScheduleTestHelpers.RecordStatAsync(adminClient, striker.Id, matchTwo.Id, goals: 1, assists: 1)).EnsureSuccessStatusCode();
        (await ScheduleTestHelpers.RecordStatAsync(adminClient, winger.Id, matchOne.Id, goals: 0, assists: 2)).EnsureSuccessStatusCode();

        var playerClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Player);
        var totals = await playerClient.GetFromJsonAsync<List<PlayerStatsSummary>>("/api/stats/players");

        Assert.NotNull(totals);
        var first = totals![0];
        Assert.Equal(striker.Id, first.PlayerId);
        Assert.Equal(2, first.Matches);
        Assert.Equal(3, first.Goals);
        Assert.Equal(1, first.Assists);

        var second = totals.Single(t => t.PlayerId == winger.Id);
        Assert.Equal(1, second.Matches);
        Assert.Equal(0, second.Goals);
        Assert.Equal(2, second.Assists);
    }

    // TC-33: The team record counts only matches with a score, and works out
    // wins, draws, losses and goals; an unscored match counts as upcoming.
    [Fact]
    public async Task GetTeamStats_ComputesRecordFromScoredMatches()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        await ScheduleTestHelpers.CreateMatchAsync(adminClient, date: "2026-09-01", goalsFor: 3, goalsAgainst: 1); // win
        await ScheduleTestHelpers.CreateMatchAsync(adminClient, date: "2026-09-08", goalsFor: 2, goalsAgainst: 2); // draw
        await ScheduleTestHelpers.CreateMatchAsync(adminClient, date: "2026-09-15", goalsFor: 0, goalsAgainst: 1); // loss
        await ScheduleTestHelpers.CreateMatchAsync(adminClient, date: "2026-12-01");                                // upcoming
        await ScheduleTestHelpers.CreateTrainingAsync(adminClient);                                                 // not a match

        var coachClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Coach);
        var team = await coachClient.GetFromJsonAsync<TeamStatsSummary>("/api/stats/team");

        Assert.NotNull(team);
        Assert.Equal(3, team!.MatchesPlayed);
        Assert.Equal(1, team.Wins);
        Assert.Equal(1, team.Draws);
        Assert.Equal(1, team.Losses);
        Assert.Equal(5, team.GoalsFor);
        Assert.Equal(4, team.GoalsAgainst);
        Assert.Equal(1, team.UpcomingMatches);
    }
}
