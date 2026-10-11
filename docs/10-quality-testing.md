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
| `GET` on Players, Teams, Volunteers, Attendance, Events, PlayerStats, Stats | Any authenticated (and approved) user |
| `POST /api/players` | Admin only |
| `PUT /api/players/{id}` | Admin, or a Player updating only their own record (date of birth and phone only) |
| `DELETE /api/players/{id}` | Admin only |
| `POST` / `PUT` / `DELETE /api/volunteers` | Admin only |
| `POST /api/teams` | Admin only |
| `POST /api/attendance` | Admin, Coach |
| `POST` / `PUT /api/events` | Admin, Coach |
| `DELETE /api/events/{id}` | Admin only |
| `POST` / `PUT` / `DELETE /api/playerstats` | Admin, Coach |
| `POST /api/notifications/send` | Admin, Coach |
| `GET /api/users/pending`, `POST .../approve`, `POST .../reject` | Admin only |
| `POST /api/auth/register` | Public, but only the Player and Volunteer roles can be requested, and the account starts unapproved (see Section 2.3) |

**Automated test evidence**

| Test case | Automated test | What it verifies | Result |
|---|---|---|---|
| TC-07 | `AuthorizationTests.DeletePlayer_AsPlayerRole_ReturnsForbidden` | A Player cannot call an Admin-only endpoint (`DELETE /api/players/{id}`); 403 Forbidden is returned | Pass |
| TC-17 | `AttendanceControllerTests.RecordAttendance_AsPlayerRole_ReturnsForbidden` | A Player cannot record attendance (Admin/Coach only); 403 Forbidden is returned | Pass |
| TC-14 | `AuthControllerTests.Register_WithAdminRole_ReturnsBadRequest` | Self-registration cannot be used to obtain an Admin account; 400 Bad Request is returned and no account is created | Pass |
| TC-21 | `NotificationsControllerTests.MarkAsRead_ForAnotherUsersNotification_ReturnsNotFound` | A user cannot act on another user's notification; 404 Not Found is returned | Pass |
| TC-37, TC-38 | `RolePermissionTests.CreatePlayer_AsPlayer_ReturnsForbidden`, `CreateVolunteer_AsVolunteer_ReturnsForbidden` | A Player/Volunteer cannot register themselves onto the roster; 403 Forbidden | Pass |
| TC-40, TC-41 | `RolePermissionTests.UpdatePlayer_AnotherPlayersRecord_ReturnsForbidden`, `EditOrDeletePlayer_AsCoach_ReturnsForbidden` | A Player cannot edit another player's record, and a Coach cannot edit or delete any player record; 403 Forbidden | Pass |
| TC-47, TC-53 | `UsersControllerTests.PendingAccounts_AsCoach_ReturnsForbidden`, `RejectAccountTests.Reject_AsCoach_ReturnsForbidden` | A Coach cannot view, approve or reject pending accounts; 403 Forbidden | Pass |

**Coverage.** The original objective was to verify that every role other than Admin is denied Admin-only endpoints. This is now demonstrated for the Player, Volunteer and Coach roles across players, volunteers and account approval (table above). The Coach and Volunteer roles are still not individually tested against `DELETE /api/volunteers/{id}` or `POST /api/teams`; these are protected by the same `[Authorize(Roles = "Admin")]` mechanism as the endpoints that are tested, so the residual risk is considered low (see Section 4).

### 2.3 Account Approval Gate

Role checks alone were not enough once self-registration stopped creating a roster record immediately (see `03-proposed-solution.md`, Account Approval): an account also needs to be approved before it can do anything at all, not just before it reaches a specific Admin-only endpoint. This is enforced by `RequireApprovedAccountMiddleware`, which runs on every authenticated request and checks the account's `IsApproved` flag directly in the database (not from the JWT), so an approval or rejection takes effect immediately without the user logging in again.

| Test case | Automated test | What it verifies | Result |
|---|---|---|---|
| TC-48 | `ApprovalAccessTests.UnapprovedAccount_IsBlockedFromProtectedEndpoints` | A pending account with a valid token still gets 403 Forbidden on a protected endpoint | Pass |
| TC-49 | `ApprovalAccessTests.Approval_TakesEffectWithoutReLogin` | The same token works immediately once an Admin approves the account | Pass |
| TC-45, TC-46 | `UsersControllerTests.ApprovePlayer_CreatesPlayerRecord_AndLeavesPending`, `ApproveVolunteer_WithExistingRecord_DoesNotDuplicateIt` | Approving creates the roster/volunteer record exactly once | Pass |
| TC-50 | `RejectAccountTests.RejectPendingAccount_BlocksAccessAndMarksRejected` | A rejected account stays blocked and is marked as rejected | Pass |
| TC-51, TC-52 | `RejectAccountTests.RejectedAccount_CanBeApprovedLater`, `Reject_ApprovedOrUnknownAccount_IsRejected` | A rejected account can be approved later; an already-approved or unknown account cannot be rejected | Pass |

### 2.4 Authentication

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

### 2.5 Known Security Risks

These are risks that were identified during development and are accepted for the prototype. They should be resolved before any real deployment.

| ID | Risk | Severity (if deployed) | Current state | Recommended action |
|---|---|---|---|---|
| SEC-01 | **JWT signing key stored in `appsettings.json`.** The file is tracked in Git, so the key is visible to anyone with access to the repository and remains in the commit history. Anyone who holds the key can mint a valid token for any role, including Admin, which would bypass all role-based access control. | High | Accepted for the prototype. Noted in `backend/README.md`. | Move the key to user-secrets (development) or an environment variable / secret store (production), and generate a new key, because the current one is in the history. |
| SEC-02 | **Fixed default credentials.** `DbSeeder` creates an Admin and a Coach account with a known password on every startup, in every environment. | High | Intended for local demo use only, and documented in `backend/README.md`. | Seed only in the Development environment, or read the initial password from configuration. |
| SEC-03 | **No per-record scoping.** Access control is at route level only. Any authenticated user, including a Player or Volunteer, can read the full lists of players (including date of birth and phone number), volunteers and attendance. This only partly meets requirement 2.1. | Medium | Deferred deliberately (see the comment in `PlayersController.cs`). | Scope reads so a Player sees only their own record and a Coach only relevant records. |

### 2.6 Security Testing Summary

All automated security tests pass (92 of 92 tests in the backend suite passed on the last full run, 2026-10-11). Authentication is enforced, invalid logins are rejected, role restrictions hold across players, volunteers, schedule, stats and account approval, and the new approval gate (2.3) blocks unapproved and rejected accounts from the rest of the API while taking effect immediately once approved. Testing is incomplete in two respects: `DELETE /api/volunteers/{id}` and `POST /api/teams` are not individually tested against every non-Admin role, and the three risks in Section 2.5 remain open. No penetration testing or dependency vulnerability scanning was carried out.

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
| 2 | Test `DELETE /api/volunteers/{id}` and `POST /api/teams` against every non-Admin role | Security |
| 3 | Move the JWT key out of `appsettings.json` and replace it with a new key (SEC-01) | Security |
| 4 | Restrict default account seeding to Development (SEC-02) | Security |
| 5 | Implement the server-side dashboard summary endpoint | Performance |
