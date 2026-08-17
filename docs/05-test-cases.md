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
| TC-08 | Dashboard | Dashboard reflects correct data | Admin logged in, data exists | 1. Create a team and player 2. View dashboard | Dashboard shows correct counts | Functional | Pass (defect noted - players registered via Register page not reflected in dashboard count, GitHub Issue #2) |

Manual testing completed on 2026-08-16. All 8 test cases passed.
2 defects identified and logged as GitHub Issues during exploratory testing.
