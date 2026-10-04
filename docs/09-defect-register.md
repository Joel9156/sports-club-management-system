# Defect Register

## 1. Purpose

The defect register is used to record defects identified during testing and track their progress. It provides a clear record of each defect, its severity, current status, root cause, corrective action and retesting result.

## 2. Defect Register

| Defect ID | GitHub Issue | Date Found | Feature | Defect Description | Severity | Status | Root Cause | Retest Result |
|---|---|---|---|---|---|---|---|---|
| DEF-01 | Issue #1 | 2026-08-16 | Player Management | The Team column in Manage Players displayed the numeric TeamId instead of the team name after a player was assigned to a team. | Medium | Closed (superseded) | `AdminPlayersPage.jsx` rendered the raw `p.teamId` value directly instead of resolving it to the matching team's name | Obsolete - the Team column no longer exists |
| DEF-02 | Issue #2 | 2026-08-16 | Player Management | A user registered through the Register page with the Player role did not appear in the Manage Players list. | Medium | Closed (fix superseded) | Registering a Player account and creating a Player roster record were two separate, easy-to-skip steps (the auth Register form only created a User; a second, distinct submission on the Player Registration page was needed to create the actual Player record) | Pass (against the original fix - see note below; the roster record is now created at Admin approval instead) |
| DEF-03 | (none - found during this review) | 2026-09-18 | Attendance | `POST /api/attendance` accepts a second, contradictory record for the same player and session date instead of rejecting it. | Medium | Closed | No uniqueness check on `(PlayerId, SessionDate)` in `AttendanceController` or the database schema | Pass |
## Final Defect Closure and Residual Risk Analysis

### Purpose

This section evaluates the resolution of defects identified during development and considers whether any remaining risks could affect the quality of the final system.

### Defect Closure Review

| Defect ID | Original Problem | Corrective Action | Closure Evidence Required |
|---|---|---|---|
| DEF-01 | The Players list displayed a numeric team ID instead of the team name. | The original team-allocation feature was removed when the project scope changed to a single-team club. | Confirm that the current interface follows the revised requirements and that the original defect is no longer applicable. |
| DEF-02 | Users registering with the Player role did not appear in the Players list. | The registration workflow was updated to create a corresponding Player record. The later account-approval workflow superseded this implementation. | Verify that an approved Player account produces the expected roster record and appears in the Players list. |
| DEF-03 | Duplicate attendance records could be created for the same player and session date. | Duplicate prevention was added through application-level validation and a database uniqueness constraint. | Verify that a duplicate attendance submission is rejected and that only one record is stored. |

### Root Cause and Preventive Action Review

**DEF-01: Team information display**

The original problem involved the presentation of an internal team identifier in the user interface. Following the removal of team allocation from the project scope, the original display requirement is no longer applicable.

The preventive action is to review interface functionality whenever project requirements change, ensuring that obsolete features and their associated tests are updated consistently.

**DEF-02: Player registration and roster consistency**

The original registration workflow created an authentication account without automatically creating the corresponding player roster record. This resulted in inconsistent information between registered accounts and the Players list.

The later account-approval workflow changed how player records are created. Regression testing should therefore cover the complete registration, approval and roster-creation process rather than testing only the original registration method.

**DEF-03: Duplicate attendance records**

The attendance implementation previously lacked sufficient protection against duplicate records for the same player and session date.

The corrective approach includes checking for an existing attendance record before saving and enforcing uniqueness at the database level. This provides protection at both the application and data-storage levels.

The preventive action is to retain automated tests that attempt duplicate attendance submissions and verify that the system rejects them.

### Residual Risk Assessment

| Area | Remaining Risk | Recommended Verification |
|---|---|---|
| Team management | Historical defect documentation may not reflect the revised single-team scope. | Confirm that the current requirements, interface and historical defect records are consistent. |
| Player registration | Changes to the approval workflow could reintroduce inconsistencies between User and Player records. | Test registration, approval and player-list visibility together. |
| Attendance | Future changes to attendance validation could allow duplicate submissions. | Retain duplicate-prevention tests and confirm the database constraint remains effective. |

