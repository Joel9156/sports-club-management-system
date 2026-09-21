using System.Net;
using System.Net.Http.Json;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

// docs/05-test-cases.md now has TC-09 through TC-11 for Attendance; those
// three tests are labelled to match below. GetAttendance_ReturnsOkWithRecordedSessions
// and GetAttendanceRecord_ById_ReturnsCorrectRecord are supplementary
// automated coverage beyond that manual list, so they're left unlabelled.
public class AttendanceControllerTests
{
    // TC-09: Valid attendance creation (Coach role) returns 201.
    [Fact]
    public async Task RecordAttendance_WithValidData_ReturnsCreated()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var playerResponse = await adminClient.PostAsJsonAsync("/api/players", new
        {
            fullName = "Jordan Ashby",
            dateOfBirth = "2012-02-14",
            email = "jordan.ashby@example.com",
            phone = "555-0601",
            registrationDate = "2026-01-15",
            isActive = true,
        });
        playerResponse.EnsureSuccessStatusCode();
        var player = await playerResponse.Content.ReadFromJsonAsync<Player>();

        var coachClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Coach);

        var response = await coachClient.PostAsJsonAsync("/api/attendance", new
        {
            playerId = player!.Id,
            sessionDate = "2026-02-01",
            isPresent = true,
            notes = "",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Attendance>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal(player.Id, created.PlayerId);
    }

    // TC-10: Recording attendance without a bearer token returns 401.
    [Fact]
    public async Task RecordAttendance_WithoutAuth_ReturnsUnauthorized()
    {
        using var factory = new SportsClubApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/attendance", new
        {
            playerId = 1,
            sessionDate = "2026-02-01",
            isPresent = true,
            notes = "",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // GET /api/attendance returns every recorded session.
    [Fact]
    public async Task GetAttendance_ReturnsOkWithRecordedSessions()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var playerResponse = await adminClient.PostAsJsonAsync("/api/players", new
        {
            fullName = "Casey Ford",
            dateOfBirth = "2011-06-30",
            email = "casey.ford@example.com",
            phone = "555-0602",
            registrationDate = "2026-01-15",
            isActive = true,
        });
        playerResponse.EnsureSuccessStatusCode();
        var player = await playerResponse.Content.ReadFromJsonAsync<Player>();

        var recordResponse = await adminClient.PostAsJsonAsync("/api/attendance", new
        {
            playerId = player!.Id,
            sessionDate = "2026-02-01",
            isPresent = true,
            notes = "",
        });
        recordResponse.EnsureSuccessStatusCode();

        var response = await adminClient.GetAsync("/api/attendance");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var records = await response.Content.ReadFromJsonAsync<List<Attendance>>();
        Assert.NotNull(records);
        Assert.Single(records!);
        Assert.Equal(player.Id, records![0].PlayerId);
        Assert.True(records[0].IsPresent);
    }

    // TC-11: ?teamId= filters attendance down to players on that team (via a
    // join on Player.TeamId - Attendance itself has no TeamId column).
    [Fact]
    public async Task GetAttendance_FilteredByTeamId_ReturnsOnlyMatchingRecords()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var teamAResponse = await adminClient.PostAsJsonAsync("/api/teams", new
        {
            name = "U12 Eagles",
            ageGroup = "U12",
            coachName = "Coach Lee",
            season = "2026",
        });
        teamAResponse.EnsureSuccessStatusCode();
        var teamA = await teamAResponse.Content.ReadFromJsonAsync<Team>();

        var teamBResponse = await adminClient.PostAsJsonAsync("/api/teams", new
        {
            name = "U14 Falcons",
            ageGroup = "U14",
            coachName = "Coach Patel",
            season = "2026",
        });
        teamBResponse.EnsureSuccessStatusCode();
        var teamB = await teamBResponse.Content.ReadFromJsonAsync<Team>();

        var playerAResponse = await adminClient.PostAsJsonAsync("/api/players", new
        {
            fullName = "Sam Okafor",
            dateOfBirth = "2011-09-20",
            email = "sam.okafor@example.com",
            phone = "555-0603",
            registrationDate = "2026-01-15",
            isActive = true,
        });
        playerAResponse.EnsureSuccessStatusCode();
        var playerA = await playerAResponse.Content.ReadFromJsonAsync<Player>();

        var playerBResponse = await adminClient.PostAsJsonAsync("/api/players", new
        {
            fullName = "Robin Fisher",
            dateOfBirth = "2010-04-05",
            email = "robin.fisher@example.com",
            phone = "555-0604",
            registrationDate = "2026-01-15",
            isActive = true,
        });
        playerBResponse.EnsureSuccessStatusCode();
        var playerB = await playerBResponse.Content.ReadFromJsonAsync<Player>();

        // Assign each player to a different team.
        (await adminClient.PutAsJsonAsync($"/api/players/{playerA!.Id}", new
        {
            id = playerA.Id,
            fullName = playerA.FullName,
            dateOfBirth = playerA.DateOfBirth,
            email = playerA.Email,
            phone = playerA.Phone,
            teamId = teamA!.Id,
            registrationDate = playerA.RegistrationDate,
            isActive = playerA.IsActive,
        })).EnsureSuccessStatusCode();

        (await adminClient.PutAsJsonAsync($"/api/players/{playerB!.Id}", new
        {
            id = playerB.Id,
            fullName = playerB.FullName,
            dateOfBirth = playerB.DateOfBirth,
            email = playerB.Email,
            phone = playerB.Phone,
            teamId = teamB!.Id,
            registrationDate = playerB.RegistrationDate,
            isActive = playerB.IsActive,
        })).EnsureSuccessStatusCode();

        (await adminClient.PostAsJsonAsync("/api/attendance", new
        {
            playerId = playerA.Id,
            sessionDate = "2026-02-01",
            isPresent = true,
            notes = "",
        })).EnsureSuccessStatusCode();

        (await adminClient.PostAsJsonAsync("/api/attendance", new
        {
            playerId = playerB.Id,
            sessionDate = "2026-02-01",
            isPresent = false,
            notes = "",
        })).EnsureSuccessStatusCode();

        var response = await adminClient.GetAsync($"/api/attendance?teamId={teamA.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var records = await response.Content.ReadFromJsonAsync<List<Attendance>>();
        Assert.NotNull(records);
        Assert.Single(records!);
        Assert.Equal(playerA.Id, records![0].PlayerId);
    }

    // GET /api/attendance/{id} returns the exact record that was created.
    [Fact]
    public async Task GetAttendanceRecord_ById_ReturnsCorrectRecord()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var playerResponse = await adminClient.PostAsJsonAsync("/api/players", new
        {
            fullName = "Drew Whitman",
            dateOfBirth = "2013-11-02",
            email = "drew.whitman@example.com",
            phone = "555-0605",
            registrationDate = "2026-01-15",
            isActive = true,
        });
        playerResponse.EnsureSuccessStatusCode();
        var player = await playerResponse.Content.ReadFromJsonAsync<Player>();

        var recordResponse = await adminClient.PostAsJsonAsync("/api/attendance", new
        {
            playerId = player!.Id,
            sessionDate = "2026-02-08",
            isPresent = false,
            notes = "Injured",
        });
        recordResponse.EnsureSuccessStatusCode();
        var created = await recordResponse.Content.ReadFromJsonAsync<Attendance>();

        var response = await adminClient.GetAsync($"/api/attendance/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var record = await response.Content.ReadFromJsonAsync<Attendance>();
        Assert.NotNull(record);
        Assert.Equal(created.Id, record!.Id);
        Assert.Equal(player.Id, record.PlayerId);
        Assert.False(record.IsPresent);
        Assert.Equal("Injured", record.Notes);
    }

    // TC-17: Only Admin/Coach can record attendance - a Player role is forbidden.
    [Fact]
    public async Task RecordAttendance_AsPlayerRole_ReturnsForbidden()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var playerResponse = await adminClient.PostAsJsonAsync("/api/players", new
        {
            fullName = "Riley Chen",
            dateOfBirth = "2013-03-11",
            email = "riley.chen.attendance@example.com",
            phone = "555-0303",
            registrationDate = "2026-01-15",
            isActive = true,
        });
        playerResponse.EnsureSuccessStatusCode();
        var player = await playerResponse.Content.ReadFromJsonAsync<Player>();

        var playerClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Player);

        var response = await playerClient.PostAsJsonAsync("/api/attendance", new
        {
            playerId = player!.Id,
            sessionDate = "2026-02-01",
            isPresent = true,
            notes = "",
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // TC-18: Recording attendance against a nonexistent player returns 400.
    [Fact]
    public async Task RecordAttendance_WithNonexistentPlayer_ReturnsBadRequest()
    {
        using var factory = new SportsClubApiFactory();
        var coachClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Coach);

        var response = await coachClient.PostAsJsonAsync("/api/attendance", new
        {
            playerId = 999999,
            sessionDate = "2026-02-01",
            isPresent = true,
            notes = "",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // TC-19: ?date= filters attendance down to records for that session date.
    [Fact]
    public async Task GetAttendance_FilteredByDate_ReturnsOnlyMatchingRecords()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var playerResponse = await adminClient.PostAsJsonAsync("/api/players", new
        {
            fullName = "Attendance Filter Test",
            dateOfBirth = "2012-05-04",
            email = "attendance-filter@example.com",
            phone = "555-0101",
            registrationDate = "2026-01-15",
            isActive = true,
        });
        playerResponse.EnsureSuccessStatusCode();
        var player = await playerResponse.Content.ReadFromJsonAsync<Player>();

        await adminClient.PostAsJsonAsync("/api/attendance", new
        {
            playerId = player!.Id,
            sessionDate = "2026-03-01",
            isPresent = true,
            notes = "",
        });
        await adminClient.PostAsJsonAsync("/api/attendance", new
        {
            playerId = player.Id,
            sessionDate = "2026-03-08",
            isPresent = false,
            notes = "",
        });

        var records = await adminClient.GetFromJsonAsync<List<Attendance>>(
            "/api/attendance?date=2026-03-01");

        Assert.NotNull(records);
        Assert.Single(records!);
        Assert.Equal("2026-03-01", records![0].SessionDate.ToString("yyyy-MM-dd"));
    }

    // DEF-03: a second record for the same player and session date is rejected
    // with 409 instead of leaving two contradictory rows.
    [Fact]
    public async Task RecordAttendance_DuplicatePlayerAndDate_ReturnsConflict()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var playerResponse = await adminClient.PostAsJsonAsync("/api/players", new
        {
            fullName = "Alex Duplicate",
            dateOfBirth = "2012-01-01",
            email = "alex.duplicate@example.com",
            phone = "555-0701",
            registrationDate = "2026-01-15",
            isActive = true,
        });
        playerResponse.EnsureSuccessStatusCode();
        var player = await playerResponse.Content.ReadFromJsonAsync<Player>();

        var first = await adminClient.PostAsJsonAsync("/api/attendance", new
        {
            playerId = player!.Id,
            sessionDate = "2026-04-01",
            isPresent = true,
            notes = "",
        });
        first.EnsureSuccessStatusCode();

        var second = await adminClient.PostAsJsonAsync("/api/attendance", new
        {
            playerId = player.Id,
            sessionDate = "2026-04-01",
            isPresent = false,
            notes = "",
        });

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        var records = await adminClient.GetFromJsonAsync<List<Attendance>>(
            $"/api/attendance?playerId={player.Id}&date=2026-04-01");
        Assert.Single(records!);
        Assert.True(records![0].IsPresent);
    }

    // The uniqueness rule is per (player, date) - the same player on another
    // date is still fine.
    [Fact]
    public async Task RecordAttendance_SamePlayerDifferentDate_ReturnsCreated()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var playerResponse = await adminClient.PostAsJsonAsync("/api/players", new
        {
            fullName = "Blake Sessions",
            dateOfBirth = "2012-01-01",
            email = "blake.sessions@example.com",
            phone = "555-0702",
            registrationDate = "2026-01-15",
            isActive = true,
        });
        playerResponse.EnsureSuccessStatusCode();
        var player = await playerResponse.Content.ReadFromJsonAsync<Player>();

        (await adminClient.PostAsJsonAsync("/api/attendance", new
        {
            playerId = player!.Id,
            sessionDate = "2026-04-01",
            isPresent = true,
            notes = "",
        })).EnsureSuccessStatusCode();

        var second = await adminClient.PostAsJsonAsync("/api/attendance", new
        {
            playerId = player.Id,
            sessionDate = "2026-04-08",
            isPresent = true,
            notes = "",
        });

        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
    }
}
