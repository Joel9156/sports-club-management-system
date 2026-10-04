using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SportsClubApi.Data;

namespace SportsClubApi.Middleware;

// Blocks every authenticated request from an account an Admin hasn't approved
// yet. The approval flag is read from the database on each request rather than
// from the token, so approving someone takes effect immediately without them
// logging in again. Anonymous requests (login, register) pass through untouched.
public class RequireApprovedAccountMiddleware
{
    private readonly RequestDelegate _next;

    public RequireApprovedAccountMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var subject = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!int.TryParse(subject, out var userId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var approved = await db.Users
                .Where(u => u.Id == userId)
                .Select(u => u.IsApproved)
                .FirstOrDefaultAsync();

            if (!approved)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message = "Your account is waiting for approval." });
                return;
            }
        }

        await _next(context);
    }
}
