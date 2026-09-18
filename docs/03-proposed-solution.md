# Proposed Solution

## Overview
The proposed solution is a web-based Player and Volunteer Management System that replaces the club's current mix of spreadsheets, emails, and manual process with a single, centralized platform.

The prototype is based on Mt Eden FC, a real local club, to make the design more realistic and reflect how a local club might actually operate. However, all data used in the system is sample or test data and does not include any real player or volunteer information. This is a course prototype only and is not an official system or endorsed by the club.

The system targets the issues identified in the Problem Definition. A centralised database with entry-point validation removes duplicate player records. Structured volunteer profiles with required-field validation ensure volunteer information is complete and accessible. Automated reporting replaces manual data aggregation, and role-based workflows reduce the overall administrative burden on club staff. (Team allocation was originally planned here too, but is no longer part of the prototype's scope - see the Team Allocation section for why.)

The system supports four user roles: Players, Volunteers, Coaches, and Administrators. Each role has access appropriate to their needs, supporting both usability and data security.

The prototype is build with React (Vite) for the frontend and ASP.NET Core Web API with SQLite for the backend, using sample data to represent a small-to-medium sized club.

A web-based GUI was chosen instead of a desktop application or command-line tool because the system needs to be accessible to four different user roles through a regular web browser. This means users can access the system from different devices without having to install any software, which also supports the usability and accessibility requirements identified during the requirements analysis.

## Player Registration 
This feature directly addresses the duplicate player records and wrong team allocation issues identified in the Problem Definition. Currently, player data is entered manually across multiple spreadsheets with no shared validation, leading to duplicate entries and inconsistent information reaching coaches and administrators.

The registration form allows player (or parents/guardians, where the player is a minor) to submit their details online. The system validates entries at the point of submission before the record is saved to the central database. Where the registrant is under 18, the form additionally requires parent/guardian contact information as a mandatory field. (Preferred sport and age category are not captured by the current `Player` model or registration form - see the Team Allocation section below for why team-based fields are no longer part of this feature's scope. Guardian contact information for minors is likewise not yet implemented, independent of that scope change - see `frontend/src/pages/players/PlayerRegisterPage.jsx`.)

By validating data once at the point of entry and storing it in a single database, the system removes the duplication etries problem at its source rather than relying on manual cross-checking afterwards.

**Related quality attributes:** data accuracy(validation and duplicate detection at entry), usability (guided form reduces entry errors), reliability (single source of truth for player records).

## Volunteer Management
This feature addresses the problem of volunteer information going missing or being incomplete, as identified in the Problem Definition. Volunteer details are currently collected through emails and spreadsheets with no consistent format or required fields, meaning coaches and administrators often lack updated information about volunteer roles and availivility.

The system provides a structured volunteer profile form covering contact details, assigned role, and availability. Required-filed validation ensures a profile cannot be saved as incomplete. Administrators and coaches can view a centralized, searchable list of volunteers, replacing the need to track this information across separate documents.

**Related quality attributes:** reliablity (required-field validation prevents incomplete records), usability (centralized, searchable volunteer list for coaches and admins).

## Team Allocation
This feature originally addressed the problem of players ending up on the wrong team due to the lack of a shared, validated data source, as identified in the Problem Definition. Team assignment was previously done manually, often cross-referencing multiple spreadsheets, which created opportunities for a player to be placed in an incorrect age group or team.

**Scope change:** during development, the team confirmed that Mt Eden FC - the real local club this prototype is modelled on (see Overview) - runs as a single team rather than multiple age-group teams. Maintaining a separate team-allocation workflow (team creation/editing, per-player team assignment, a team-filtered roster) added administrative surface area that didn't correspond to any decision a Mt Eden FC administrator actually needs to make, and it diverted testing effort away from features that do reflect the club's real needs (Attendance, Notifications). Commit `0b2f4b9` ("Remove team management feature and simplify attendance/roster views") removed the dedicated team-management admin page, team selection from player registration, and team filtering from the roster and attendance views; every registered player now appears on a single club-wide roster instead.

The backend's `Team` model, `TeamsController` (`GET`/`POST` only - it never had `PUT`/`DELETE`), and `Player.TeamId` foreign key remain in the codebase but are no longer used by the frontend. They were left in place rather than deleted outright so that multi-team support could be rebuilt on that existing structure if the system were later adapted to a club that does run several age-group teams. This is a deliberate scope reduction to match the actual target club, not an oversight - it should be read alongside the Problem Definition's "players ending up on the wrong team" issue as no longer a live problem for Mt Eden FC specifically, even though it remains a real problem for multi-team clubs in general.

**Related quality attributes:** maintainability (removing an unused workflow reduces the surface area that needs to stay consistent and tested), usability (a single roster removes an unnecessary selection step for a club that only has one team).

## Attendance Tracking
This feature supports the slow reporting problem identified in the Problem Definition. A key reason reporting is currently slow is that attendance is recorded on paper or in separate spreadsheets, and someone has to manually collect and total this data before it can be reported. Recording attendance digitally at the poiont it happens removes this manual aggregation step entirely.

Coaches can mark attendance for each training session or match against the club roster, generated directly from Player Registration (see the Team Allocation section for why this is now a single club-wide roster rather than per-team). Each attendance record is saved immediately to the central database, making it available for reporting without any additional data entry. Administrators and coaches can also view a player's attendance history over time.

**Related quality attributes:** performance (removes the manual aggregation step, allowing faster report generation), reliability (attendance is recorded at the point it occurs, avoiding errors from reconstructing records later), usability (attendance can be marked quickly against an existing roster).

## Reports&Dashboard
This feature directly addresses the slow reproting problem identified in the Problem Definition. Administrations currently have to manually pull number together from separate spradsheets and emails, which is time-consuming and delays visibility into how the club is running.

Because player registration, volunteer information, and attendance are all stored in a single central database, the dashboard can generate reports automatically rather than requiring manual aggregation. The dashboard displays key metrics such as total registered players, attendance rates, and volunteer coverage, with the option to filter by date range (per-team filtering is no longer applicable - see the Team Allocation section). The dashboard is required to load these summary metrics within three seconds for a sample dataset of at least 500 records.

**Related quality attributes:** performamce (defined load-time target for a realistic dataset size), usability (key metrics visible at a glance without manual calculation), maintainability (a single data source means reporting logic only needs to be updated in one place).

## Notification

While not one of the core problems listed in the Problem Definition, this feature supports the stakeholder need identified in the Background section: volunteers needing clear, up-to-date information about their roles and schedules. Without a notification mechanism, staff must rely on separately checking the system or being emailed manually, which adds to the administrative workload the project aims to reduce.

The prototype implements in-app notifications only, alerting volunteers and coaches to schedule changes, new role assignments, or upcoming sessions. Email or SMS notifications are considered out of scope for this prototype, given the semester timeframe and the need to avoid dependencies on external services with real data.

**Related quality attributes:** usability (reduces reliance on manual follow-up), reliability (notifications generated directly from system events, avoiding missed manual communication).

## Secure Login

Secure login underpins the role-based access described throughout this document. With four distinct user roles - Players/Parents, Volunteers, Coaches, and Administrators - each needing access to different information, a shared login without role separation would risk exposing sensitive data (such as volunteer contact details or player records) to users who should not see it.

The prototype implements authentication using JWT (JSON Web Tokens). On login, users receive a token identifying their role, which determines what data and actions they can access - for example, only Administrators can view the full dashboard, and only Coaches (and Administrators) can mark attendance.

**Related quality attributes:** security (role-based access control limits data exposure), reliability (consistent enforcement of access rules across the system).