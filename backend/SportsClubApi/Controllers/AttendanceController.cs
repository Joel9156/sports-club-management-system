using Microsoft.AspNetCore.Mvc;
using SportsClubApi.Data;

namespace SportsClubApi.Controllers;

// Stub controller. Endpoints to be implemented once attendance requirements are finalized.
[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly AppDbContext _context;

    public AttendanceController(AppDbContext context)
    {
        _context = context;
    }
}
