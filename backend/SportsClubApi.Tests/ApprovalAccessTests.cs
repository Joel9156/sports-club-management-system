using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SportsClubApi.Dtos;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

public class ApprovalAccessTests
{
    // TC-48: An unapproved account gets a 403 with a "waiting for approval"
    // message on protected endpoints, even with a valid token.
    [Fact]
    public async Task UnapprovedAccount_IsBlockedFromProtectedEndpoints()
    {
        using var factory = new SportsClubApiFactory();
        var unapproved = await RegisterAsync(factory, "unapproved-player@example.com");

        var response = await unapproved.GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("waiting for approval", body);
    }

    // TC-49: The same token works once an Admin approves the account, without
    // logging in again.
    [Fact]
    public async Task Approval_TakesEffectWithoutReLogin()
    {
        using var factory = new SportsClubApiFactory();
        var unapproved = await RegisterAsync(factory, "approve-me@example.com");
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var pending = await adminClient.GetFromJsonAsync<List<PendingAccountSummary>>(
            "/api/users/pending", ScheduleTestHelpers.Json);
        var id = pending!.Single(p => p.Email == "approve-me@example.com").Id;
        await adminClient.PostAsync($"/api/users/{id}/approve", null);

        var response = await unapproved.GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<HttpClient> RegisterAsync(SportsClubApiFactory factory, string email)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Unapproved Person",
            email,
            password = TestHelpers.DefaultPassword,
            role = "Player",
        });
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }
}
