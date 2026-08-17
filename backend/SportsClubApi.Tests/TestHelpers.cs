using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SportsClubApi.Data;
using SportsClubApi.Dtos;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

// Shared helpers for seeding users directly into a test's InMemory database
// and logging in through the real /api/auth/login endpoint to get a usable
// JWT, so tests exercise the actual auth flow instead of forging tokens.
internal static class TestHelpers
{
    public const string DefaultPassword = "Password123!";

    public static async Task<User> SeedUserAsync(
        SportsClubApiFactory factory,
        UserRole role,
        string? email = null,
        string password = DefaultPassword)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = new User
        {
            Email = email ?? $"{role.ToString().ToLowerInvariant()}-{Guid.NewGuid():N}@example.com",
            FullName = $"Test {role}",
            Role = role,
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user;
    }

    // Seeds a user with the given role, logs in as them via the real endpoint,
    // and returns an HttpClient with the resulting JWT already attached.
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        SportsClubApiFactory factory,
        UserRole role)
    {
        var user = await SeedUserAsync(factory, role);

        var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = user.Email,
            Password = DefaultPassword,
        });
        loginResponse.EnsureSuccessStatusCode();

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        return client;
    }
}
