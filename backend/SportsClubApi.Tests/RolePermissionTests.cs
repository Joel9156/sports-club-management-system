using System.Net;
using System.Net.Http.Json;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

// Who may register, edit and delete players and volunteers: Admins do; a
// Player may only complete their own record; nobody else can.
public class RolePermissionTests
{
    private static object PlayerBody(Player p, string? email = null, string? name = null,
        string dob = "2000-01-01", string phone = "555-0000", bool isActive = true) => new
    {
        id = p.Id,
        fullName = name ?? p.FullName,
        dateOfBirth = dob,
        email = email ?? p.Email,
        phone,
        registrationDate = p.RegistrationDate,
        isActive,
    };

    // TC-37: Registering a player is Admin-only - a Player gets 403.
    [Fact]
    public async Task CreatePlayer_AsPlayer_ReturnsForbidden()
    {
        using var factory = new SportsClubApiFactory();
        var playerClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Player);

        var response = await playerClient.PostAsJsonAsync("/api/players", new
        {
            fullName = "Self Registered",
            dateOfBirth = "2000-01-01",
            email = "self@example.com",
            phone = "555-0000",
            registrationDate = "2026-01-01",
            isActive = true,
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // TC-38: Registering a volunteer is Admin-only - a Volunteer gets 403.
    [Fact]
    public async Task CreateVolunteer_AsVolunteer_ReturnsForbidden()
    {
        using var factory = new SportsClubApiFactory();
        var volunteerClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Volunteer);

        var response = await volunteerClient.PostAsJsonAsync("/api/volunteers", new
        {
            fullName = "Self Volunteer",
            email = "vol@example.com",
            phone = "555-0000",
            role = "Manager",
            availability = "Weekends",
            isActive = true,
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // TC-39: A Player can complete their own record (date of birth, phone) but
    // not change whose it is or whether it's active.
    [Fact]
    public async Task UpdatePlayer_OwnRecord_UpdatesDetailsButNotProtectedFields()
    {
        using var factory = new SportsClubApiFactory();
        var user = await TestHelpers.SeedUserAsync(factory, UserRole.Player, "me@example.com");
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var mine = await ScheduleTestHelpers.CreatePlayerAsync(adminClient, "me@example.com");

        var playerClient = await TestHelpers.LoginAsync(factory, user);
        var response = await playerClient.PutAsJsonAsync($"/api/players/{mine.Id}",
            PlayerBody(mine, email: "someone-else@example.com", dob: "1999-09-09",
                phone: "021-000-0000", isActive: false));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var saved = await adminClient.GetFromJsonAsync<Player>($"/api/players/{mine.Id}");
        Assert.Equal("021-000-0000", saved!.Phone);
        Assert.Equal(new DateOnly(1999, 9, 9), saved.DateOfBirth);
        Assert.Equal("me@example.com", saved.Email);
        Assert.True(saved.IsActive);
    }

    // TC-40: A Player cannot edit someone else's record.
    [Fact]
    public async Task UpdatePlayer_AnotherPlayersRecord_ReturnsForbidden()
    {
        using var factory = new SportsClubApiFactory();
        var user = await TestHelpers.SeedUserAsync(factory, UserRole.Player, "me@example.com");
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var other = await ScheduleTestHelpers.CreatePlayerAsync(adminClient, "other@example.com");

        var playerClient = await TestHelpers.LoginAsync(factory, user);
        var response = await playerClient.PutAsJsonAsync($"/api/players/{other.Id}",
            PlayerBody(other, name: "Hijacked"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // TC-41: A Coach can't edit or delete player records - that's Admin-only.
    [Fact]
    public async Task EditOrDeletePlayer_AsCoach_ReturnsForbidden()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var player = await ScheduleTestHelpers.CreatePlayerAsync(adminClient, "target@example.com");
        var coachClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Coach);

        var edit = await coachClient.PutAsJsonAsync($"/api/players/{player.Id}",
            PlayerBody(player, name: "Edited"));
        var delete = await coachClient.DeleteAsync($"/api/players/{player.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, edit.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }
}
