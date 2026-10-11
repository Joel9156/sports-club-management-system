using System.Net;
using System.Net.Http.Json;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

// Closes the gaps listed in docs/10-quality-testing.md: Admin-only endpoints
// not yet tried against every non-Admin role, and the list endpoints not yet
// tried without a token.
public class SecurityTests
{
    // DELETE /api/volunteers/{id} is Admin-only - Player, Volunteer and Coach
    // are all refused, and the record is still there afterwards.
    [Theory]
    [InlineData(UserRole.Player)]
    [InlineData(UserRole.Volunteer)]
    [InlineData(UserRole.Coach)]
    public async Task DeleteVolunteer_AsNonAdmin_ReturnsForbiddenAndKeepsRecord(UserRole role)
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var createResponse = await adminClient.PostAsJsonAsync("/api/volunteers", new
        {
            fullName = "Protected Volunteer",
            email = $"protected-{role}@example.com".ToLowerInvariant(),
            phone = "555-0000",
            role = "Manager",
            availability = "Weekends",
            isActive = true,
        });
        createResponse.EnsureSuccessStatusCode();
        var volunteer = (await createResponse.Content.ReadFromJsonAsync<Volunteer>())!;

        var otherClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, role);
        var response = await otherClient.DeleteAsync($"/api/volunteers/{volunteer.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var stillThere = await adminClient.GetAsync($"/api/volunteers/{volunteer.Id}");
        Assert.Equal(HttpStatusCode.OK, stillThere.StatusCode);
    }

    // POST /api/teams is Admin-only - Player, Volunteer and Coach are refused
    // and no team is created.
    [Theory]
    [InlineData(UserRole.Player)]
    [InlineData(UserRole.Volunteer)]
    [InlineData(UserRole.Coach)]
    public async Task CreateTeam_AsNonAdmin_ReturnsForbiddenAndCreatesNothing(UserRole role)
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var teamsBefore = (await adminClient.GetFromJsonAsync<List<Team>>("/api/teams"))!.Count;

        var otherClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, role);
        var response = await otherClient.PostAsJsonAsync("/api/teams", new
        {
            name = "Rogue Team",
            ageGroup = "Open",
            coachName = "Nobody",
            season = "2026",
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var teamsAfter = (await adminClient.GetFromJsonAsync<List<Team>>("/api/teams"))!.Count;
        Assert.Equal(teamsBefore, teamsAfter);
    }

    // Reading personal data needs a login: no token means 401 on every list
    // endpoint that exposes player, volunteer or team records.
    [Theory]
    [InlineData("/api/players")]
    [InlineData("/api/volunteers")]
    [InlineData("/api/teams")]
    [InlineData("/api/attendance")]
    [InlineData("/api/events")]
    [InlineData("/api/notifications")]
    public async Task GetList_WithoutToken_ReturnsUnauthorized(string url)
    {
        using var factory = new SportsClubApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // A garbage bearer token is treated the same as no token.
    [Fact]
    public async Task GetPlayers_WithInvalidToken_ReturnsUnauthorized()
    {
        using var factory = new SportsClubApiFactory();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "not.a.real.token");

        var response = await client.GetAsync("/api/players");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
