using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SportsClubApi.Models;

namespace SportsClubApi.Tests;

// Small builders shared by the schedule/stats tests so each test reads as
// "given a match and a player..." rather than repeating request bodies.
internal static class ScheduleTestHelpers
{
    // The API sends enums as their names ("Match"), so responses containing a
    // ScheduledEvent need the same converter the server is configured with.
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static async Task<ScheduledEvent> CreateMatchAsync(
        HttpClient adminClient,
        string date = "2026-10-05",
        string opponent = "Eden Rovers",
        int? goalsFor = null,
        int? goalsAgainst = null)
    {
        var response = await adminClient.PostAsJsonAsync("/api/events", new
        {
            type = "Match",
            date,
            location = "Lloyd Elsmore Park",
            opponent,
            goalsFor,
            goalsAgainst,
        });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ScheduledEvent>(Json))!;
    }

    public static async Task<ScheduledEvent> CreateTrainingAsync(HttpClient adminClient, string date = "2026-10-02")
    {
        var response = await adminClient.PostAsJsonAsync("/api/events", new
        {
            type = "Training",
            date,
            location = "Nixon Park",
        });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ScheduledEvent>(Json))!;
    }

    public static async Task<Player> CreatePlayerAsync(HttpClient adminClient, string email)
    {
        var response = await adminClient.PostAsJsonAsync("/api/players", new
        {
            fullName = $"Player {email}",
            dateOfBirth = "2000-01-01",
            email,
            phone = "555-0000",
            registrationDate = "2026-01-01",
            isActive = true,
        });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<Player>())!;
    }

    public static Task<HttpResponseMessage> RecordStatAsync(
        HttpClient client, int playerId, int eventId, int goals = 0, int assists = 0) =>
        client.PostAsJsonAsync("/api/playerstats", new
        {
            playerId,
            scheduledEventId = eventId,
            goals,
            assists,
        });
}
