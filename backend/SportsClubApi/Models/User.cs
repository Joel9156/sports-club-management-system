namespace SportsClubApi.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;

    // Hashed with ASP.NET Core's PasswordHasher<User> (PBKDF2) - never store plaintext.
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }
    public string FullName { get; set; } = string.Empty;

    // Self-registered Player/Volunteer accounts start unapproved and only get
    // their roster/volunteer record once an Admin approves them - see
    // UsersController. Accounts created any other way (DbSeeder, an Admin
    // creating a Coach account) default to already approved. Unapproved
    // accounts are blocked from the API by RequireApprovedAccountMiddleware.
    public bool IsApproved { get; set; } = true;

    // Set when an Admin rejects an unapproved account. A rejected account can
    // still be approved later, which clears this flag.
    public bool IsRejected { get; set; }
}
