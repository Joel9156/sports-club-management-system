using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SportsClubApi.Data;

namespace SportsClubApi.Tests;

// A WebApplicationFactory that swaps the app's real SQLite database for a
// fresh, uniquely-named EF Core InMemory database. Each factory instance gets
// its own database, so tests that create their own factory (the pattern used
// throughout this project) never see another test's data.
public class SportsClubApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"SportsClubTestDb-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Removing only the DbContextOptions<AppDbContext> descriptor isn't
            // enough: AddDbContext also registers internal EF options-configuration
            // descriptors (keyed by AppDbContext) that get combined rather than
            // replaced, which would leave the original UseSqlite() call applied
            // alongside our UseInMemoryDatabase() call below and make EF Core
            // throw "two database providers registered". Strip everything tied to
            // AppDbContext's options first, then register it fresh with InMemory.
            var efDescriptors = services
                .Where(d => d.ServiceType.FullName?.Contains(nameof(AppDbContext)) == true)
                .ToList();
            foreach (var descriptor in efDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });

        // Keep test output focused on test results rather than the app's request logs.
        builder.ConfigureLogging(logging => logging.ClearProviders());
    }
}
