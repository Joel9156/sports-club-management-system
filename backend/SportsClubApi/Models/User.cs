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
    // their roster/volunteer record (and access to the rest of the app) once
    // an Admin approves them - see AuthController.Register and the upcoming
    // approval endpoint. Accounts created any other way (DbSeeder, an Admin
    // creating a Coach account) default to already approved.
    public bool IsApproved { get; set; } = true;
}
