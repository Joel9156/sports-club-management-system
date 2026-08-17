# Requirements Traceability Matrix

| Requirement ID | Requirement | Test Case ID | Test Case |
|----------------|-------------|--------------|-----------|
| FR-01 | Users can securely log in | TC-01, TC-02 | Valid login, Invalid login |
| FR-02 | Administrators can register and manage players | TC-03, TC-04 | Register valid player, Missing required field |
| FR-03 | Administrators can register and manage volunteers | TC-05 | Register valid volunteer |
| FR-04 | Authorised users can allocate players to teams | TC-06 | Assign player to team |
| FR-05 | Coaches or administrators can record attendance | - | Not yet tested (stub) |
| FR-06 | Users can view reports and dashboards | TC-08 | Dashboard reflects correct data |
| FR-07 | The system provides relevant notifications | - | Not yet implemented |
| NFR-01 | Sensitive information is protected from unauthorised access | TC-07 | Non-admin cannot access admin features |
| NFR-02 | The system responds within an acceptable time | - | To be tested in later phase |
| NFR-03 | The interface is simple and easy to understand | TC-08 | Dashboard reflects correct data |
| NFR-04 | Records remain accurate and consistent | TC-03, TC-05, TC-06 | Player registration, Volunteer registration, Team allocation |
| NFR-05 | The system can support increasing numbers of users | - | To be tested in later phase |
| NFR-06 | The system can be updated and maintained efficiently | - | Code review and documentation |

Traceability between requirements and test cases underpins quality assurance by ensuring every stated requirement has a corresponding verification point, making gaps in coverage (such as FR-05, FR-07, NFR-02, and NFR-05 above) explicit rather than hidden. This matrix also supports change management: when a requirement changes, the linked test cases can be quickly identified and updated, and when a test fails, the affected requirement is immediately traceable, reducing the risk of regressions being missed as the system evolves.
