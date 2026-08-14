using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsClubApi.Data;
using SportsClubApi.Dtos;
using SportsClubApi.Models;
using SportsClubApi.Services;

namespace SportsClubApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    // POST: api/auth/register
    // Only Player/Volunteer accounts can be self-registered. Coach/Admin
    // accounts are seeded/created directly in the database by an administrator,
    // so a signup request is never trusted to grant elevated access.
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (request.Role != UserRole.Player && request.Role != UserRole.Volunteer)
        {
            return BadRequest(new { message = "Self-registration is only available for Player or Volunteer accounts." });
        }

        var emailTaken = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailTaken)
        {
            return Conflict(new { message = "An account with this email already exists." });
        }

        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
            Role = request.Role,
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(BuildAuthResponse(user));
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(BuildAuthResponse(user));
    }

    private AuthResponse BuildAuthResponse(User user) => new()
    {
        Token = _tokenService.CreateToken(user),
        Email = user.Email,
        FullName = user.FullName,
        Role = user.Role.ToString(),
    };
}
