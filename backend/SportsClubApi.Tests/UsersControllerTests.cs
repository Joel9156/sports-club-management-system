using System.Net;
using System.Net.Http.Json;
using SportsClubApi.Dtos;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

public class UsersControllerTests
{
    // TC-45: A pending Player registration shows up for an Admin to review,
    // and approving it creates the Player roster record.
    [Fact]
    public async Task ApprovePlayer_CreatesPlayerRecord_AndLeavesPending()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var register = await factory.CreateClient().PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Pending Player",
            email = "pending-player@example.com",
            password = TestHelpers.DefaultPassword,
            role = "Player",
        });
        var registered = await register.Content.ReadFromJsonAsync<AuthResponse>();

        var pending = await adminClient.GetFromJsonAsync<List<PendingAccountSummary>>("/api/users/pending", ScheduleTestHelpers.Json);
        Assert.Contains(pending!, p => p.Email == "pending-player@example.com");

        var approve = await adminClient.PostAsync($"/api/users/{Id(pending!, "pending-player@example.com")}/approve", null);
        Assert.Equal(HttpStatusCode.NoContent, approve.StatusCode);

        var player = await adminClient.GetFromJsonAsync<List<Player>>("/api/players");
        Assert.Contains(player!, p => p.Email == "pending-player@example.com" && p.FullName == "Pending Player");

        var stillPending = await adminClient.GetFromJsonAsync<List<PendingAccountSummary>>("/api/users/pending", ScheduleTestHelpers.Json);
        Assert.DoesNotContain(stillPending!, p => p.Email == "pending-player@example.com");

        Assert.False(registered!.IsApproved); // sanity check on the earlier registration response
    }

    // TC-46: Approving a Volunteer whose email already has a Volunteer record
    // (e.g. an Admin pre-created it) doesn't create a duplicate.
    [Fact]
    public async Task ApproveVolunteer_WithExistingRecord_DoesNotDuplicateIt()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        await adminClient.PostAsJsonAsync("/api/volunteers", new
        {
            fullName = "Existing Volunteer",
            email = "existing-volunteer@example.com",
            phone = "555-0000",
            role = "Manager",
            availability = "Weekends",
            isActive = true,
        });

        await factory.CreateClient().PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Existing Volunteer",
            email = "existing-volunteer@example.com",
            password = TestHelpers.DefaultPassword,
            role = "Volunteer",
        });
        var pending = await adminClient.GetFromJsonAsync<List<PendingAccountSummary>>("/api/users/pending", ScheduleTestHelpers.Json);

        var approve = await adminClient.PostAsync($"/api/users/{Id(pending!, "existing-volunteer@example.com")}/approve", null);
        Assert.Equal(HttpStatusCode.NoContent, approve.StatusCode);

        var volunteers = await adminClient.GetFromJsonAsync<List<Volunteer>>("/api/volunteers");
        Assert.Single(volunteers!, v => v.Email == "existing-volunteer@example.com");
    }

    // TC-47: A Coach cannot see or approve pending accounts.
    [Fact]
    public async Task PendingAccounts_AsCoach_ReturnsForbidden()
    {
        using var factory = new SportsClubApiFactory();
        var coachClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Coach);

        var list = await coachClient.GetAsync("/api/users/pending");
        var approve = await coachClient.PostAsync("/api/users/1/approve", null);

        Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, approve.StatusCode);
    }

    private static int Id(List<PendingAccountSummary> pending, string email) =>
        pending.Single(p => p.Email == email).Id;
}
