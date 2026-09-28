using System.Net;
using System.Net.Http.Json;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

public class TeamsControllerTests
{
    // GET by an id that doesn't exist returns 404.
    [Fact]
    public async Task GetTeam_WithInvalidId_ReturnsNotFound()
    {
        using var factory = new SportsClubApiFactory();
        var client = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var response = await client.GetAsync("/api/teams/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Creating a team is Admin-only - a Coach is forbidden.
    [Fact]
    public async Task CreateTeam_AsCoach_ReturnsForbidden()
    {
        using var factory = new SportsClubApiFactory();
        var coachClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Coach);

        var response = await coachClient.PostAsJsonAsync("/api/teams", new
        {
            name = "U16 Panthers",
            ageGroup = "U16",
            coachName = "Coach Reid",
            season = "2026",
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // Every role can read the team list, not just Admin.
    [Fact]
    public async Task GetTeams_AsPlayer_ReturnsTeams()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        (await adminClient.PostAsJsonAsync("/api/teams", new
        {
            name = "U16 Panthers",
            ageGroup = "U16",
            coachName = "Coach Reid",
            season = "2026",
        })).EnsureSuccessStatusCode();

        var playerClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Player);
        var teams = await playerClient.GetFromJsonAsync<List<Team>>("/api/teams");

        Assert.NotNull(teams);
        Assert.Contains(teams!, t => t.Name == "U16 Panthers");
    }
}
