using System.Net;
using System.Net.Http.Json;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

public class AuthorizationTests
{
    // TC-07: A non-admin (Player) role cannot access an admin-only endpoint.
    [Fact]
    public async Task DeletePlayer_AsPlayerRole_ReturnsForbidden()
    {
        using var factory = new SportsClubApiFactory();
        var adminClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var createResponse = await adminClient.PostAsJsonAsync("/api/players", new
        {
            fullName = "Riley Chen",
            dateOfBirth = "2013-03-11",
            email = "riley.chen@example.com",
            phone = "555-0303",
            registrationDate = "2026-01-15",
            isActive = true,
        });
        createResponse.EnsureSuccessStatusCode();
        var player = await createResponse.Content.ReadFromJsonAsync<Player>();

        var playerClient = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Player);

        var deleteResponse = await playerClient.DeleteAsync($"/api/players/{player!.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }
}
