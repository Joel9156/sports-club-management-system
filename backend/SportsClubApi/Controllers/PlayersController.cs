using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsClubApi.Data;
using SportsClubApi.Models;

namespace SportsClubApi.Controllers;

// Route-level protection only for now: any authenticated role can read player
// data, and writes are restricted by role. Scoping reads/writes down to "a
// player's own record" or "a coach's own team" is deferred - see
// docs/03-proposed-solution.md and the Step 2 discussion for context.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlayersController : ControllerBase
{
    private readonly AppDbContext _context;

    public PlayersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/players
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
    {
        return await _context.Players.ToListAsync();
    }

    // GET: api/players/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Player>> GetPlayer(int id)
    {
        var player = await _context.Players.FindAsync(id);

        if (player == null)
        {
            return NotFound();
        }

        return player;
    }

    // POST: api/players
    // Admins register players on behalf of the club; Players can also submit
    // their own registration (docs/03-proposed-solution.md - Player Registration).
    //
    // Entry-point duplicate check: the proposed solution promises that
    // "entry-point validation removes duplicate player records" - without
    // this, a Player role could submit this form repeatedly and create an
    // unlimited number of records, reproducing the exact duplicate-records
    // problem (Problem #1) this project exists to solve.
    [HttpPost]
    [Authorize(Roles = "Admin,Player")]
    public async Task<ActionResult<Player>> CreatePlayer(Player player)
    {
        var emailTaken = await _context.Players.AnyAsync(p => p.Email == player.Email);
        if (emailTaken)
        {
            return Conflict(new { message = "A player with this email is already registered." });
        }

        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
    }

    // PUT: api/players/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePlayer(int id, Player player)
    {
        if (id != player.Id)
        {
            return BadRequest();
        }

        _context.Entry(player).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Players.AnyAsync(p => p.Id == id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    // DELETE: api/players/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePlayer(int id)
    {
        var player = await _context.Players.FindAsync(id);

        if (player == null)
        {
            return NotFound();
        }

        _context.Players.Remove(player);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
