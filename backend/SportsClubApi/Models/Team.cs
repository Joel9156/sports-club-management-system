namespace SportsClubApi.Models;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AgeGroup { get; set; } = string.Empty;
    public string CoachName { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;

    public ICollection<Player> Players { get; set; } = new List<Player>();
}
