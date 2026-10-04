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

            var status = await db.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.IsApproved, u.IsRejected })
                .FirstOrDefaultAsync();

            if (status == null || !status.IsApproved)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                var message = status?.IsRejected == true
                    ? "Your account registration was not approved. Please contact the club."
                    : "Your account is waiting for approval.";
                await context.Response.WriteAsJsonAsync(new { message });
                return;
            }
        }

        await _next(context);
    }
}
