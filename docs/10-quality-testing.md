# Quality Testing

## 1. Purpose and Scope

This document records the non-functional quality testing carried out on the Mt Eden FC Player and Volunteer Management System, covering the two areas defined in `04-test-strategy.md` Section 5:

- **Security testing (5.1):** whether users must authenticate, and whether they can only reach the functions their role allows. This supports requirement 2.1 (Security) in `docs/02-requirements.md`.
- **Performance testing (5.2):** whether pages and dashboards respond within an acceptable time. This supports requirements 2.2 (Performance) and 2.5 (Scalability).

Functional test cases are recorded separately in `05-test-cases.md` and defects in `09-defect-register.md`. This document refers to those records rather than repeating them.

## 2. Security Testing

### 2.1 Approach

Security behaviour is verified at the API level, because the backend is the only place access control is actually enforced (the frontend's route protection only improves usability and can be bypassed by calling the API directly). The xUnit project `SportsClubApi.Tests` starts the real application with `WebApplicationFactory`, logs in through the real `/api/auth/login` endpoint to obtain a genuine JWT, and then calls protected endpoints with and without that token. Tokens are never forged in tests. Each test runs against its own isolated in-memory database.

### 2.2 Role-Based Access Control

Access is controlled by `[Authorize]` attributes on the API controllers, using the `role` claim in the JWT. The policy in the current code is:

| Endpoint | Allowed roles |
|---|---|
| `GET` on Players, Teams, Volunteers, Attendance | Any authenticated user |
| `POST /api/players` | Admin, Player |
| `PUT` / `DELETE /api/players/{id}` | Admin only |
| `POST /api/volunteers` | Admin, Volunteer |
| `PUT` / `DELETE /api/volunteers/{id}` | Admin only |
| `POST /api/teams` | Admin only |
| `POST /api/attendance` | Admin, Coach |
| `POST /api/auth/register` | Public, but only the Player and Volunteer roles can be requested |

**Automated test evidence**

| Test case | Automated test | What it verifies | Result |
|---|---|---|---|
| TC-07 | `AuthorizationTests.DeletePlayer_AsPlayerRole_ReturnsForbidden` | A Player cannot call an Admin-only endpoint (`DELETE /api/players/{id}`); 403 Forbidden is returned | Pass |
| TC-17 | `AttendanceControllerTests.RecordAttendance_AsPlayerRole_ReturnsForbidden` | A Player cannot record attendance (Admin/Coach only); 403 Forbidden is returned | Pass |
| TC-14 | `AuthControllerTests.Register_WithAdminRole_ReturnsBadRequest` | Self-registration cannot be used to obtain an Admin account; 400 Bad Request is returned and no account is created | Pass |
| TC-21 | `NotificationsControllerTests.MarkAsRead_ForAnotherUsersNotification_ReturnsNotFound` | A user cannot act on another user's notification; 404 Not Found is returned | Pass |

**Coverage gap.** The test objective was to verify that Player, Volunteer and Coach roles cannot reach Admin-only endpoints. Only the **Player** role is currently exercised against an Admin-only endpoint (TC-07). The Volunteer and Coach roles are not tested against `PUT`/`DELETE` on players or volunteers, or against `POST /api/teams`. These endpoints are protected by the same `[Authorize(Roles = "Admin")]` mechanism, so the risk is considered low, but it has not been demonstrated by a test. Adding one test per remaining role and per Admin-only endpoint is recommended (see Section 4).

### 2.3 Authentication

| Test case | Automated test | What it verifies | Result |
|---|---|---|---|
| TC-10 | `AttendanceControllerTests.RecordAttendance_WithoutAuth_ReturnsUnauthorized` | A request with no bearer token is rejected with 401 Unauthorized | Pass |
| TC-02 | `AuthControllerTests.Login_WithWrongPassword_ReturnsUnauthorized` | A valid email with the wrong password is rejected with 401 | Pass |
| TC-02 | `AuthControllerTests.Login_WithUnknownEmail_ReturnsUnauthorized` | An unknown email is rejected with 401, using the same message as a wrong password so account existence is not revealed | Pass |
| TC-01 | `AuthControllerTests.Login_WithValidCredentials_ReturnsOkWithToken` | A valid login returns a JWT | Pass |

Supporting design points, verified by code review rather than by automated test:

- Passwords are hashed with ASP.NET Core's `PasswordHasher<User>` (PBKDF2) and never stored in plain text.
- Tokens are validated for issuer, audience, signature and lifetime (120 minutes, one minute clock skew).
- `AttendanceController`, `PlayersController`, `TeamsController`, `VolunteersController` and `NotificationsController` are all marked `[Authorize]` at class level. However, the 401 behaviour is only covered by an automated test on the attendance endpoint (TC-10); the other controllers rely on the same attribute but are not individually tested.
- On the frontend, the login (including the JWT) is kept in `sessionStorage`, so it persists across a page refresh but is cleared when the browser tab is closed. A saved token that has expired is discarded on load. `localStorage` is not used, so the login does not outlive the tab. The trade-off is that a token in browser storage can be read by any script running on the page, so it would be exposed if the application ever had a cross-site scripting (XSS) vulnerability.

### 2.4 Known Security Risks

These are risks that were identified during development and are accepted for the prototype. They should be resolved before any real deployment.

| ID | Risk | Severity (if deployed) | Current state | Recommended action |
|---|---|---|---|---|
| SEC-01 | **JWT signing key stored in `appsettings.json`.** The file is tracked in Git, so the key is visible to anyone with access to the repository and remains in the commit history. Anyone who holds the key can mint a valid token for any role, including Admin, which would bypass all role-based access control. | High | Accepted for the prototype. Noted in `backend/README.md`. | Move the key to user-secrets (development) or an environment variable / secret store (production), and generate a new key, because the current one is in the history. |
| SEC-02 | **Fixed default credentials.** `DbSeeder` creates an Admin and a Coach account with a known password on every startup, in every environment. | High | Intended for local demo use only, and documented in `backend/README.md`. | Seed only in the Development environment, or read the initial password from configuration. |
| SEC-03 | **No per-record scoping.** Access control is at route level only. Any authenticated user, including a Player or Volunteer, can read the full lists of players (including date of birth and phone number), volunteers and attendance. This only partly meets requirement 2.1. | Medium | Deferred deliberately (see the comment in `PlayersController.cs`). | Scope reads so a Player sees only their own record and a Coach only relevant records. |

### 2.5 Security Testing Summary

All automated security tests pass (49 of 49 tests in the backend suite passed on the last full run). Authentication is enforced, invalid logins are rejected, and the tested role restrictions hold. Testing is incomplete in two respects: the Volunteer and Coach roles are not tested against Admin-only endpoints, and the three risks in Section 2.4 remain open. No penetration testing or dependency vulnerability scanning was carried out.

## 3. Performance Testing

### 3.1 Approach and Tools

Performance was assessed informally during development, using:

- **Browser developer tools** (Network tab) to inspect request timing and response size for the API calls each page makes.
- **Manual observation** of how quickly each page rendered after navigation.

No load-testing tool was used, and no large dataset was generated. The automated xUnit suite verifies behaviour, not response time, so it provides no performance evidence.

### 3.2 Observations

Load times were measured from the browser Network tab against the locally running application, using the small sample data set that the application is seeded with for development.

| Page | Load time |
|---|---|
| Dashboard | 6 ms |
| Players | 9 ms |
| Volunteers | 4 ms |
| Attendance | 6 ms |
| History | 6 ms |
| Schedule | 7 ms |
| Stats | 5 ms |
| Send Notice | 7 ms |
| Notifications | 8 ms |

**Note:** all pages loaded in under 10 ms with this small sample data. These results show that the pages respond quickly at prototype scale, but they say nothing about behaviour at larger volumes. Each figure comes from a single observation on a local machine with no network latency, so they should be read as indicative rather than as a benchmark. A scalability risk exists at 500 or more records, because the dashboard statistics are computed client-side from the full list endpoints (see Section 3.3). That risk has not yet been measured.

### 3.3 Known Risk: Client-Side Dashboard Computation

The backend has no dashboard or reporting endpoint. `AdminDashboardPage.jsx` downloads the complete player and volunteer lists and counts them in the browser with `filter()`. None of the list endpoints support paging, so both the amount of data transferred and the browser's work grow in direct proportion to the number of records.

This matters because `03-proposed-solution.md` sets a target that the dashboard loads its summary metrics within three seconds for a sample dataset of at least 500 records. That target has not been tested. At prototype data volumes the approach is adequate, but it is a scalability concern at 500 or more records (requirement 2.5), and the payload also includes personal details (date of birth, phone) that the dashboard never displays.

The Coach Attendance page has a related weakness: saving sends one `POST` request per unrecorded player, so saving a large roster sends one request per player (issued in parallel) rather than a single request.

### 3.4 Recommended Improvement

1. **Add a server-side aggregate endpoint**, for example `GET /api/dashboard/summary` restricted to Admin, returning the counts (total and active players, total and active volunteers) using database `COUNT` queries. The dashboard would then transfer a few numbers instead of every record, and would no longer expose unneeded personal data to the browser.
2. **Add paging** (`page` and `pageSize`) to the Players and Volunteers list endpoints.
3. **Accept a batch of attendance records** in a single request for the Coach Attendance page.
4. **Re-test against the target.** Seed at least 500 players and volunteers, record the dashboard load time in the browser Network tab before and after the change, and add an automated test for the aggregate endpoint.

### 3.5 Performance Testing Summary

With small sample data, all nine pages measured in the Network tab loaded in under 10 ms (Section 3.2), so requirement 2.2 (fast responses) is met at prototype scale. Testing against the three-second, 500-record dashboard target has **not** been carried out, and the scalability risk from client-side dashboard computation (Section 3.3) remains open. Requirement NFR-02 remains at "Testing planned" in the requirements document. The main known risk and its recommended fix are identified above.

## 4. Next Steps

| Priority | Action | Area |
|---|---|---|
| 1 | Seed 500+ records and measure the dashboard against the three-second target | Performance |
| 2 | Add Volunteer and Coach role tests against Admin-only endpoints, and unauthenticated (401) tests for the Players, Volunteers and Teams endpoints | Security |
| 3 | Move the JWT key out of `appsettings.json` and replace it with a new key (SEC-01) | Security |
| 4 | Restrict default account seeding to Development (SEC-02) | Security |
| 5 | Implement the server-side dashboard summary endpoint | Performance |
