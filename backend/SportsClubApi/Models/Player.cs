namespace SportsClubApi.Models;

public class Player
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int? TeamId { get; set; }
    public Team? Team { get; set; }
    public DateOnly RegistrationDate { get; set; }
    public bool IsActive { get; set; } = true;
}
