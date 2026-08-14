# Frontend

React (Vite, JavaScript) frontend for the Community Sports Club Player and Volunteer Management System.

- **Framework:** React 19 + Vite
- **Routing:** react-router-dom
- **HTTP client:** axios
- **Backend API:** ASP.NET Core Web API (see [`../backend`](../backend)), expected at `http://localhost:5235/api`

## Setup

From `frontend/`:

```bash
npm install
npm run dev
```

The dev server runs at `http://localhost:5173` (the origin the backend's CORS policy allows).

Make sure the backend is running (`dotnet run` from `backend/SportsClubApi/`) so API calls succeed.

## Environment variables

Copy `.env.example` to `.env` and adjust if needed:

```
VITE_API_BASE_URL=http://localhost:5235/api
```

## Project layout

- `src/pages/` — top-level screens (Players, Teams, Volunteers, Attendance, Dashboard, ...)
- `src/components/` — reusable UI (forms, tables, layout)
- `src/api/` — axios instance and per-resource API functions
- `src/context/` — shared app state (e.g. auth/role context)

## Notes

- The backend currently has full CRUD for Players and Teams. Volunteers and Attendance endpoints are stubs, so those screens may need mock data until the backend catches up.
- There is no authentication yet; role-based access is mocked on the frontend for now.
