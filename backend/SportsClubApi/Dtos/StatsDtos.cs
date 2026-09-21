namespace SportsClubApi.Dtos;

// One row of the player stats table: totals across all matches.
public class PlayerStatsSummary
{
    public int PlayerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Matches { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
}

// The club's record, worked out from match results.
public class TeamStatsSummary
{
    // Only matches that already have a score count as played.
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int UpcomingMatches { get; set; }
}
