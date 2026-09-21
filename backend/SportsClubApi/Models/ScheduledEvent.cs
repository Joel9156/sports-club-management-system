namespace SportsClubApi.Models;

// Stored as its string name ("Match"/"Training") so the database and API
// responses read clearly, matching how UserRole is handled.
public enum EventType
{
    Match,
    Training
}

// One entry on the club's schedule: either a match or a training session.
// Mt Eden FC is a single-team club (see docs/03-proposed-solution.md), so
// there's no team column - every event belongs to the club's one team.
public class ScheduledEvent
{
    public int Id { get; set; }
    public EventType Type { get; set; }
    public DateOnly Date { get; set; }
    public string Location { get; set; } = string.Empty;

    // Match-only fields. Opponent is null for training. The score stays null
    // until the match has been played, which is how "upcoming" and "played"
    // matches are told apart.
    public string? Opponent { get; set; }
    public int? GoalsFor { get; set; }
    public int? GoalsAgainst { get; set; }

    public string? Notes { get; set; }
}
