using Microsoft.AspNetCore.Mvc;
using SportsClubApi.Data;

namespace SportsClubApi.Controllers;

// Stub controller. Endpoints to be implemented once volunteer requirements are finalized.
[ApiController]
[Route("api/[controller]")]
public class VolunteersController : ControllerBase
{
    private readonly AppDbContext _context;

    public VolunteersController(AppDbContext context)
    {
        _context = context;
    }
}
