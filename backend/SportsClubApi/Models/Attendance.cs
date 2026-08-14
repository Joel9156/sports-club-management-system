namespace SportsClubApi.Models;

public class Attendance
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public Player? Player { get; set; }
    public DateOnly SessionDate { get; set; }
    public bool IsPresent { get; set; }
    public string? Notes { get; set; }
}
