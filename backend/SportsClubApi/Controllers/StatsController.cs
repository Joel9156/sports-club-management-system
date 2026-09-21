using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsClubApi.Data;
using SportsClubApi.Dtos;
using SportsClubApi.Models;

namespace SportsClubApi.Controllers;

// Read-only, computed on request from ScheduledEvents and PlayerStats so the
// totals can never disagree with the underlying rows. Open to every role.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StatsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/stats/players
    // Every player appears, including those with no matches yet. Sorted by
    // goals, then assists, then name so the top scorers come first.
    [HttpGet("players")]
    public async Task<ActionResult<IEnumerable<PlayerStatsSummary>>> GetPlayerTotals()
    {
        var players = await _context.Players.ToListAsync();
        var stats = await _context.PlayerStats.ToListAsync();

        return players
            .Select(p =>
            {
                var mine = stats.Where(s => s.PlayerId == p.Id).ToList();
                return new PlayerStatsSummary
                {
                    PlayerId = p.Id,
                    FullName = p.FullName,
                    Matches = mine.Count,
                    Goals = mine.Sum(s => s.Goals),
                    Assists = mine.Sum(s => s.Assists),
                };
            })
            .OrderByDescending(s => s.Goals)
            .ThenByDescending(s => s.Assists)
            .ThenBy(s => s.FullName)
            .ToList();
    }

    // GET: api/stats/team
    [HttpGet("team")]
    public async Task<ActionResult<TeamStatsSummary>> GetTeamStats()
    {
        var matches = await _context.ScheduledEvents
            .Where(e => e.Type == EventType.Match)
            .ToListAsync();

        var played = matches.Where(m => m.GoalsFor != null && m.GoalsAgainst != null).ToList();

        return new TeamStatsSummary
        {
            MatchesPlayed = played.Count,
            Wins = played.Count(m => m.GoalsFor > m.GoalsAgainst),
            Draws = played.Count(m => m.GoalsFor == m.GoalsAgainst),
            Losses = played.Count(m => m.GoalsFor < m.GoalsAgainst),
            GoalsFor = played.Sum(m => m.GoalsFor!.Value),
            GoalsAgainst = played.Sum(m => m.GoalsAgainst!.Value),
            UpcomingMatches = matches.Count - played.Count,
        };
    }
}
