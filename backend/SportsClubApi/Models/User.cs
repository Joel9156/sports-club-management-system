namespace SportsClubApi.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;

    // Hashed with ASP.NET Core's PasswordHasher<User> (PBKDF2) - never store plaintext.
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }
    public string FullName { get; set; } = string.Empty;
}
