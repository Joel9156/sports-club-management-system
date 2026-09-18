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

Manual testing completed on 2026-08-16. All 8 test cases passed.
2 defects identified and logged as GitHub Issues during exploratory testing.

TC-09 through TC-13 added to cover the Attendance feature and the Issue #2 fix (self-registered players auto-added to the roster); result outcomes reflect the current implementation.

TC-14 through TC-21 added to expand coverage past 15 test cases, focused on Attendance (role restrictions, validation, date filtering) plus the initial slice of the Notification feature (in-app notifications triggered by attendance being recorded, scoped to the recipient's own account).
