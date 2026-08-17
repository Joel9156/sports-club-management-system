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
}
