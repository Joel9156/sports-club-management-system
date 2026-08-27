# Test Strategy

## 1. Purpose

The purpose of this test strategy is to ensure that the Community Sports Player and Volunteer Management System works correctly, reliably, and securely. Testing will be used to identify defects, verify the system requirements, and confirm that the main features meet the defined acceptance criteria.

The testing process will focus on important functions such as player registration, volunteer management, team allocation, attendance tracking, reports and dashboards, notifications, and user authentication.

## 2. Scope of Testing

Testing will cover the main functions of the system, including:

* User login and authentication
* Player registration and management
* Volunteer registration and management
* Team allocation
* Attendance tracking
* Reports and dashboards
* Notifications
* Input and data validation
* Role-based access
* Data accuracy and integrity

Testing will include both functional and non-functional requirements.

## 3. Testing Approach

The project will use different levels of testing to identify defects throughout development. Testing will include unit testing, integration testing, system testing, and user acceptance testing.

### 3.1 Unit Testing

Unit testing will be used to test individual functions or components separately.

Examples include:

* Checking player registration validation
* Checking required fields
* Checking volunteer information validation
* Checking login validation
* Checking team allocation logic
* Checking attendance recording functions

The purpose of unit testing is to identify problems in individual parts of the system before they are combined with other components.

### 3.2 Integration Testing

Integration testing will check whether different parts of the system communicate and work correctly together.

Examples include:

* Player registration form communicating with the database
* Volunteer registration information being stored correctly
* Player records connecting correctly with team allocations
* Attendance records connecting to the correct player
* Dashboard information being retrieved from the database

Integration testing will help identify problems that occur when different components of the system interact.

### 3.3 System Testing

System testing will evaluate the complete application after the major components have been integrated.

The team will test complete user processes, such as:

* Logging into the system
* Registering a player
* Registering a volunteer
* Allocating a player to a team
* Recording attendance
* Viewing reports and dashboards
* Receiving or viewing notifications

The purpose is to confirm that the complete system works according to the documented requirements.

### 3.4 User Acceptance Testing

User acceptance testing will determine whether the system meets the needs of its intended users.

Users or team members acting as representative users will complete common tasks and compare the results against the acceptance criteria defined in the requirements document.

Examples include:

* Registering a new player successfully
* Updating volunteer information
* Allocating a player to the correct team
* Recording and viewing attendance
* Viewing relevant information through reports and dashboards

A feature will be considered acceptable when its agreed acceptance criteria have been satisfied.

## 4. Functional Testing

Functional testing will verify that the system performs the functions described in the functional requirements.

| Feature              | Test                                 | Expected Result                                              |
| -------------------- | ------------------------------------ | ------------------------------------------------------------ |
| User Login           | Enter valid login details            | User successfully logs in                                    |
| User Login           | Enter invalid login details          | Login is rejected and an appropriate message is displayed    |
| Player Registration  | Submit valid player information      | Player record is successfully created                        |
| Player Registration  | Leave a required field empty         | System prevents submission and displays a validation message |
| Player Registration  | Attempt to create a duplicate player | System identifies or prevents the duplicate record           |
| Volunteer Management | Add valid volunteer information      | Volunteer record is successfully created                     |
| Team Allocation      | Assign a player to a valid team      | Player appears in the selected team                          |
| Attendance           | Record player attendance             | Attendance is saved against the correct player               |
| Reports              | Request player or team information   | Correct information is displayed                             |
| Notifications        | Trigger an applicable notification   | Relevant notification is generated or displayed              |

## 5. Non-Functional Testing

Non-functional testing will evaluate the quality of the system rather than only checking individual features.

### 5.1 Security Testing

Security testing will check that:

* Users must authenticate before accessing protected areas.
* Invalid login attempts are rejected.
* Users can only access information and functions permitted by their role.
* Sensitive player and volunteer information is protected from unauthorised access.

### 5.2 Performance Testing

Performance testing will check that:

* Common pages load within an acceptable amount of time.
* Player and volunteer information can be retrieved efficiently.
* Reports and dashboards respond within an acceptable amount of time under normal use.

### 5.3 Usability Testing

Usability testing will check that:

