using System.Net.Http.Json;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

// The API has no dedicated "/api/dashboard" endpoint - the admin dashboard is
// composed client-side from the players/teams/volunteers list endpoints (see
// frontend/src/pages/admin/AdminDashboardPage.jsx). TC-08 is covered here by
// verifying those endpoints report the correct counts after data is created,
// which is exactly what the dashboard displays.
public class DashboardTests
{
    // TC-08: Dashboard data (player/team/volunteer counts) reflects created data.
    [Fact]
    public async Task ListEndpoints_AfterCreatingData_ReportCorrectCounts()
    {
        using var factory = new SportsClubApiFactory();
        var client = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var teamResponse = await client.PostAsJsonAsync("/api/teams", new
        {
            name = "U14 Falcons",
            ageGroup = "U14",
            coachName = "Coach Patel",
            season = "2026",
        });
        teamResponse.EnsureSuccessStatusCode();

        var playerResponse = await client.PostAsJsonAsync("/api/players", new
        {
            fullName = "Morgan Blake",
            dateOfBirth = "2010-07-01",
            email = "morgan.blake@example.com",
            phone = "555-0404",
            registrationDate = "2026-01-15",
            isActive = true,
        });
        playerResponse.EnsureSuccessStatusCode();

        var volunteerResponse = await client.PostAsJsonAsync("/api/volunteers", new
        {
            fullName = "Casey Nguyen",
            email = "casey.nguyen@example.com",
            phone = "555-0505",
            role = "First Aid",
            availability = "Saturdays",
            isActive = true,
        });
        volunteerResponse.EnsureSuccessStatusCode();

        // Each test runs against its own isolated InMemory database (see
        // SportsClubApiFactory), so these counts are exactly what was created above.
        var teams = await client.GetFromJsonAsync<List<Team>>("/api/teams") ?? [];
        var players = await client.GetFromJsonAsync<List<Player>>("/api/players") ?? [];
        var volunteers = await client.GetFromJsonAsync<List<Volunteer>>("/api/volunteers") ?? [];

        Assert.Single(teams);
        Assert.Single(players);
        Assert.Single(volunteers);
    }
}
