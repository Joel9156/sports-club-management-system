# Defect Register

## 1. Purpose

The defect register is used to record defects identified during testing and track their progress. It helps the team keep a clear record of each defect, its severity, current status, root cause, corrective action and retesting result.

## 2. Defect Register

| Defect ID | GitHub Issue | Feature | Defect Description | Severity | Status | Root Cause | Retest Result |
|---|---|---|---|---|---|---|---|
| DEF-01 | Issue #1 | Team Allocation | The team name was displayed as an ID after a player was allocated to a team. | Medium | Closed (superseded) | `AdminPlayersPage.jsx` rendered the raw `p.teamId` foreign key value directly in the table instead of resolving it to the matching team's name | Obsolete - field no longer displayed |
| DEF-02 | Issue #2 | Player Registration | Players registered through the Register page were not correctly reflected in the dashboard player count. | Medium | Closed | Registering a Player account and creating a Player roster record were two separate, easy-to-skip steps (the auth Register form only created a User; a second, distinct submission on the Player Registration page was needed to create the actual Player record) | Fixed and retested |
| DEF-03 | (none - found during this review) | Attendance | `POST /api/attendance` accepts a second, contradictory record for the same player and session date instead of rejecting it. | Medium | Open | No uniqueness check on `(PlayerId, SessionDate)` in `AttendanceController` or the database schema | Not applicable - no fix yet |

### Defect Severity

- **High:** A major function cannot be used or there is a serious security or data problem.
- **Medium:** A function works incorrectly but the system can still be used.
- **Low:** A minor issue that does not significantly affect the main system functions.

## 3. Root Cause Analysis

Root cause analysis will be completed for each confirmed defect. This will help the team identify why the problem occurred, how it was corrected and whether the fix worked successfully after retesting.

### 3.1 DEF-01 - Team Allocation Display

**Problem:** The team name was displayed as an ID after a player was allocated to a team.

**Root Cause:** Confirmed by reading the pre-fix version of `frontend/src/pages/admin/AdminPlayersPage.jsx` (commit `120aa5a`), which rendered `<td>{p.teamId ?? '—'}</td>` in the Players table - the raw numeric foreign key - rather than looking up the corresponding team's `name` from the teams list.

**Corrective Action:** This was not fixed with a targeted lookup/join fix. Commit `0b2f4b9` ("Remove team management feature and simplify attendance/roster views") removed the Team column from `AdminPlayersPage.jsx` entirely as part of a broader scope change - Mt Eden FC is now modelled as a single-team club, so `PlayerForm.jsx` no longer collects or displays a `teamId` at all (see Task 3 / `03-proposed-solution.md` for the scope-change rationale). The defect's specific symptom can no longer occur because the field it appeared in doesn't exist any more, not because the display logic was corrected.

**Retest Result:** N/A - not retested against the original display logic, since that code path was removed rather than fixed. Confirmed via code review that no page currently renders a player's `teamId`.

### 3.2 DEF-02 - Player Registration Not Reflected on Dashboard/Players List

**Problem:** Players registered through the Register page were not correctly reflected in the dashboard player count (or the Admin Players list).

**Root Cause:** Confirmed by reading commit `4a11fb8`. Creating a login (via `/api/auth/register`) and creating the actual Player roster record (via `POST /api/players`, submitted separately from the Player Registration page) were two unrelated steps. A user who registered an account but never separately submitted the Player Registration form had no Player record at all, so they never appeared anywhere Player records are listed or counted.

**Corrective Action:** Commit `4a11fb8` ("Auto-create a Player record when a Player account registers (#2)") changed `AuthController.Register` to create a matching Player record (same name/email) immediately when a Player-role account is registered, guarded by an existing-record check so it doesn't create a duplicate if one already exists. `PlayerRegisterPage.jsx` was updated to look up and `PUT`-update that auto-created record (to fill in date of birth/phone) instead of always `POST`-ing a new one.

**Retest Result:** Pass. Covered by automated tests `Register_WithPlayerRole_CreatesLinkedPlayerRecord` (TC-12) and `Register_WithPlayerRole_DoesNotDuplicateExistingPlayerRecord` (TC-16) in `AuthControllerTests.cs`, both passing.

### 3.3 DEF-03 - Duplicate Attendance Records for the Same Player and Session Date

**Problem:** `POST /api/attendance` accepts more than one attendance record for the same player on the same session date, without rejecting or merging them. Confirmed by direct API testing: posting two records for the same `playerId` and `sessionDate` (one marking the player present, one absent) both returned `201 Created`, leaving two contradictory rows in `GET /api/attendance?playerId=&date=` for that player/date.

**Root Cause:** Confirmed by reading `backend/SportsClubApi/Controllers/AttendanceController.cs` and `Data/AppDbContext.cs`. `RecordAttendance` only checks that the referenced player exists before inserting; it never checks whether an `Attendance` row already exists for that `(PlayerId, SessionDate)` pair, and `AppDbContext.OnModelCreating` defines no unique index or constraint over that pair either. The frontend's `CoachAttendancePage.jsx` disables the checkbox for a player who already has a record for the selected date, which prevents this through the normal UI flow, but that's a client-side convenience only - it doesn't stop a second request reaching the API directly (e.g. a retried/duplicated request, two coaches marking the same session, or an Admin using the same endpoint), and doesn't protect data integrity at the source.

**Corrective Action:** Not yet implemented. This was identified and confirmed during this documentation pass rather than fixed, per the assignment allowing analysis-only defects. Recommended fix: reject a `POST` where an `Attendance` row already exists for the same `(PlayerId, SessionDate)` (mirroring the duplicate-email check already used in `PlayersController`/`AuthController`), and/or add a unique index on `(PlayerId, SessionDate)` in `AppDbContext` as a database-level backstop. Deferred rather than fixed immediately so the change (and its own test coverage) can be reviewed with the team first, given it changes API behaviour that `CoachAttendancePage.jsx` and the existing `AttendanceControllerTests.cs` tests currently assume.

**Retest Result:** Not applicable yet - no fix has been made.

## 4. Next Steps

Three confirmed defects are now documented with root cause analysis (DEF-01, DEF-02, DEF-03), meeting the Assessment 2 minimum. DEF-03 remains open - fixing it (a duplicate-check on `POST /api/attendance`) is recommended as near-term follow-up work, to be reviewed with the team before implementation since it changes existing API behaviour.