### Contribution to the Final Release Decision

Defect closure should be based on evidence that the corrective action is effective or that the affected requirement has formally been removed from scope.

A closed defect does not automatically demonstrate that all related functionality is free from risk. Regression testing is needed to confirm that later changes have not reintroduced previously identified problems.

The final release decision should consider unresolved defects, their severity, available retest evidence and any accepted residual risks.

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

**Update (2026-09-28):** The corrective action above no longer matches the code. `AuthController.Register` was changed to support Admin-approved registration: a self-registered Player/Volunteer account is now created with `IsApproved = false` and no Player/Volunteer record is created at registration time at all - that record will instead be created when an Admin approves the account (approval endpoint in progress, see `05-test-cases.md`'s note under TC-12/TC-16). This is a deliberate design change, not a regression discovered through testing: registering without any review let anyone put themselves straight onto the roster, which was judged too permissive once the system had multiple roles with different levels of trust. The approval endpoint (`POST /api/users/{id}/approve`) and the Admin Approvals page are now built, and approving an account creates its roster record (TC-45, TC-46). Between registration and approval a new Player does not appear on the roster - that is the intended behaviour, not DEF-02's original symptom. Manual retest of the full flow (register, approve, reject, re-approve) is recorded in the test cases.

### 4.3 DEF-03 - Duplicate Attendance Records for the Same Player and Session Date

**Problem:** `POST /api/attendance` accepts more than one attendance record for the same player on the same session date, without rejecting or merging them. Confirmed by direct API testing: posting two records for the same `playerId` and `sessionDate` (one marking the player present, one absent) both returned `201 Created`, leaving two contradictory rows in `GET /api/attendance?playerId=&date=` for that player/date.

**Root Cause:** Confirmed by reading `backend/SportsClubApi/Controllers/AttendanceController.cs` and `Data/AppDbContext.cs`. `RecordAttendance` only checks that the referenced player exists before inserting; it never checks whether an `Attendance` row already exists for that `(PlayerId, SessionDate)` pair, and `AppDbContext.OnModelCreating` defines no unique index or constraint over that pair either. The frontend's `CoachAttendancePage.jsx` disables the checkbox for a player who already has a record for the selected date, which prevents this through the normal UI flow, but that's a client-side convenience only - it doesn't stop a second request reaching the API directly (e.g. a retried/duplicated request, two coaches marking the same session, or an Admin using the same endpoint), and doesn't protect data integrity at the source.

**Corrective Action / Resolution:** Fixed in commit `a1d56d6` ("Reject duplicate attendance for the same player and date (DEF-03)"). A unique index on `(PlayerId, SessionDate)` was added in `AppDbContext` (migration `AddAttendanceUniqueIndex`) as a database-level backstop, and `AttendanceController.RecordAttendance` now checks for an existing record with the same `PlayerId` and `SessionDate` before saving and returns `409 Conflict` if one exists (mirroring the duplicate-email check in `PlayersController`/`AuthController`).

**Retest Result:** Pass. Covered by automated tests `RecordAttendance_DuplicatePlayerAndDate_ReturnsConflict` (a second POST for the same player and date returns 409 and only the original record remains) and `RecordAttendance_SamePlayerDifferentDate_ReturnsCreated` (the same player on a different date is still accepted) in `AttendanceControllerTests.cs`, both passing.

## 5. Next Steps

Three confirmed defects are now documented with root cause analysis (DEF-01, DEF-02, DEF-03), meeting the Assessment 2 minimum. DEF-03 has since been fixed and closed (duplicate-check on `POST /api/attendance` plus a unique index on `(PlayerId, SessionDate)`). DEF-02's original fix was superseded by Admin-approved registration (see the update under 4.2), which is now implemented and tested.
