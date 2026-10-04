# Requirements Traceability Matrix

This matrix links each requirement to the features that implement it, the test cases that verify it, their results and any defects. FR-01 to FR-07 and NFR-01 to NFR-06 are the requirements from Assessment 1 (`docs/docs/02-requirements.md`). FR-08 to FR-11 were added during Assessment 2 for features built after that document was written.

**Status values:** *Verified* = tested and passing, with the evidence listed. *Partially verified* = some of the requirement is tested, a gap is noted. *Not verified* = not tested yet. *Out of scope* = deliberately removed, with the reason given.

## 1. Matrix

| Req ID | Requirement | Implemented by | Test cases | Results | Defects | Status |
|---|---|---|---|---|---|---|
| FR-01 | Users can securely log in | Login, JWT auth, `AuthController` | TC-01, TC-02, TC-10 | Pass (automated `AuthControllerTests`, `AttendanceControllerTests`) | None | Verified |
| FR-02 | Administrators can register and maintain player records | Players page, `PlayersController`, self-registration then Admin approval | TC-03, TC-04, TC-12, TC-13, TC-16, TC-39, TC-40, TC-45 | TC-03/04 manual Pass (2026-08-16); TC-12, TC-13, TC-16, TC-39, TC-40, TC-45 automated Pass | DEF-02 (closed, fix superseded by approval flow) | Verified |
| FR-03 | Administrators can register and maintain volunteer records | Volunteers page, `VolunteersController`, approval creates the record | TC-05, TC-15, TC-38, TC-46 | TC-05 manual Pass; TC-15, TC-38, TC-46 automated Pass | None | Verified |
| FR-04 | Authorised users can allocate players to teams | *Removed* (single-team club) | TC-06 (historical) | TC-06 Pass for the original feature; feature later removed | DEF-01 (superseded: the feature it affected was removed) | Out of scope |
| FR-05 | Coaches or administrators can record and view attendance | Coach Attendance page, Attendance History, `AttendanceController` | TC-09, TC-10, TC-11, TC-17, TC-18, TC-19 | Pass (automated `AttendanceControllerTests`) | DEF-03 (closed) | Verified |
| FR-06 | Users can view reports and dashboards | Admin Dashboard, Stats page, `StatsController` | TC-08, TC-32, TC-33 | Pass (automated); dashboard counts are computed in the browser from the list endpoints | None | Partially verified |
| FR-07 | The system provides relevant notifications | Notifications page, attendance notifications, Send Notice page | TC-20, TC-21, TC-42, TC-43, TC-44 | Pass (automated) | None | Partially verified |
| FR-08 | Users can view a club schedule of matches and training, and Coaches/Admins can manage it | Schedule pages, `EventsController` | TC-22, TC-23, TC-24, TC-25, TC-26, TC-34, TC-35 | Pass (automated); manual UI test not yet recorded | None | Verified |
| FR-09 | Coaches/Admins can record each player's goals and assists per match, and users can view player totals and the team record | Stats pages, `PlayerStatsController`, `StatsController` | TC-27, TC-28, TC-29, TC-30, TC-31, TC-32, TC-33, TC-36 | Pass (automated); manual UI test not yet recorded | None | Verified |
| FR-10 | Access to each feature depends on role (Admin, Coach, Player, Volunteer) | Route-level and API `[Authorize(Roles)]` checks | TC-07, TC-23, TC-30, TC-35, TC-37, TC-38, TC-40, TC-41, TC-43, TC-47, TC-53 | Pass (automated `AuthorizationTests`, `RolePermissionTests`, and others) | None | Verified |
| FR-11 | New accounts are reviewed by an Admin before they can use the system; the Admin can approve or reject them | Pending-approval page, Approvals page, `UsersController`, `RequireApprovedAccountMiddleware` | TC-12, TC-45, TC-46, TC-47, TC-48, TC-49, TC-50, TC-51, TC-52, TC-53 | Pass (automated `UsersControllerTests`, `ApprovalAccessTests`, `RejectAccountTests`); full flow also checked manually in the browser on 2026-10-04 | None | Verified |
| NFR-01 | Sensitive information is protected from unauthorised access | Role checks, approval gate, per-user notifications | TC-07, TC-10, TC-14, TC-17, TC-21, TC-30, TC-37, TC-38, TC-40, TC-41, TC-43, TC-47, TC-48, TC-50, TC-53 | Pass (automated) | None | Partially verified |
| NFR-02 | The system responds to common operations within 2 seconds | Browser-measured page loads | None (see `10-quality-testing.md` §3) | Page loads under 10 ms on small sample data; 500-record dashboard target not tested | None | Partially verified |
| NFR-03 | The interface is simple and easy to understand | Navigation per role, pending and rejected messages | None (no usability test run) | Not tested | None | Not verified |
| NFR-04 | Records remain accurate and consistent (no duplicates) | Unique email, attendance uniqueness, stat uniqueness | TC-03, TC-05, TC-13, TC-16, TC-28, TC-39, TC-46 | Pass (automated for duplicates and uniqueness) | DEF-02, DEF-03 (closed) | Verified |
| NFR-05 | The system can support increasing numbers of users and records | Not tested | None | Not tested; the prototype's scale is limited to sample data | None | Not verified |
| NFR-06 | The system can be updated and maintained efficiently | 58 automated tests, GitHub Actions CI (`ci.yml`), documentation | All automated tests | Build and test pass in CI for the backend; frontend lint and build are not run in CI | None | Partially verified |

## 2. Changes from Assessment 1

- **FR-04 (team allocation) was removed.** The club was modelled as a single team, so team management was dropped from the frontend (commit `0b2f4b9`). The reasoning is in `03-proposed-solution.md` (Team Allocation section). TC-06 and DEF-01 remain on record as history.
- **FR-08 to FR-11 were added.** Club schedule, player statistics, role-based management permissions and account approval were built during Assessment 2 and are not in the Assessment 1 requirements document. They need to be added there.
- **FR-07 is only partly implemented.** Notifications are sent for attendance and by Coaches/Admins. Requirement 1.7 also mentions registration and team-allocation updates. Notifying applicants when their account is approved or rejected is not built yet.

## 3. Open items

- **Requirement 2.1 security gap:** Any authenticated user can still read the full player and volunteer lists through the API (risk SEC-03 in `10-quality-testing.md`). This is accepted for the prototype and affects NFR-01.
- **NFR-02 and NFR-05:** Only small-scale measurements exist. The 500-record and 100-user targets have not been tested.
- **NFR-03:** No usability testing with users has been done.
- **Approved-screen refresh:** A browser tab keeps the approval status it got at login until the user logs in again. The API check is immediate. This is a known limitation (see `05-test-cases.md`).
