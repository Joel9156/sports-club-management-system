using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsClubApi.Data;
using SportsClubApi.Models;

namespace SportsClubApi.Controllers;

// The club's schedule of matches and training sessions. Every role can read
// it; only Admins manage it.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EventsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/events?type=Match
    // Soonest first; the frontend splits this into upcoming and past.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduledEvent>>> GetEvents([FromQuery] EventType? type)
    {
        var query = _context.ScheduledEvents.AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(e => e.Type == type.Value);
        }

        return await query.OrderBy(e => e.Date).ThenBy(e => e.Id).ToListAsync();
    }

    // GET: api/events/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ScheduledEvent>> GetEvent(int id)
    {
        var scheduledEvent = await _context.ScheduledEvents.FindAsync(id);

        if (scheduledEvent == null)
        {
            return NotFound();
        }

        return scheduledEvent;
    }

    // POST: api/events
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ScheduledEvent>> CreateEvent(ScheduledEvent scheduledEvent)
    {
        var problem = Validate(scheduledEvent);
        if (problem != null)
        {
            return BadRequest(new { message = problem });
        }

        _context.ScheduledEvents.Add(scheduledEvent);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEvent), new { id = scheduledEvent.Id }, scheduledEvent);
    }

    // PUT: api/events/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEvent(int id, ScheduledEvent scheduledEvent)
    {
        if (id != scheduledEvent.Id)
        {
            return BadRequest();
        }

        var problem = Validate(scheduledEvent);
        if (problem != null)
        {
            return BadRequest(new { message = problem });
        }

        _context.Entry(scheduledEvent).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.ScheduledEvents.AnyAsync(e => e.Id == id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    // DELETE: api/events/5
    // Also removes any player stats recorded against this match (cascade).
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var scheduledEvent = await _context.ScheduledEvents.FindAsync(id);

        if (scheduledEvent == null)
        {
            return NotFound();
        }

        _context.ScheduledEvents.Remove(scheduledEvent);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Returns a message describing the first problem, or null if it's fine.
    // Keeps matches and training from carrying each other's fields, and stops
    // a half-entered score (only one side) from making the team record wrong.
    private static string? Validate(ScheduledEvent e)
    {
        if (string.IsNullOrWhiteSpace(e.Location))
        {
            return "Location is required.";
        }

        if (e.Type == EventType.Training)
        {
            if (!string.IsNullOrWhiteSpace(e.Opponent) || e.GoalsFor != null || e.GoalsAgainst != null)
            {
                return "Training sessions can't have an opponent or a score.";
            }

            return null;
        }

        if (string.IsNullOrWhiteSpace(e.Opponent))
        {
            return "A match needs an opponent.";
        }

        if ((e.GoalsFor == null) != (e.GoalsAgainst == null))
        {
            return "Enter both goals for and goals against, or neither.";
        }

        if (e.GoalsFor < 0 || e.GoalsAgainst < 0)
        {
            return "Goals can't be negative.";
        }

        return null;
    }
}
