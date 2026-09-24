# Defect Register

## 1. Purpose

The defect register is used to record defects identified during testing and track their progress. It provides a clear record of each defect, its severity, current status, root cause, corrective action and retesting result.

## 2. Defect Register

| Defect ID | GitHub Issue | Date Found | Feature | Defect Description | Severity | Status | Root Cause | Retest Result |
|---|---|---|---|---|---|---|---|---|
| DEF-01 | Issue #1 | 2026-08-16 | Player Management | The Team column in Manage Players displayed the numeric TeamId instead of the team name after a player was assigned to a team. | Medium | Closed (superseded) | `AdminPlayersPage.jsx` rendered the raw `p.teamId` value directly instead of resolving it to the matching team's name | Obsolete - the Team column no longer exists |
| DEF-02 | Issue #2 | 2026-08-16 | Player Management | A user registered through the Register page with the Player role did not appear in the Manage Players list. | Medium | Closed | Registering a Player account and creating a Player roster record were two separate, easy-to-skip steps (the auth Register form only created a User; a second, distinct submission on the Player Registration page was needed to create the actual Player record) | Pass |
| DEF-03 | (none - found during this review) | 2026-09-18 | Attendance | `POST /api/attendance` accepts a second, contradictory record for the same player and session date instead of rejecting it. | Medium | Closed | No uniqueness check on `(PlayerId, SessionDate)` in `AttendanceController` or the database schema | Pass |

## 3. Defect Severity

- **High:** A major system function cannot be used, or the defect causes a serious security or data problem.
- **Medium:** A function works incorrectly, but the main system can still be used.
- **Low:** A minor issue that does not significantly affect the main system functions.

## 4. Root Cause Analysis

Root cause analysis will be completed for each confirmed defect. This will help the team identify why the problem occurred, what action was taken to correct it and whether the fix worked successfully after retesting.

### 4.1 DEF-01 - Team Name Display

**Problem:** The Manage Players page displayed the numeric TeamId instead of the team name after a player was assigned to a team.

**Root Cause:** Confirmed by reading the pre-fix version of `frontend/src/pages/admin/AdminPlayersPage.jsx` (commit `120aa5a`), which rendered `<td>{p.teamId ?? '—'}</td>` in the Players table - the raw numeric foreign key - rather than looking up the corresponding team's `name` from the teams list.

**Corrective Action:** This was not fixed with a targeted lookup/join fix. Commit `0b2f4b9` ("Remove team management feature and simplify attendance/roster views") removed the Team column from `AdminPlayersPage.jsx` entirely as part of a broader scope change - Mt Eden FC is now modelled as a single-team club, so `PlayerForm.jsx` no longer collects or displays a `teamId` at all (see `03-proposed-solution.md`'s Team Allocation section for the scope-change rationale). The defect's specific symptom can no longer occur because the field it appeared in doesn't exist any more, not because the display logic was corrected.

**Retest Result:** N/A - not retested against the original display logic, since that code path was removed rather than fixed. Confirmed via code review that no page currently renders a player's `teamId`.

### 4.2 DEF-02 - Registered Player Not Appearing

**Problem:** A user who registered through the Register page with the Player role did not appear in the Manage Players list.

**Root Cause:** The registration process created a User account but did not automatically create a corresponding Player record. User authentication and the player roster were handled as separate records, so successful registration with the Player role did not add the user to the Players table used by the Manage Players page.

**Impact:** Newly registered players could successfully create an account but were missing from the player roster. This caused inconsistent data between registered users and the player management functionality available to administrators.

**Corrective Action:** The registration process was updated so that when a user registers with the Player role, the system checks whether a Player record already exists for the email address. If no record exists, a new Player record is automatically created using the registered user's name and email.

**Preventive Action:** Registration testing should verify that creating a Player account also creates the corresponding Player record. Duplicate-record checks should also be included to prevent multiple Player records for the same email address.

**Retest Result:** Pass.

### 4.3 DEF-03 - Duplicate Attendance Records for the Same Player and Session Date

**Problem:** `POST /api/attendance` accepts more than one attendance record for the same player on the same session date, without rejecting or merging them. Confirmed by direct API testing: posting two records for the same `playerId` and `sessionDate` (one marking the player present, one absent) both returned `201 Created`, leaving two contradictory rows in `GET /api/attendance?playerId=&date=` for that player/date.

**Root Cause:** Confirmed by reading `backend/SportsClubApi/Controllers/AttendanceController.cs` and `Data/AppDbContext.cs`. `RecordAttendance` only checks that the referenced player exists before inserting; it never checks whether an `Attendance` row already exists for that `(PlayerId, SessionDate)` pair, and `AppDbContext.OnModelCreating` defines no unique index or constraint over that pair either. The frontend's `CoachAttendancePage.jsx` disables the checkbox for a player who already has a record for the selected date, which prevents this through the normal UI flow, but that's a client-side convenience only - it doesn't stop a second request reaching the API directly (e.g. a retried/duplicated request, two coaches marking the same session, or an Admin using the same endpoint), and doesn't protect data integrity at the source.

**Corrective Action / Resolution:** Fixed in commit `a1d56d6` ("Reject duplicate attendance for the same player and date (DEF-03)"). A unique index on `(PlayerId, SessionDate)` was added in `AppDbContext` (migration `AddAttendanceUniqueIndex`) as a database-level backstop, and `AttendanceController.RecordAttendance` now checks for an existing record with the same `PlayerId` and `SessionDate` before saving and returns `409 Conflict` if one exists (mirroring the duplicate-email check in `PlayersController`/`AuthController`).

**Retest Result:** Pass. Covered by automated tests `RecordAttendance_DuplicatePlayerAndDate_ReturnsConflict` (a second POST for the same player and date returns 409 and only the original record remains) and `RecordAttendance_SamePlayerDifferentDate_ReturnsCreated` (the same player on a different date is still accepted) in `AttendanceControllerTests.cs`, both passing.

## 5. Next Steps

Three confirmed defects are now documented with root cause analysis (DEF-01, DEF-02, DEF-03), meeting the Assessment 2 minimum. DEF-03 has since been fixed and closed (duplicate-check on `POST /api/attendance` plus a unique index on `(PlayerId, SessionDate)`), so no defects are currently open.
