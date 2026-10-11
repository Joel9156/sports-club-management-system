# Requirements and Quality Analysis

## 1. Functional Requirements

### 1.1 User Authentication
The system shall allow authorised users to securely log in.

### 1.2 Player Registration
The system shall allow administrators to register players and store their personal and participation information.

### 1.3 Volunteer Management
The system shall allow administrators to register and manage volunteer information and roles.

### 1.4 Team Allocation (Removed from Scope)

Removed: Mt Eden FC operates as a single team, so team allocation function is no longer required.
The system shall allow authorised users to assign players to appropriate teams.

### 1.5 Attendance Tracking
The system shall allow coaches or administrators to record and view player attendance.

### 1.6 Reports and Dashboards
The system shall provide reports and dashboards showing player, volunteer, team and attendance information.

### 1.7 Notifications
The system shall provide notifications for important updates such as team allocations and registration information.

### 1.8 Club Schedule
The system shall allow authorised users to view a club schedule of matches and training sessions, and allow Coaches and Administrators to add and edit those events.

### 1.9 Player Statistics
The system shall allow Coaches and Administrators to record each player's goals and assists for a match, and allow authorised users to view player total statistics and the team's match record.

### 1.10 Role-Based Management Permissions
The system shall restrict each management function to the roles responsible for it. Administrators manage players, volunteers and the schedule (including deleting events); Coaches record attendance, schedule events, enter statistics and send notices; Players may only complete their own player record.

### 1.11 Account Approval
The system shall require an Administrator to approve self-registered Player and Volunteer accounts before they can use the system. An Administrator can also reject an account, and a rejected account can be approved later.

## 2. Non-Functional Requirements

### 2.1 Security
The system shall restrict access to protected functions based on the user's assigned role. Unauthorised users attempting to access restricted functions shall be denied access.

### 2.2 Performance
The system shall respond to common operations, including viewing player, volunteer, team and attendance information, within 2 seconds under normal operating conditions.

### 2.3 Usability
At least 80% of users participating in usability testing shall be able to complete common tasks, such as registering a player and viewing team information, without assistance.

### 2.4 Reliability
The system shall correctly save and retrieve player, volunteer, team and attendance records in at least 99% of test transactions without data loss or corruption.

### 2.5 Scalability
The system shall support at least 100 concurrent users while maintaining a response time of no more than 3 seconds for common operations.

### 2.6 Maintainability
All automated tests for core system functionality shall pass before a release is approved. Source code and project documentation shall follow the agreed project structure and naming conventions.

## 3. Quality Analysis

### Data Accuracy

The system should maintain accurate player, volunteer, team and attendance information. Input validation should be used to reduce incorrect or incomplete data. Required fields should be checked before information is saved.

### Data Integrity

The system should reduce duplicate and inconsistent records by using unique identifiers for players and volunteers. Relationships between players, teams and attendance records should remain consistent when information is updated.

### Security

Sensitive player and volunteer information should only be accessible to authorised users. Role-based access should be used so that users can only view or modify information relevant to their responsibilities.

### Usability

The system should provide a clear and simple interface so that administrators, coaches and volunteers can complete common tasks without unnecessary difficulty.

### Reliability

The system should store information consistently and make sure that saved records remain available when users need them.

## Requirements Review and Refinement

The requirements were reviewed to make sure they are clear, consistent, realistic and testable. Each functional and non-functional requirement has an ID so it can be linked to acceptance criteria and test cases.

Some requirements were also improved to make them easier to test. For example, instead of saying that the system should "manage players", the requirement states that administrators should be able to register and maintain player records.

## 4. Quality Criteria

The system will be assessed using the following quality criteria:

* **Accuracy:** Player, volunteer, team and attendance information should be correct.
* **Integrity:** Duplicate and inconsistent records should be minimised.
* **Security:** Sensitive information should only be accessible to authorised users.
* **Usability:** Users should be able to complete common tasks easily.
* **Reliability:** Information should remain available and consistent.
* **Performance:** Normal system functions should respond within an acceptable time.
* **Maintainability:** The system should be structured so that future changes can be made efficiently.

## 5. Acceptance Criteria

### 5.1 User Authentication

* Users with valid login details can successfully access the system.
* Invalid login details are rejected.
* Users can only access features allowed for their role.

### 5.2 Player Registration

* An authorised user can register a new player.
* Required player information must be entered before the record can be saved.
* Duplicate player records should be prevented.
* Successfully registered players should appear in the system.

