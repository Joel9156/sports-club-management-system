using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsClubApi.Data;
using SportsClubApi.Models;

namespace SportsClubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TeamsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/teams
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
    {
        return await _context.Teams.ToListAsync();
    }

    // GET: api/teams/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Team>> GetTeam(int id)
    {
        var team = await _context.Teams.FindAsync(id);

        if (team == null)
        {
            return NotFound();
        }

        return team;
    }

    // POST: api/teams
    [HttpPost]
    public async Task<ActionResult<Team>> CreateTeam(Team team)
    {
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
    }
}
