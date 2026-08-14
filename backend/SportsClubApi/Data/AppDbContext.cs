using Microsoft.EntityFrameworkCore;
using SportsClubApi.Models;

namespace SportsClubApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Volunteer> Volunteers => Set<Volunteer>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Player>()
            .HasOne(p => p.Team)
            .WithMany(t => t.Players)
            .HasForeignKey(p => p.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Attendance>()
            .HasOne(a => a.Player)
            .WithMany()
            .HasForeignKey(a => a.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Email is the login identifier, so it must be unique.
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Store the role as its string name (e.g. "Admin") rather than an int
        // so the database is readable directly, matching the "role" claim value.
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();
    }
}
