using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsClubApi.Data;
using SportsClubApi.Models;

namespace SportsClubApi.Controllers;

// Was a stub with no endpoints; implemented here (mirroring PlayersController)
// to support the Coach attendance-marking page. Only GET/POST for now - no
// PUT/DELETE - so a session that's already been recorded can't be edited yet.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly AppDbContext _context;

    public AttendanceController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/attendance?teamId=1&date=2026-08-20&playerId=5
    // All filters are optional. teamId filters via the player's team since
    // Attendance itself only stores PlayerId, not TeamId directly - EF
    // translates the Player.TeamId comparison into a SQL join even without
    // an explicit Include.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Attendance>>> GetAttendance(
        [FromQuery] int? teamId,
        [FromQuery] DateOnly? date,
        [FromQuery] int? playerId)
    {
        var query = _context.Attendances.AsQueryable();

        if (teamId.HasValue)
        {
            query = query.Where(a => a.Player != null && a.Player.TeamId == teamId.Value);
        }

        if (date.HasValue)
        {
            query = query.Where(a => a.SessionDate == date.Value);
        }

        if (playerId.HasValue)
        {
            query = query.Where(a => a.PlayerId == playerId.Value);
        }

        return await query.ToListAsync();
    }

    // GET: api/attendance/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Attendance>> GetAttendanceRecord(int id)
    {
        var record = await _context.Attendances.FindAsync(id);

        if (record == null)
        {
            return NotFound();
        }

        return record;
    }

    // POST: api/attendance
    // Coaches mark attendance for their own sessions; Admins can too.
    [HttpPost]
    [Authorize(Roles = "Admin,Coach")]
    public async Task<ActionResult<Attendance>> RecordAttendance(Attendance attendance)
    {
        var playerExists = await _context.Players.AnyAsync(p => p.Id == attendance.PlayerId);
        if (!playerExists)
        {
            return BadRequest(new { message = "Player does not exist." });
        }

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAttendanceRecord), new { id = attendance.Id }, attendance);
    }
}
