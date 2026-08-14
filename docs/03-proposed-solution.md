# Proposed Solution

## Overview
The proposed solution is a web-based Player and Volunteer Management System that replaces the club's current mix of spreadsheets, emails, and manual process with a single, centralized platform.

The system targets the issues identified in the Problem Definition. A centralised database with entry-point validation removes duplicate player records. Rule-based team allocation replaces the manual, error-prone assignment process. Structured volunteer profiles with required-field validation ensure volunteer information is complete and accessible. Automated reporting replaces manual data aggregation, and role-based workflows reduce the overall administrative burden on club staff.

The system supports four user roles: Players, Volunteers, Coaches, and Administrators. Each role has access appropriate to their needs, supporting both usability and data security.

The prototype is build with React (Vite) for the frontend and ASP.NET Core Web API with SQLite for the backend, using sample data to represent a small-to-medium sized club.

## Player Registration 
This feature directly addresses the duplicate player records and wrong team allocation issues identified in the Problem Definition. Currently, player data is entered manually across multiple spreadsheets with no shared validation, leading to duplicate entries and inconsistent information reaching coaches and administrators.

The registration form allows player (or parents/guardians, where the player is a minor) to submit their details online. The system validates entries at the point of submission before the record is saved to the central database. Where the registrant is under 18, the form additionally requires parent/guardian contact information as a mandatory field. Each registration also captures preferred sport and age category, which feeds directly into automated team allocation.

By validating data once at the point of entry and storing it in a single database, the system removes the duplication etries problem at its source rather than relying on manual cross-checking afterwards.

**Related quality attributes:** data accuracy(validation and duplicate detection at entry), usability (guided form reduces entry errors), reliability (single source of truth for player records).

## Volunteer Management
This feature addresses the problem of volunteer information going missing or being incomplete, as identified in the Problem Definition. Volunteer details are currently collected through emails and spreadsheets with no consistent format or required fields, meaning coaches and administrators often lack updated information about volunteer roles and availivility.

The system provides a structured volunteer profile form covering contact details, assigned role, and availability. Required-filed validation ensures a profile cannot be saved as incomplete. Administrators and coaches can view a centralized, searchable list of volunteers, replacing the need to track this information across separate documents.

**Related quality attributes:** reliablity (required-field validation prevents incomplete records), usability (centralized, searchable volunteer list for coaches and admins).

## Team Allocation
This feature addresses the problem of players ending up on the wrong team due to the lack of a shared, validated data sources, as identified in the Problem Definition. Team assignment is currently done manually, often cross-referencing multiple spreadsheets, which creates opportunities for player to be placed in an incorrect age gropu or team.

The system uses the age category and preferred sport captured during Player Registration on automatically suggest a team allocation based on predefined rules. Administrators can review and manually adjust allocations before they are confirmed, keeping a human check in the process while removing the manual cross-referencing work. Coaches can view a latest roster for each team, generated directly from the central database rather than a separately maintained list.

**Related quality attributes:** data accuracy (allocation draws directly from validated registration data), reliability (rule-based allocation reduces manual assignment errors), maintainability (allocation rules are defined in one place and can be updated as club needs change).

## Attendance Tracking
This feature supports the slow reporting problem identified in the Problem Definition. A key reason reporting is currently slow is that attendance is recorded on paper or in separate spreadsheets, and someone has to manually collect and total this data before it can be reported. Recording attendance digitally at the poiont it happens removes this manual aggregation step entirely.

Coaches can mark attendance for each training session or match against the relevant team roster, generated from Team Allocation. Each attendance record is saved immediately to the central database, making it available for reporting without any additional data entry. Administrators and coaches can also view a player's or team's attendance history over time.

**Related quality attributes:** performance (removes the manual aggregation step, allowing faster report generation), reliability (attendance is recorded at the point it occurs, avoiding errors from reconstructing records later), usability (attendance can be marked quickly against an existing roster).
## Reports&Dashboard
This feature directly addresses the slow reproting problem identified in the Problem Definition. Administrations currently have to manually pull number together from separate spradsheets and emails, which is time-consuming and delays visibility into how the club is running.

Because player registration, team allocation, volunteer information, and attendance are all stored in a single entral database, the dashboard can generate reports automatically rather than requiring manual aggreation. The dashboard displays key metrics such as total registered players, players per team, attendance ratess, and volunteer coverage, with the option to filter by team or date range. The dashboard is required to load these summary metrics within three seconds for a sample dataset of at least 500 records.

**Related quality attributes:** performamce (defined load-time target for a realistic dataset size), usability (key metrics visible at a glance without manual calculation), maintainability (a single data source means reporting logic only needs to be updated in one place).

## Notification

While not one of the core problems listed in the Problem Definition, this feature supports the stakeholder need identified in the Background section: volunteers needing clear, up-to-date information about their roles and schedules. Without a notification mechanism, staff must rely on separately checking the system or being emailed manually, which adds to the administrative workload the project aims to reduce.

The prototype implements in-app notifications only, alerting volunteers and coaches to schedule changes, new role assignments, or upcoming sessions. Email or SMS notifications are considered out of scope for this prototype, given the semester timeframe and the need to avoid dependencies on external services with real data.

**Related quality attributes:** usability (reduces reliance on manual follow-up), reliability (notifications generated directly from system events, avoiding missed manual communication).

## Secure Login

Secure login underpins the role-based access described throughout this document. With four distinct user roles - Players/Parents, Volunteers, Coaches, and Administrators - each needing access to different information, a shared login without role separation would risk exposing sensitive data (such as volunteer contact details or player records) to users who should not see it.

The prototype implements authentication using JWT (JSON Web Tokens). On login, users receive a token identifying their role, which determines what data and actions they can access - for example, only Administrators can view the full dashboard, and only Coaches can mark attendance for their own team.

**Related quality attributes:** security (role-based access control limits data exposure), reliability (consistent enforcement of access rules across the system).