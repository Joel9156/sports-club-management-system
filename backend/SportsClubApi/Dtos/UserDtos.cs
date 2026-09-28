using SportsClubApi.Models;

namespace SportsClubApi.Dtos;

// A self-registered account waiting for an Admin to approve it.
public class PendingAccountSummary
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
