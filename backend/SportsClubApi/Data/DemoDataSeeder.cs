using Microsoft.EntityFrameworkCore;
using SportsClubApi.Models;

namespace SportsClubApi.Data;

// Sample roster, schedule and stats so a fresh checkout has something to show
// on the Players, Schedule and Stats pages. Idempotent: players are added only
// if their email is missing, and the schedule and stats only if the schedule
// is empty, so it never touches data someone has already entered. Program.cs
// runs it against a real database only, so the automated tests (InMemory) keep
// starting from an empty one.
public static class DemoDataSeeder
{
    private static readonly (string Name, string Dob, string Email, string Phone)[] Roster =
    {
        ("Liam Walker", "1998-03-12", "liam.walker@mtedenfc.test", "021-555-0110"),
        ("Noah Tane", "1996-07-25", "noah.tane@mtedenfc.test", "021-555-0111"),
        ("Oliver Singh", "2000-11-02", "oliver.singh@mtedenfc.test", "021-555-0112"),
        ("Jack Patel", "1999-01-19", "jack.patel@mtedenfc.test", "021-555-0113"),
        ("Mason Kim", "2001-05-30", "mason.kim@mtedenfc.test", "021-555-0114"),
        ("Ethan Brown", "1997-09-08", "ethan.brown@mtedenfc.test", "021-555-0115"),
        ("Lucas Wilson", "2002-02-14", "lucas.wilson@mtedenfc.test", "021-555-0116"),
        ("Aiden Park", "1995-12-21", "aiden.park@mtedenfc.test", "021-555-0117"),
        ("Caleb Hohepa", "2000-06-17", "caleb.hohepa@mtedenfc.test", "021-555-0118"),
        ("Ryan Nguyen", "1998-10-05", "ryan.nguyen@mtedenfc.test", "021-555-0119"),
        ("Daniel Fraser", "2003-04-23", "daniel.fraser@mtedenfc.test", "021-555-0120"),
    };

    // (date, opponent, goals for, goals against); null score = not played yet.
    private static readonly (string Date, string Opponent, int? For, int? Against)[] Matches =
    {
        ("2026-09-06", "Eden Rovers", 3, 1),
        ("2026-09-13", "Grey Lynn", 2, 2),
        ("2026-09-20", "Ponsonby United", 0, 1),
        ("2026-09-27", "Kingsland FC", null, null),
        ("2026-10-04", "Sandringham", null, null),
    };

    // Per played match: (roster index, goals, assists) for every player.
    private static readonly (int Player, int Goals, int Assists)[][] Lines =
    {
        new[] { (0, 1, 0), (1, 2, 1), (2, 0, 1), (3, 0, 0), (4, 0, 0), (5, 0, 0), (6, 0, 1), (7, 0, 0), (8, 0, 0), (9, 0, 0), (10, 0, 0) },
        new[] { (0, 0, 1), (1, 1, 0), (2, 1, 0), (3, 0, 0), (4, 0, 0), (5, 0, 1), (6, 0, 0), (7, 0, 0), (8, 0, 0), (9, 0, 0), (10, 0, 0) },
        new[] { (0, 0, 0), (1, 0, 0), (2, 0, 0), (3, 0, 0), (4, 0, 0), (5, 0, 0), (6, 0, 0), (7, 0, 0), (8, 0, 0), (9, 0, 0), (10, 0, 0) },
    };

    public static async Task SeedAsync(AppDbContext context)
    {
        var registered = DateOnly.Parse("2026-09-01");

        foreach (var r in Roster)
        {
            if (!await context.Players.AnyAsync(p => p.Email == r.Email))
            {
                context.Players.Add(new Player
                {
                    FullName = r.Name,
                    DateOfBirth = DateOnly.Parse(r.Dob),
                    Email = r.Email,
                    Phone = r.Phone,
                    RegistrationDate = registered,
                    IsActive = true,
                });
            }
        }

        await context.SaveChangesAsync();

        if (await context.ScheduledEvents.AnyAsync())
        {
            return;
        }

        foreach (var date in new[] { "2026-09-16", "2026-09-24", "2026-10-01" })
        {
            context.ScheduledEvents.Add(new ScheduledEvent
            {
                Type = EventType.Training,
                Date = DateOnly.Parse(date),
                Location = "Nixon Park",
            });
        }

        var matches = Matches.Select(m => new ScheduledEvent
        {
            Type = EventType.Match,
            Date = DateOnly.Parse(m.Date),
            Location = "Lloyd Elsmore Park",
            Opponent = m.Opponent,
            GoalsFor = m.For,
            GoalsAgainst = m.Against,
        }).ToList();
        context.ScheduledEvents.AddRange(matches);
        await context.SaveChangesAsync();

        var emails = Roster.Select(r => r.Email).ToList();
        var players = await context.Players.Where(p => emails.Contains(p.Email)).ToListAsync();

        for (var m = 0; m < Lines.Length; m++)
        {
            foreach (var (index, goals, assists) in Lines[m])
            {
                var player = players.First(p => p.Email == Roster[index].Email);
                context.PlayerStats.Add(new PlayerStat
                {
                    PlayerId = player.Id,
                    ScheduledEventId = matches[m].Id,
                    Goals = goals,
                    Assists = assists,
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
