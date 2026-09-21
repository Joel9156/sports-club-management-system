using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsClubApi.Data;
using SportsClubApi.Models;

namespace SportsClubApi.Controllers;

// Per-match goals/assists for a player. Every role can read; Coaches (and
// Admins) enter and correct them - that includes removing a row entered by
// mistake, since ticking a player off a match's line-up deletes their row.
// (Totals and the team record are computed in StatsController from these
// rows and the match results.)
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlayerStatsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PlayerStatsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/playerstats?playerId=1&eventId=2 (both filters optional)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerStat>>> GetPlayerStats(
        [FromQuery] int? playerId,
        [FromQuery] int? eventId)
    {
        var query = _context.PlayerStats.AsQueryable();

        if (playerId.HasValue)
        {
            query = query.Where(s => s.PlayerId == playerId.Value);
        }

        if (eventId.HasValue)
        {
            query = query.Where(s => s.ScheduledEventId == eventId.Value);
        }

        return await query.ToListAsync();
    }

    // POST: api/playerstats
    // Recording a row means the player took part in that match.
    [HttpPost]
    [Authorize(Roles = "Admin,Coach")]
    public async Task<ActionResult<PlayerStat>> CreatePlayerStat(PlayerStat stat)
    {
        if (stat.Goals < 0 || stat.Assists < 0)
        {
            return BadRequest(new { message = "Goals and assists can't be negative." });
        }

        if (!await _context.Players.AnyAsync(p => p.Id == stat.PlayerId))
        {
            return BadRequest(new { message = "Player does not exist." });
        }

        var scheduledEvent = await _context.ScheduledEvents.FindAsync(stat.ScheduledEventId);
        if (scheduledEvent == null)
        {
            return BadRequest(new { message = "Match does not exist." });
        }

        if (scheduledEvent.Type != EventType.Match)
        {
            return BadRequest(new { message = "Stats can only be recorded against a match, not training." });
        }

        var alreadyRecorded = await _context.PlayerStats.AnyAsync(s =>
            s.PlayerId == stat.PlayerId && s.ScheduledEventId == stat.ScheduledEventId);
        if (alreadyRecorded)
        {
            return Conflict(new { message = "Stats are already recorded for this player in this match." });
        }

        // Don't trust nested objects from the request body.
        stat.Player = null;
        stat.ScheduledEvent = null;

        _context.PlayerStats.Add(stat);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPlayerStats), new { playerId = stat.PlayerId, eventId = stat.ScheduledEventId }, stat);
    }

    // PUT: api/playerstats/5 - corrects goals/assists only; which player and
    // which match a row belongs to can't be changed (delete and re-add instead).
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Coach")]
    public async Task<IActionResult> UpdatePlayerStat(int id, PlayerStat update)
    {
        if (id != update.Id)
        {
            return BadRequest();
        }

        if (update.Goals < 0 || update.Assists < 0)
        {
            return BadRequest(new { message = "Goals and assists can't be negative." });
        }

        var stat = await _context.PlayerStats.FindAsync(id);
        if (stat == null)
        {
            return NotFound();
        }

        stat.Goals = update.Goals;
        stat.Assists = update.Assists;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/playerstats/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Coach")]
    public async Task<IActionResult> DeletePlayerStat(int id)
    {
        var stat = await _context.PlayerStats.FindAsync(id);
        if (stat == null)
        {
            return NotFound();
        }

        _context.PlayerStats.Remove(stat);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
