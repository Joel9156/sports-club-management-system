# Test Cases

| TC ID | Feature | Test Case | Precondition | Steps | Expected Result | Type | Result |
|-------|---------|-----------|--------------|-------|-----------------|------|--------|
| TC-01 | Authentication | Valid login | User account exists | 1. Go to login page 2. Enter valid email/password 3. Click login | User is logged in and redirected to dashboard | Functional | Pass |
| TC-02 | Authentication | Invalid login | None | 1. Go to login page 2. Enter wrong password 3. Click login | Error message displayed, access denied | Functional | Pass |
| TC-03 | Player Registration | Register valid player | Admin logged in | 1. Go to Players page 2. Fill all required fields 3. Submit | Player appears in player list | Functional | Pass |
| TC-04 | Player Registration | Submit with missing required field | Admin logged in | 1. Go to Players page 2. Leave name empty 3. Submit | Validation error shown, record not saved | Functional | Pass |
| TC-05 | Volunteer Management | Register valid volunteer | Admin logged in | 1. Go to Volunteers page 2. Fill all required fields 3. Submit | Volunteer appears in volunteer list | Functional | Pass |
| TC-06 | Team Allocation | Assign player to team | Admin logged in, player and team exist | 1. Edit player 2. Select team 3. Save | Player appears under correct team in dashboard | Functional | Pass (defect noted - team name displays as ID, GitHub Issue #1) |
| TC-07 | Role-based Access | Non-admin cannot access admin features | Logged in as Player role | 1. Login as Player 2. Try to access admin-only page | Access denied or redirected | Non-functional (Security) | Pass |
| TC-08 | Dashboard | Dashboard reflects correct data | Admin logged in, data exists | 1. Create a team and player 2. View dashboard | Dashboard shows correct counts | Functional | Pass (defect noted at the time - players registered via Register page not reflected in dashboard count, GitHub Issue #2 - since fixed, see DEF-02 in 09-defect-register.md and TC-12/TC-16 below) |
| TC-09 | Attendance | Record valid attendance | Coach logged in, player and team exist | 1. Go to Attendance page 2. Select team and date 3. Mark player present 4. Save | Attendance record saved successfully | Functional | Pass |
| TC-10 | Attendance | Record attendance without login | None | 1. Try to access attendance page without login | Redirected to login page | Non-functional (Security) | Pass |
| TC-11 | Attendance | View attendance by team | Coach logged in | 1. Go to Attendance page 2. Select team | Attendance records for that team displayed | Functional | Pass |
| TC-12 | Player Registration | Player registration auto-creates roster record | None | 1. Register new account as Player 2. Admin views Players list | Player appears in Players list automatically | Functional | Pass |
| TC-13 | Authentication | Duplicate player email rejected | None | 1. Register with existing email 2. Submit | 409 Conflict returned | Functional | Pass |
| TC-14 | Authentication | Self-registration rejects Admin role | None | 1. Submit register form with role = Admin | 400 Bad Request returned, no account created | Non-functional (Security) | Pass |
| TC-15 | Player Registration | Volunteer registration doesn't create a player record | None | 1. Register new account as Volunteer 2. Admin views Players list | No matching player record created | Functional | Pass |
| TC-16 | Player Registration | Registering doesn't duplicate an existing player record | Admin pre-created a player with the same email | 1. Register a Player account using that email | Only one player record exists for that email | Functional | Pass |
| TC-17 | Attendance | Non-admin/coach cannot record attendance | Logged in as Player role | 1. Attempt to record attendance as a Player | 403 Forbidden returned | Non-functional (Security) | Pass |
| TC-18 | Attendance | Record attendance against a nonexistent player | Coach logged in | 1. Submit attendance for a player id that doesn't exist | 400 Bad Request returned | Functional | Pass |
| TC-19 | Attendance | Filter attendance by session date | Admin logged in, records exist for multiple dates | 1. Request attendance filtered by a specific date | Only records for that date are returned | Functional | Pass |
| TC-20 | Notification | Attendance recorded generates a notification | Player account exists with a matching email | 1. Coach records attendance for that player 2. Player logs in and views notifications | Notification about the recorded attendance is visible | Functional | Pass |
| TC-21 | Notification | Users cannot mark another user's notification as read | Two user accounts exist | 1. Log in as a different user 2. Attempt to mark the first user's notification as read | 404 Not Found returned | Non-functional (Security) | Pass |
| TC-22 | Schedule | Admin adds a match | Admin logged in | 1. Add a match with date, location and opponent | 201 Created, match appears on the schedule | Functional | Pass |
| TC-23 | Schedule | Player cannot manage the schedule | Logged in as Player | 1. Attempt to add an event as a Player | 403 Forbidden returned | Non-functional (Security) | Pass |
| TC-24 | Schedule | Match without an opponent rejected | Admin logged in | 1. Add a match with no opponent | 400 Bad Request returned | Functional | Pass |
| TC-25 | Schedule | Training cannot carry a match score | Admin logged in | 1. Add a training session with goals for/against | 400 Bad Request returned | Functional | Pass |
| TC-26 | Schedule | Any role can read the schedule | Events exist, logged in as Player | 1. Request the schedule as a Player | Events returned, soonest first | Functional | Pass |
| TC-27 | Stats | Admin records a player's goals/assists for a match | Admin logged in, match and player exist | 1. Record 2 goals, 1 assist for a player in a match | 201 Created, values saved | Functional | Pass |
| TC-28 | Stats | Duplicate stats for one player in one match rejected | Stats already recorded for that player and match | 1. Record stats for the same player and match again | 409 Conflict returned | Functional | Pass |
| TC-29 | Stats | Stats cannot be recorded against training | Admin logged in, a training session exists | 1. Record stats against the training session | 400 Bad Request returned | Functional | Pass |
| TC-30 | Stats | Only Coaches and Admins can enter stats | Logged in as Player | 1. Attempt to record stats as a Player | 403 Forbidden returned | Non-functional (Security) | Pass |
| TC-31 | Stats | Negative goals rejected | Admin logged in | 1. Record stats with goals = -1 | 400 Bad Request returned | Functional | Pass |
| TC-32 | Stats | Player totals add up across matches | Stats recorded for several matches | 1. Request player totals | Matches, goals and assists summed per player, top scorer first | Functional | Pass |
| TC-33 | Stats | Team record computed from scored matches | Won, drawn, lost and unscored matches exist | 1. Request the team record | Wins/draws/losses and goals correct; unscored match counted as upcoming | Functional | Pass |
| TC-34 | Schedule | Coach adds a training session | Coach logged in | 1. Add a training session with date and location | 201 Created | Functional | Pass |
| TC-35 | Schedule | Only Admins can delete an event | Match exists, Coach and Admin accounts exist | 1. Delete the match as a Coach 2. Delete it as an Admin | Coach gets 403 Forbidden; Admin gets 204 No Content | Non-functional (Security) | Pass |
| TC-36 | Stats | Coach records a player's goals/assists | Coach logged in, match and player exist | 1. Record 1 goal, 2 assists for a player in a match | 201 Created | Functional | Pass |
| TC-37 | Player Registration | Only Admins can register a player | Logged in as Player | 1. Attempt to create a player record as a Player | 403 Forbidden returned | Non-functional (Security) | Pass |
| TC-38 | Volunteer Management | Only Admins can register a volunteer | Logged in as Volunteer | 1. Attempt to create a volunteer record as a Volunteer | 403 Forbidden returned | Non-functional (Security) | Pass |
| TC-39 | Player Registration | Player completes their own record | Player account and matching roster record exist | 1. Update own record with a new phone and date of birth, also trying to change the email and set inactive | 204 returned; phone and date of birth saved; email and active status unchanged | Functional | Pass |
| TC-40 | Player Registration | Player cannot edit another player's record | Two players exist | 1. As Player A, update Player B's record | 403 Forbidden returned | Non-functional (Security) | Pass |
| TC-41 | Player Registration | Coach cannot edit or delete player records | Coach logged in, player exists | 1. Edit a player as a Coach 2. Delete a player as a Coach | 403 Forbidden returned for both | Non-functional (Security) | Pass |
| TC-42 | Notification | Coach sends a notice to all players | Two player accounts and one volunteer account exist | 1. Coach sends a message with audience "Players" 2. A player and the volunteer view their notifications | Response reports 2 sent; each player sees the message; the volunteer sees none | Functional | Pass |
| TC-43 | Notification | Player cannot send notices | Logged in as Player | 1. Attempt to send a notice as a Player | 403 Forbidden returned | Non-functional (Security) | Pass |
| TC-44 | Notification | Blank notice rejected | Coach logged in | 1. Send a notice whose message is only spaces | 400 Bad Request returned | Functional | Pass |

Manual testing completed on 2026-08-16. All 8 test cases passed.
2 defects identified and logged as GitHub Issues during exploratory testing.

TC-09 through TC-13 added to cover the Attendance feature and the Issue #2 fix (self-registered players auto-added to the roster); result outcomes reflect the current implementation.

TC-14 through TC-21 added to expand coverage past 15 test cases, focused on Attendance (role restrictions, validation, date filtering) plus the initial slice of the Notification feature (in-app notifications triggered by attendance being recorded, scoped to the recipient's own account).

TC-22 through TC-33 cover the schedule and stats features (matches and training on a club schedule; per-match goals/assists, player totals and the team record). They were verified at the API level by the automated xUnit tests of the same names (`EventsControllerTests`, `PlayerStatsControllerTests`, `StatsControllerTests`), all passing; the new frontend pages for these features have not yet been through a documented manual UI test.

TC-34 through TC-44 cover the role split: Admins register, edit and delete players and volunteers and delete events; Coaches add and edit events, enter stats, record attendance and send notices; Players may only complete their own record; every role can read. The rules are enforced by the API and verified by the xUnit tests (`EventsControllerTests`, `PlayerStatsControllerTests`, `RolePermissionTests`, `NotificationsControllerTests`), 49 tests in total all passing. TC-23 and TC-30 were reworded to match (Coaches are now allowed to manage the schedule and stats; Players are not).
