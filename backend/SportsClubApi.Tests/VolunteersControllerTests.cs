using System.Net;
using System.Net.Http.Json;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

public class VolunteersControllerTests
{
    // TC-05: Valid volunteer creation returns 201.
    [Fact]
    public async Task CreateVolunteer_WithValidData_ReturnsCreated()
    {
        using var factory = new SportsClubApiFactory();
        var client = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var response = await client.PostAsJsonAsync("/api/volunteers", new
        {
            fullName = "Taylor Morgan",
            email = "taylor.morgan@example.com",
            phone = "555-0202",
            role = "Coach Assistant",
            availability = "Weekends",
            isActive = true,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Volunteer>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal("Taylor Morgan", created.FullName);
    }

    private static async Task<Volunteer> CreateVolunteerAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/volunteers", new
        {
            fullName = $"Volunteer {email}",
            email,
            phone = "555-0000",
            role = "Manager",
            availability = "Weekends",
            isActive = true,
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Volunteer>())!;
    }

    // GET by id returns the matching volunteer.
    [Fact]
    public async Task GetVolunteer_WithValidId_ReturnsVolunteer()
    {
        using var factory = new SportsClubApiFactory();
        var client = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var volunteer = await CreateVolunteerAsync(client, "found@example.com");

        var response = await client.GetAsync($"/api/volunteers/{volunteer.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fetched = await response.Content.ReadFromJsonAsync<Volunteer>();
        Assert.Equal(volunteer.Id, fetched!.Id);
        Assert.Equal("found@example.com", fetched.Email);
    }

    // GET by an id that doesn't exist returns 404.
    [Fact]
    public async Task GetVolunteer_WithInvalidId_ReturnsNotFound()
    {
        using var factory = new SportsClubApiFactory();
        var client = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var response = await client.GetAsync("/api/volunteers/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // A valid PUT updates the record and returns 204.
    [Fact]
    public async Task UpdateVolunteer_WithValidData_ReturnsNoContentAndPersists()
    {
        using var factory = new SportsClubApiFactory();
        var client = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var volunteer = await CreateVolunteerAsync(client, "update-me@example.com");

        var response = await client.PutAsJsonAsync($"/api/volunteers/{volunteer.Id}", new
        {
            id = volunteer.Id,
            fullName = "Updated Name",
            email = volunteer.Email,
            phone = "555-9999",
            role = "First Aid",
            availability = "Sunday mornings",
            isActive = false,
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var saved = await client.GetFromJsonAsync<Volunteer>($"/api/volunteers/{volunteer.Id}");
        Assert.Equal("Updated Name", saved!.FullName);
        Assert.Equal("555-9999", saved.Phone);
        Assert.False(saved.IsActive);
    }

    // The route id and body id must match, or the request is rejected with 400
    // before any lookup happens.
    [Fact]
    public async Task UpdateVolunteer_IdMismatch_ReturnsBadRequest()
    {
        using var factory = new SportsClubApiFactory();
        var client = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var volunteer = await CreateVolunteerAsync(client, "mismatch@example.com");

        var response = await client.PutAsJsonAsync($"/api/volunteers/{volunteer.Id}", new
        {
            id = volunteer.Id + 1,
            fullName = volunteer.FullName,
            email = volunteer.Email,
            phone = volunteer.Phone,
            role = volunteer.Role,
            availability = volunteer.Availability,
            isActive = volunteer.IsActive,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Updating an id that doesn't exist returns 404, not a server error.
    [Fact]
    public async Task UpdateVolunteer_NotFound_ReturnsNotFound()
    {
        using var factory = new SportsClubApiFactory();
        var client = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var response = await client.PutAsJsonAsync("/api/volunteers/999999", new
        {
            id = 999999,
            fullName = "Ghost",
            email = "ghost@example.com",
            phone = "555-0000",
            role = "Manager",
            availability = "Never",
            isActive = true,
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Deleting an existing volunteer returns 204 and removes it.
    [Fact]
    public async Task DeleteVolunteer_WithValidId_ReturnsNoContent()
    {
        using var factory = new SportsClubApiFactory();
        var client = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);
        var volunteer = await CreateVolunteerAsync(client, "delete-me@example.com");

        var response = await client.DeleteAsync($"/api/volunteers/{volunteer.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var afterDelete = await client.GetAsync($"/api/volunteers/{volunteer.Id}");
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }

    // Deleting an id that doesn't exist returns 404.
    [Fact]
    public async Task DeleteVolunteer_NotFound_ReturnsNotFound()
    {
        using var factory = new SportsClubApiFactory();
        var client = await TestHelpers.CreateAuthenticatedClientAsync(factory, UserRole.Admin);

        var response = await client.DeleteAsync("/api/volunteers/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
