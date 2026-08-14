using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsClubApi.Data;
using SportsClubApi.Models;

namespace SportsClubApi.Controllers;

// Was a stub with no endpoints; implemented here (mirroring PlayersController)
// because the frontend's volunteer profile form needs somewhere to submit to.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VolunteersController : ControllerBase
{
    private readonly AppDbContext _context;

    public VolunteersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/volunteers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Volunteer>>> GetVolunteers()
    {
        return await _context.Volunteers.ToListAsync();
    }

    // GET: api/volunteers/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Volunteer>> GetVolunteer(int id)
    {
        var volunteer = await _context.Volunteers.FindAsync(id);

        if (volunteer == null)
        {
            return NotFound();
        }

        return volunteer;
    }

    // POST: api/volunteers
    // Admins register volunteers on behalf of the club; Volunteers can also
    // submit their own profile (docs/03-proposed-solution.md - Volunteer Management).
    [HttpPost]
    [Authorize(Roles = "Admin,Volunteer")]
    public async Task<ActionResult<Volunteer>> CreateVolunteer(Volunteer volunteer)
    {
        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetVolunteer), new { id = volunteer.Id }, volunteer);
    }

    // PUT: api/volunteers/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateVolunteer(int id, Volunteer volunteer)
    {
        if (id != volunteer.Id)
        {
            return BadRequest();
        }

        _context.Entry(volunteer).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Volunteers.AnyAsync(v => v.Id == id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    // DELETE: api/volunteers/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteVolunteer(int id)
    {
        var volunteer = await _context.Volunteers.FindAsync(id);

        if (volunteer == null)
        {
            return NotFound();
        }

        _context.Volunteers.Remove(volunteer);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
