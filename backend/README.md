# Backend

ASP.NET Core Web API for the Community Sports Club Player and Volunteer Management System.

- **Project:** [SportsClubApi/](SportsClubApi/)
- **Framework:** ASP.NET Core (.NET 10), C#
- **Database:** SQLite via EF Core (`sportsclub.db`, created locally, not committed)
- **Auth:** JWT bearer tokens, issued via `/api/auth/login` and `/api/auth/register` (see below)

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

## Authentication

Auth is JWT-based (`Microsoft.AspNetCore.Authentication.JwtBearer`). Signing key/issuer/audience live in `appsettings.json` under `Jwt` — fine for this prototype, but in a real deployment the key should come from user-secrets/an environment variable instead of being committed.

- `POST /api/auth/register` — `{ email, password, fullName, role }`. `role` must be `"Player"` or `"Volunteer"` (self-registration for `"Coach"`/`"Admin"` is rejected with 400 — those accounts are provisioned directly in the database). Returns the same shape as login.
- `POST /api/auth/login` — `{ email, password }`. Returns `{ token, email, fullName, role }`.
- Send the token on subsequent requests as `Authorization: Bearer <token>`.
- The JWT's `role` claim is what `[Authorize(Roles = "...")]` checks. Current policy (route-level only, not per-record scoping):
  - `GET` endpoints on Players/Teams/Volunteers — any authenticated role
  - `POST /api/players`, `POST /api/volunteers` — `Admin` or the matching self-service role (`Player`, `Volunteer`)
  - `PUT`/`DELETE` and `POST /api/teams` — `Admin` only
- Enums (e.g. `role`) serialize as strings (`"Player"`, not `0`).

## Project layout

- `Models/` — domain entities (Player, Volunteer, Team, Attendance, User)
- `Dtos/` — request/response shapes not covered by an entity (currently auth only)
- `Services/TokenService.cs` — builds the JWT returned by `/api/auth`
- `Data/AppDbContext.cs` — EF Core database context
- `Controllers/` — API endpoints. PlayersController, TeamsController, and VolunteersController have CRUD (Teams has no PUT/DELETE yet); AttendanceController is still a stub.
- `Migrations/` — EF Core migrations

## Adding a migration

After changing a model or `AppDbContext`:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```
