using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsClubApi.Data;
using SportsClubApi.Dtos;
using SportsClubApi.Models;

namespace SportsClubApi.Controllers;

// In-app notifications only (see docs/03-proposed-solution.md - Notification).
// Notifications reach people two ways: automatically, when something
// notification-worthy happens (see AttendanceController), or manually, when a
// Coach/Admin sends one via POST send. Reading is scoped to "the caller's own
// records" rather than a role-wide check - a user can only ever see or modify
// their own notifications, regardless of role.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationsController(AppDbContext context)
    {
        _context = context;
    }

    private int? CurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return int.TryParse(idClaim, out var id) ? id : null;
    }

    // GET: api/notifications
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Notification>>> GetMyNotifications()
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    // POST: api/notifications/send
    // Coaches (and Admins) send a message to every account in the chosen
    // audience. Each recipient gets their own notification row, so they can
    // read it independently. The sender's name is put in front of the text so
    // recipients can tell who it's from.
    [HttpPost("send")]
    [Authorize(Roles = "Admin,Coach")]
    public async Task<ActionResult> Send(SendNotificationRequest request)
    {
        var message = request.Message.Trim();
        if (message.Length == 0)
        {
            return BadRequest(new { message = "Message can't be empty." });
        }

        var roles = request.Audience switch
        {
            NotificationAudience.Players => new[] { UserRole.Player },
            NotificationAudience.Volunteers => new[] { UserRole.Volunteer },
            _ => new[] { UserRole.Player, UserRole.Volunteer },
        };

        var recipients = (await _context.Users.ToListAsync())
            .Where(u => roles.Contains(u.Role))
            .ToList();

        var sender = User.FindFirstValue(ClaimTypes.Name) ?? "Club";
        foreach (var recipient in recipients)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = recipient.Id,
                Message = $"From {sender}: {message}",
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new { sent = recipients.Count });
    }

    // POST: api/notifications/5/read
    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var notification = await _context.Notifications.FindAsync(id);

        // Not found and "found but belongs to someone else" both return 404,
        // not 403 - otherwise the response would leak that a notification
        // with this id exists for another user.
        if (notification == null || notification.UserId != userId)
        {
            return NotFound();
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
