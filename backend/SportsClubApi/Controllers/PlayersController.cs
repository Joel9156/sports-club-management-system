using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsClubApi.Data;
using SportsClubApi.Models;

namespace SportsClubApi.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    [HttpPost]
    public async Task<ActionResult<Player>> CreatePlayer(Player player)
    {
        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player);
    }

    // PUT: api/players/5
    [HttpPut("{id:int}")]
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