### 5.3 Volunteer Management

* An authorised user can register a volunteer.
* Volunteer contact details and roles can be stored.
* Required fields are validated before the record is saved.
* Existing volunteer information can be updated.

### 5.4 Team Allocation (Removed from Scope)

* Removed: Mt Eden FC operates as a single team, so team allocation function is no longer required.
* An authorised user can assign a player to a team.
* The player should appear in the correct team after allocation.
* Invalid or duplicate team allocations should be prevented where appropriate.

### 5.5 Attendance Tracking

* An authorised user can record attendance for a player.
* Attendance information is linked to the correct player.
* Previous attendance records can be viewed.

### 5.6 Reports and Dashboards

* Authorised users can view player, volunteer, team and attendance information.
* Reports display information stored in the central system.
* Users can filter relevant information where required.

### 5.7 Notifications

* The system can provide important notifications to relevant users.
* Notifications should relate to events such as registration, team allocation or other important updates.

### 5.8 Club Schedule

* Only Administrators and Coaches can add or edit schedules.
* Only Administrators can delete events. 
* A match requires an opponent, and a training session cannot have a score. Invalid entries are rejected.
* All authorised users can view the schedule.

### 5.9 Player Statistics

* Only Coaches and Administrators can record statistics.
* Goals and assists are recorded for each player in each match.
* Stats can only be recorded for matches, not training sessions.
* Goals and assists cannot be negative.
* Duplicate entries for the same player in the same match are not permitted.
* All roles can view players' totals (matches, goals and assists) and the team's match record (wins, draws and losses).

### 5.10 Role-Based Management Permissions

* Only Administrators can register, edit or delete players and volunteers, and delete a schedule event.
* Coaches can also add or edit schedule events, record attendance, enter player statistics, and send notices.
* Players can only update their own player details, not another player's, and only certain fields (not their email, team or active status).
* Any authorised user can view players, volunteers, the schedule and statistics - only write access is restricted by role.

### 5.11 Account Approval

* A self-registered Player or Volunteer account is created as pending, and no roster or volunteer record exists for it until an Administrator approves it.
* A pending account cannot use the rest of the system until it is approved.
* When an Administrator approves an account, the matching Player or Volunteer record is created, and no duplicate is made if one already exists.
* An Administrator can reject a pending account; the account is kept, not deleted, and remains blocked.
* A rejected account can later be approved by an Administrator, after which it can use the system.
* Coaches cannot approve or reject accounts.

## 6. Requirements Traceability Matrix

The Requirements Traceability Matrix (RTM) links the main system requirements with their quality expectations and testing activities. The matrix is updated as testing progresses so the team can track which requirements have been tested, fixed or still require further testing.

| ID | Requirement | Type | Quality Attribute | Planned Test | Current Status |
|---|---|---|---|---|---|
| FR-01 | Users can securely log in to the system | Functional | Security | Valid and invalid login testing | Tested |
| FR-02 | Administrators can register and manage players | Functional | Accuracy, Usability | Player registration testing | Issue #2 fix reported as merged; verification pending |
| FR-03 | Administrators can register and manage volunteers | Functional | Accuracy, Usability | Volunteer management testing | Tested |
| FR-04 | Authorised users can allocate players to teams | Functional | Correctness | Team allocation testing | Tested |
| FR-05 | Coaches or administrators can record attendance | Functional | Reliability, Accuracy | Attendance recording testing | Automated tests to be added |
| FR-06 | Users can view reports and dashboards | Functional | Performance, Usability | Report and dashboard testing | Testing in progress |
| FR-07 | The system provides relevant notifications | Functional | Reliability | Notification testing | Testing pending |

| NFR-01 | Sensitive information is protected from unauthorised access | Non-functional | Security | Role-based access testing | Further security testing planned |
| NFR-02 | The system responds within an acceptable time | Non-functional | Performance | Performance testing | Testing planned |
| NFR-03 | The interface is simple and easy to understand | Non-functional | Usability | User acceptance testing | Testing planned |
| NFR-04 | Records remain accurate and consistent | Non-functional | Reliability | Data integrity testing | Further testing planned |
| NFR-05 | The system can support increasing numbers of users and records | Non-functional | Scalability | Scalability testing | Testing planned |
| NFR-06 | The system can be updated and maintained efficiently | Non-functional | Maintainability | Code and documentation review | Ongoing |

