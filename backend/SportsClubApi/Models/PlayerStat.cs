namespace SportsClubApi.Models;

// One player's numbers for one match. A row existing at all means the player
// took part in that match, so "matches played" is just a count of a player's
// rows - it isn't stored separately, which keeps it from drifting out of sync.
// Unique per (PlayerId, ScheduledEventId); see AppDbContext.
public class PlayerStat
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public Player? Player { get; set; }
    public int ScheduledEventId { get; set; }
    public ScheduledEvent? ScheduledEvent { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
}
