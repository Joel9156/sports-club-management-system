namespace SportsClubApi.Models;

// The four user roles defined in the proposed solution (docs/03-proposed-solution.md).
// Stored as a string in the database and as the "role" claim in the JWT so it reads
// clearly in tokens/logs instead of an opaque integer.
public enum UserRole
{
    Player,
    Volunteer,
    Coach,
    Admin
}
