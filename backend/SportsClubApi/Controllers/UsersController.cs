using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsClubApi.Data;
using SportsClubApi.Dtos;
using SportsClubApi.Models;

namespace SportsClubApi.Controllers;

// Approving a self-registered Player/Volunteer account, Admin only. This is
// the other half of AuthController.Register setting IsApproved = false: an
// Admin reviews the account here, and approving it is what actually creates
// the Player/Volunteer roster record (see DEF-02 in 09-defect-register.md).
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/users/pending
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<PendingAccountSummary>>> GetPendingAccounts()
    {
        return await _context.Users
            .Where(u => !u.IsApproved)
            .OrderBy(u => u.FullName)
            .Select(u => new PendingAccountSummary { Id = u.Id, Email = u.Email, FullName = u.FullName, Role = u.Role })
            .ToListAsync();
    }

    // POST: api/users/5/approve
    // Marks the account approved and, if a matching Player/Volunteer record
    // doesn't already exist for its email, creates one from the account's
    // name/email (mirroring what self-registration used to do immediately).
    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> ApproveAccount(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (user.IsApproved)
        {
            return NoContent();
        }

        user.IsApproved = true;

        if (user.Role == UserRole.Player)
        {
            var playerExists = await _context.Players.AnyAsync(p => p.Email == user.Email);
            if (!playerExists)
            {
                _context.Players.Add(new Player
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    RegistrationDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    IsActive = true,
                });
            }
        }
        else if (user.Role == UserRole.Volunteer)
        {
            var volunteerExists = await _context.Volunteers.AnyAsync(v => v.Email == user.Email);
            if (!volunteerExists)
            {
                _context.Volunteers.Add(new Volunteer
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    IsActive = true,
                });
            }
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
