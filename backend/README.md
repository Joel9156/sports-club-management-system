# Backend

ASP.NET Core Web API for the Community Sports Club Player and Volunteer Management System.

- **Project:** [SportsClubApi/](SportsClubApi/)
- **Framework:** ASP.NET Core (.NET 10), C#
- **Database:** SQLite via EF Core (`sportsclub.db`, created locally, not committed)
- **Auth:** none yet

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) matching the version in `SportsClubApi.csproj`
- EF Core CLI tools: `dotnet tool install --global dotnet-ef` (if not already installed)

## Setup

From `backend/SportsClubApi/`:

```bash
dotnet restore
dotnet ef database update   # creates sportsclub.db and applies migrations
dotnet run
```

The API listens on the URLs in `Properties/launchSettings.json` (default `http://localhost:5235`).

## Swagger / OpenAPI

With the app running in the Development environment, open `/swagger` (e.g. `http://localhost:5235/swagger`) to browse and try the API.

## CORS

The API allows requests from `http://localhost:5173` (the default Vite dev server port for the React frontend).

## Project layout

- `Models/` — domain entities (Player, Volunteer, Team, Attendance)
- `Data/AppDbContext.cs` — EF Core database context
- `Controllers/` — API endpoints (PlayersController and TeamsController have full CRUD; VolunteersController and AttendanceController are stubs)
- `Migrations/` — EF Core migrations

## Adding a migration

After changing a model or `AppDbContext`:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```
