using System.Net;
using System.Net.Http.Json;
using SportsClubApi.Dtos;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

public class AuthControllerTests
{
    // TC-01: Valid login returns a JWT token.
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        using var factory = new SportsClubApiFactory();
        var user = await TestHelpers.SeedUserAsync(factory, UserRole.Admin);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = user.Email,
            Password = TestHelpers.DefaultPassword,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth!.Token));
        Assert.Equal(user.Email, auth.Email);
        Assert.Equal("Admin", auth.Role);
    }

    // TC-02: Invalid login (wrong password) returns 401.
    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        using var factory = new SportsClubApiFactory();
        var user = await TestHelpers.SeedUserAsync(factory, UserRole.Player);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = user.Email,
            Password = "WrongPassword123!",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // TC-02: Invalid login (unknown email) also returns 401.
    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        using var factory = new SportsClubApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "no-such-user@example.com",
            Password = TestHelpers.DefaultPassword,
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
