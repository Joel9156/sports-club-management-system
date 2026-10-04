using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SportsClubApi.Dtos;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

public class RejectAccountTests
{
    // TC-50: An Admin can reject a pending account; it stays blocked from the
    // API and is marked as rejected on the pending list.
    [Fact]
    public async Task RejectPendingAccount_BlocksAccessAndMarksRejected()
    {
        using var factory = new SportsClubApiFactory();
        var applicant = await RegisterAsync(factory, "rejected-applicant@example.com");
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var id = await PendingIdAsync(adminClient, "rejected-applicant@example.com");

        var reject = await adminClient.PostAsync($"/api/users/{id}/reject", null);

        Assert.Equal(HttpStatusCode.NoContent, reject.StatusCode);
        var pending = await adminClient.GetFromJsonAsync<List<PendingAccountSummary>>(
            "/api/users/pending", ScheduleTestHelpers.Json);
        Assert.True(pending!.Single(p => p.Id == id).IsRejected);

        var blocked = await applicant.GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.Forbidden, blocked.StatusCode);
    }

    // TC-51: A rejected account can be approved later, which lets it in and
    // clears the rejected mark.
    [Fact]
    public async Task RejectedAccount_CanBeApprovedLater()
    {
        using var factory = new SportsClubApiFactory();
        var applicant = await RegisterAsync(factory, "reapproved@example.com");
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var id = await PendingIdAsync(adminClient, "reapproved@example.com");
        await adminClient.PostAsync($"/api/users/{id}/reject", null);

        var approve = await adminClient.PostAsync($"/api/users/{id}/approve", null);

        Assert.Equal(HttpStatusCode.NoContent, approve.StatusCode);
        var allowed = await applicant.GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
    }

    // TC-52: An approved account cannot be rejected (400), and rejecting an
    // unknown account returns 404.
    [Fact]
    public async Task Reject_ApprovedOrUnknownAccount_IsRejected()
    {
        using var factory = new SportsClubApiFactory();
        await RegisterAsync(factory, "already-approved@example.com");
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var id = await PendingIdAsync(adminClient, "already-approved@example.com");
        await adminClient.PostAsync($"/api/users/{id}/approve", null);

        var rejectApproved = await adminClient.PostAsync($"/api/users/{id}/reject", null);
        var rejectUnknown = await adminClient.PostAsync("/api/users/999999/reject", null);

        Assert.Equal(HttpStatusCode.BadRequest, rejectApproved.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, rejectUnknown.StatusCode);
    }

    // TC-53: A Coach cannot reject accounts.
    [Fact]
    public async Task Reject_AsCoach_ReturnsForbidden()
    {
        using var factory = new SportsClubApiFactory();
        var coachClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Coach);

        var response = await coachClient.PostAsync("/api/users/1/reject", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<HttpClient> RegisterAsync(SportsClubApiFactory factory, string email)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Applicant",
            email,
            password = TestHelpers.DefaultPassword,
            role = "Player",
        });
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    private static async Task<int> PendingIdAsync(HttpClient adminClient, string email)
    {
        var pending = await adminClient.GetFromJsonAsync<List<PendingAccountSummary>>(
            "/api/users/pending", ScheduleTestHelpers.Json);
        return pending!.Single(p => p.Email == email).Id;
    }
}
