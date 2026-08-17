# Project Progress

## 1. Work Completed So Far

The project has progressed through documentation, implementation, and testing, with each stage building on the requirements and quality artifacts established earlier in the project. The problem the system addresses has been documented in `01-problem-definition.md`, and the functional and non-functional requirements, along with the associated quality analysis, are recorded in `02-requirements.md`. The proposed solution, covering the intended features and architecture, is described in `03-proposed-solution.md`, and the approach to testing the system is set out in `04-test-strategy.md`. Eight test cases were designed against this strategy and have since been executed manually, with the steps, expected results, and outcomes recorded in `05-test-cases.md`. A requirements traceability matrix linking requirements to their corresponding quality expectations and testing activities was created in `06-traceability-matrix.md`, and use of AI tools throughout development has been tracked in `08-ai-usage-log.md`.

On the implementation side, the backend has been built as an ASP.NET Core Web API, with domain models covering Players, Volunteers, Teams, and Attendance, backed by a SQLite database accessed through Entity Framework Core. Authentication is implemented using JWT bearer tokens, and API endpoints enforce role-based access control so that only appropriately authorised users can perform sensitive actions such as creating or updating records. The frontend has been built with React and Vite, and includes role-based routing so that Admin, Player, Volunteer, and Coach users are directed to the pages relevant to their role, along with an Admin dashboard that displays live counts of players, volunteers, and teams drawn from the API.

Automated testing has also been put in place: an xUnit test project exercises the backend end to end using `WebApplicationFactory` and an in-memory database, covering TC-01 through TC-08, and all nine automated tests currently pass. In addition to this automated coverage, all eight test cases from `05-test-cases.md` have been executed manually against the running application. All eight passed, though two defects were identified during this round of testing and have been logged as GitHub Issues for tracking and follow-up.

## 2. Current Limitations, Risks and Challenges

Several parts of the system are not yet complete. The Attendance feature exists only as a stub on the backend and has no supporting frontend functionality, and notifications have not been implemented at all, meaning two features described in the proposed solution are still outstanding. On the Teams side, editing and deleting existing team records is not yet supported by either the API or the interface, which limits how administrators can correct or maintain team data once it has been created.

Manual testing surfaced two defects that remain open. The first (GitHub Issue #1) is that the Players list displays a team's numeric ID rather than its name in the Team column, which makes the data harder to interpret at a glance even though the underlying association is correct. The second (GitHub Issue #2) is that players registered through the self-service Register page are not appearing in the Players list as expected. Both are being tracked for resolution in the next phase of work.

There is also a usability limitation in the current authentication implementation: the JWT is kept in memory rather than persisted to storage, which means that refreshing the browser logs the user out. This was a deliberate trade-off during initial development but will need to be revisited before the system is considered production-ready.

Beyond the technical gaps, the team is managing the ordinary logistical challenge of three members working across different personal schedules, which affects how quickly work can be reviewed, integrated, and tested collaboratively.

## 3. Risk Management Plan

The two open defects, Issue #1 and Issue #2, are prioritised to be fixed in the next phase of development, ahead of further feature work, since both affect data that administrators rely on when managing players and teams. The Attendance and Notifications features, being the largest remaining gaps against the proposed solution, are planned for implementation in the final phase of the project, once the existing defects have been resolved and the core CRUD functionality is stable.

To manage the risk introduced by the team's differing schedules, the group will hold weekly check-ins to review progress, redistribute work where needed, and surface blockers early rather than close to project deadlines. Individual contributions will continue to be tracked through GitHub commit history, which provides a transparent and verifiable record of who implemented which parts of the system and supports fair assessment of each member's involvement.

## 4. Plan for Completing the Final Project

The remaining work is planned in the following order. First, the two identified defects will be fixed and retested to confirm the Players list correctly displays team names and that self-registered players appear as expected. Second, the Attendance feature will be completed, extending the existing stub controller into full functionality with a corresponding frontend interface. Third, a Notifications feature will be added, giving relevant users visibility of updates such as team allocation or attendance changes.

Once these features are in place, test coverage will be expanded beyond the current eight test cases to include edge cases and regression testing, ensuring that new functionality has not broken previously verified behaviour. Additional non-functional testing will also be carried out, focusing on security (confirming role-based access remains correctly enforced as new endpoints are added) and performance (confirming the system continues to respond acceptably as data volumes grow). The project will conclude with the preparation of the final project report, summarising the work completed, the testing carried out, and the outcomes achieved against the original requirements.

## 5. Brief Team Reflection

AI tools have played a meaningful role in the project so far, with Claude Code used for scaffolding the backend and frontend structure, generating the automated xUnit test suite, and assisting with manual test execution and defect documentation, as recorded in `08-ai-usage-log.md`. GitHub Copilot is planned for use in the upcoming feature implementation work, particularly for the Attendance and Notifications features, to support faster and more consistent code generation while still requiring the team to review and validate any AI-assisted output before it is merged.

Team collaboration has been managed primarily through Discord for day-to-day communication and coordination, and through GitHub for version control, issue tracking, and code review. This combination has allowed the team to keep discussion and decision-making separate from the project's technical history, while GitHub's commit history continues to serve as evidence of each member's individual contribution to the project.
