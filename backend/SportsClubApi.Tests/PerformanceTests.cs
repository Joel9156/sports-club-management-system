using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using SportsClubApi.Data;
using SportsClubApi.Models;
using Xunit.Abstractions;

namespace SportsClubApi.Tests;

// docs/03-proposed-solution.md sets a target of loading the dashboard's summary
// metrics within 3 seconds for a dataset of at least 500 records. The dashboard
// is built from GET /api/players and GET /api/volunteers (see
// AdminDashboardPage.jsx), so this seeds 500 of each and times those calls the
// way the page makes them (in parallel).
//
// Caveat: tests run against the in-memory database with no network between the
// client and API, so this measures the API's own work and JSON size, not
// browser rendering or real network latency. The 3-second limit is the
// project's target, kept generous on purpose so a slow CI machine doesn't make
// the test flaky; the elapsed time is printed so the real figure is visible.
public class PerformanceTests
{
    private const int RecordCount = 500;
    private static readonly TimeSpan Target = TimeSpan.FromSeconds(3);

    private readonly ITestOutputHelper _output;

    public PerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static async Task SeedAsync(SportsClubApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        for (var i = 0; i < RecordCount; i++)
        {
            context.Players.Add(new Player
            {
                FullName = $"Load Player {i}",
                Email = $"load-player-{i}@example.com",
                Phone = "555-0000",
                DateOfBirth = new DateOnly(2000, 1, 1),
                RegistrationDate = new DateOnly(2026, 1, 1),
                IsActive = i % 4 != 0,
            });
            context.Volunteers.Add(new Volunteer
            {
                FullName = $"Load Volunteer {i}",
                Email = $"load-volunteer-{i}@example.com",
                Phone = "555-0000",
                Role = "Helper",
                Availability = "Weekends",
                IsActive = i % 5 != 0,
            });
        }

        await context.SaveChangesAsync();
    }

    // The two list calls behind the Admin dashboard, with 500 players and 500
    // volunteers, return everything and finish within the 3-second target.
    [Fact]
    public async Task DashboardLists_With500Records_LoadWithinTarget()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        await SeedAsync(factory);

        var stopwatch = Stopwatch.StartNew();
        var playersTask = adminClient.GetFromJsonAsync<List<Player>>("/api/players");
        var volunteersTask = adminClient.GetFromJsonAsync<List<Volunteer>>("/api/volunteers");
        var players = await playersTask;
        var volunteers = await volunteersTask;
        stopwatch.Stop();

        _output.WriteLine(
            $"Dashboard lists ({players!.Count} players, {volunteers!.Count} volunteers): {stopwatch.ElapsedMilliseconds} ms");

        Assert.Equal(RecordCount, players.Count);
        Assert.Equal(RecordCount, volunteers.Count);
        Assert.True(stopwatch.Elapsed < Target,
            $"Dashboard lists took {stopwatch.ElapsedMilliseconds} ms; target is {Target.TotalMilliseconds} ms.");
    }

    // The dashboard's counts, worked out the same way the page does it, are
    // right for a large dataset (guards the client-side computation's input).
    [Fact]
    public async Task DashboardCounts_With500Records_AreCorrect()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        await SeedAsync(factory);

        var players = await adminClient.GetFromJsonAsync<List<Player>>("/api/players");
        var volunteers = await adminClient.GetFromJsonAsync<List<Volunteer>>("/api/volunteers");

        // Seed rule: every 4th player and every 5th volunteer is inactive.
        Assert.Equal(RecordCount - (RecordCount / 4), players!.Count(p => p.IsActive));
        Assert.Equal(RecordCount - (RecordCount / 5), volunteers!.Count(v => v.IsActive));
    }
}
