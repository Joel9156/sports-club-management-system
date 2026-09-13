using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SportsClubApi.Data;
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

    // GitHub #2: registering a Player account must also create a Player
    // roster record (same name/email) so the new player shows up on the
    // Admin Players list without a separate, easy-to-skip step.
    [Fact]
    public async Task Register_WithPlayerRole_CreatesLinkedPlayerRecord()
    {
        using var factory = new SportsClubApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = "new-player@example.com",
            Password = TestHelpers.DefaultPassword,
            FullName = "New Player",
            Role = UserRole.Player,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var player = await context.Players.SingleOrDefaultAsync(p => p.Email == "new-player@example.com");

        Assert.NotNull(player);
        Assert.Equal("New Player", player!.FullName);
    }

    // Volunteer accounts don't belong on the player roster.
    [Fact]
    public async Task Register_WithVolunteerRole_DoesNotCreatePlayerRecord()
    {
        using var factory = new SportsClubApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = "new-volunteer@example.com",
            Password = TestHelpers.DefaultPassword,
            FullName = "New Volunteer",
            Role = UserRole.Volunteer,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var player = await context.Players.SingleOrDefaultAsync(p => p.Email == "new-volunteer@example.com");

        Assert.Null(player);
    }

    // Registering shouldn't create a second, conflicting Player record when
    // one already exists for that email (e.g. an admin pre-registered the
    // player on the roster before they created their own login).
    [Fact]
    public async Task Register_WithPlayerRole_DoesNotDuplicateExistingPlayerRecord()
    {
        using var factory = new SportsClubApiFactory();

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Players.Add(new Player
            {
                FullName = "Existing Player",
                Email = "existing-player@example.com",
                RegistrationDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IsActive = true,
            });
            await context.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = "existing-player@example.com",
            Password = TestHelpers.DefaultPassword,
            FullName = "Existing Player",
            Role = UserRole.Player,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var verifyScope = factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var playerCount = await verifyContext.Players.CountAsync(p => p.Email == "existing-player@example.com");

        Assert.Equal(1, playerCount);
    }
}
