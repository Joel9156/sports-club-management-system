using System.Net;
using System.Net.Http.Json;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

public class PlayersControllerTests
{
    // TC-03: Valid player creation returns 201.
    [Fact]
    public async Task CreatePlayer_WithValidData_ReturnsCreated()
    {
        using var factory = new SportsClubApiFactory();
        var client = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var response = await client.PostAsJsonAsync("/api/players", new
        {
            fullName = "Jamie Rivera",
            dateOfBirth = "2012-05-04",
            email = "jamie.rivera@example.com",
            phone = "555-0101",
            registrationDate = "2026-01-15",
            isActive = true,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Player>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal("Jamie Rivera", created.FullName);
    }

    // TC-04: Player creation with a missing required field (fullName) returns 400.
    [Fact]
    public async Task CreatePlayer_WithMissingFullName_ReturnsBadRequest()
    {
        using var factory = new SportsClubApiFactory();
        var client = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var response = await client.PostAsJsonAsync("/api/players", new
        {
            fullName = (string?)null,
            dateOfBirth = "2012-05-04",
            email = "jamie.rivera@example.com",
            phone = "555-0101",
            registrationDate = "2026-01-15",
            isActive = true,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // TC-06: Assigning a player to a team updates the player's record.
    [Fact]
    public async Task UpdatePlayer_AssignedToTeam_PersistsTeamAssignment()
    {
        using var factory = new SportsClubApiFactory();
        var client = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var teamResponse = await client.PostAsJsonAsync("/api/teams", new
        {
            name = "U12 Eagles",
            ageGroup = "U12",
            coachName = "Coach Lee",
            season = "2026",
        });
        teamResponse.EnsureSuccessStatusCode();
        var team = await teamResponse.Content.ReadFromJsonAsync<Team>();

        var playerResponse = await client.PostAsJsonAsync("/api/players", new
        {
            fullName = "Sam Okafor",
            dateOfBirth = "2011-09-20",
            email = "sam.okafor@example.com",
            phone = "555-0102",
            registrationDate = "2026-01-15",
            isActive = true,
        });
        playerResponse.EnsureSuccessStatusCode();
        var player = await playerResponse.Content.ReadFromJsonAsync<Player>();

        var updateResponse = await client.PutAsJsonAsync($"/api/players/{player!.Id}", new
        {
            id = player.Id,
            fullName = player.FullName,
            dateOfBirth = player.DateOfBirth,
            email = player.Email,
            phone = player.Phone,
            teamId = team!.Id,
            registrationDate = player.RegistrationDate,
            isActive = player.IsActive,
        });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var updatedPlayer = await client.GetFromJsonAsync<Player>($"/api/players/{player.Id}");

        Assert.NotNull(updatedPlayer);
        Assert.Equal(team.Id, updatedPlayer!.TeamId);
    }
}