* Navigation is clear and understandable.
* Forms are easy to complete.
* Required fields are clearly identified.
* Validation and error messages are understandable.
* Users can complete common tasks without unnecessary steps.

### 5.4 Reliability and Data Integrity Testing

Testing will confirm that:

* Saved records remain available after normal system operations.
* Player and volunteer information is stored accurately.
* Duplicate records are minimised.
* Team and attendance information remains linked to the correct player.
* Updating information does not unintentionally remove or corrupt other records.

## 6. Test Environment

Testing will be carried out using the development version of the web-based system.

The test environment will include:

* Frontend application
* Backend services
* Project database
* Supported web browser
* Test user accounts
* Sample player, volunteer, team, and attendance data

Test data should be used instead of sensitive real-world personal information wherever possible.

## 7. Defect Management

Any defects identified during testing should be recorded so that the team can track, fix, and retest them.

Each defect should include:

* Defect title
* Date identified
* Feature affected
* Description of the problem
* Steps to reproduce the problem
* Expected result
* Actual result
* Severity
* Status
* Person responsible

Possible defect statuses include:

* Open
* In Progress
* Fixed
* Retest
* Closed

Defects may be tracked using GitHub Issues or another agreed project tracking method.

## 8. Defect Severity

Defects can be classified according to their impact.

### Critical

A defect that prevents an essential system function from working or creates a serious security or data integrity problem.

### High

A major feature does not work correctly, but other parts of the system can still be used.

### Medium

A feature works but contains a problem that affects normal use.

### Low

A minor issue that does not prevent the user from completing the task, such as a small interface or formatting problem.

## 9. Entry Criteria

Testing of a feature can begin when:

* The relevant requirement has been documented.
* Acceptance criteria have been defined.
* The feature has been implemented sufficiently for testing.
* The required test environment is available.
* Necessary test data has been prepared.

## 10. Exit Criteria

Testing for a feature can be considered complete when:

* Relevant functional requirements have been tested.
* Relevant acceptance criteria have been satisfied.
* Critical defects have been resolved.
* High-severity defects have been fixed or formally reviewed by the team.
* Fixed defects have been retested.
* Test results have been recorded.

## 11. Test Evidence

Evidence of testing will be maintained throughout the project. This may include:

* Test case results
* Screenshots
* GitHub Issues
* Defect records
* Automated test results
* Manual test results
* Git commit history
* Retest results

This evidence will demonstrate that testing was carried out continuously and that identified defects were addressed during development.

## 12. Roles and Responsibilities

All team members are responsible for contributing to software quality.

* **Developers:** Perform unit testing and fix defects in their implemented features.
* **Testers/team members:** Perform functional, integration, and system testing.
* **Requirements and quality role:** Check that tests are linked to requirements and acceptance criteria.
* **Project team:** Review major defects and confirm whether the system is ready for final acceptance testing.

## 13. Requirements Traceability

Testing should be linked to the requirements and acceptance criteria defined in `02-requirements.md`.

Each important requirement should have at least one corresponding test. This will help the team demonstrate that the implemented system has been tested against the original project requirements.

For example:

| Requirement            | Related Test                                |
| ---------------------- | ------------------------------------------- |
| Player registration    | Valid and invalid player registration tests |
| Volunteer management   | Add and update volunteer tests              |
| Team allocation        | Player-to-team allocation test              |
| Attendance tracking    | Record and retrieve attendance test         |
| User authentication    | Valid and invalid login tests               |
| Security               | Role-based access tests                     |
| Reports and dashboards | Data display and retrieval tests            |

## 14. Current Testing Progress

Initial testing has been carried out on authentication, player registration, volunteer management, team allocation, dashboard functionality and role-based access.

Both positive and negative test scenarios have been used. For example, login was tested with valid and invalid credentials, while player registration was tested with valid information and missing required fields.

Testing has also identified defects in the current prototype. One issue was found where the team name was displayed as an ID after team allocation. Another issue was found where players registered through the registration page were not correctly reflected in the dashboard player count. These defects have been recorded using GitHub Issues so they can be tracked, fixed and retested.

Attendance, notifications, performance and scalability require further testing as the prototype develops.
