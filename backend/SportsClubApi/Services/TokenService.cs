using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SportsClubApi.Models;

namespace SportsClubApi.Services;

// Builds the JWT handed back to the frontend on login/register. The frontend
// stores this in memory and sends it as "Authorization: Bearer <token>" on
// every request; ASP.NET Core's JWT bearer middleware (see Program.cs)
// validates it and populates HttpContext.User from its claims.
public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // ClaimTypes.Role is what [Authorize(Roles = "...")] checks against,
        // so the role claim must use that exact claim type to be recognised.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString()),
        };

        var expiryMinutes = double.Parse(jwtSection["ExpiryMinutes"] ?? "120");

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
