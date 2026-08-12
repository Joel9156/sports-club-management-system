namespace SportsClubApi.Models;

public class Volunteer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Availability { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
