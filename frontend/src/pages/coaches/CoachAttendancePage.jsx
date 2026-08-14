// AttendanceController exists on the backend but has no endpoints implemented
// yet - it's still a stub (see backend/SportsClubApi/Controllers/AttendanceController.cs).
// This page can't do anything real until endpoints along these lines exist:
//   GET  /api/attendance?teamId={id}&date={date}  - existing records for a session
//   POST /api/attendance                            - { playerId, sessionDate, isPresent, notes }
// The Attendance model already has the right shape (PlayerId, SessionDate,
// IsPresent, Notes), so a controller mirroring PlayersController's pattern
// would unblock this page - it would then look like TeamRosterView with a
// present/absent checkbox per player instead of a static table.
function CoachAttendancePage() {
  return (
    <div className="page">
      <h1>Attendance</h1>
      <p className="hint">
        The backend's attendance endpoints aren't implemented yet, so this page can't
        mark or view attendance. Once <code>AttendanceController</code> has working
        GET/POST endpoints, this page can be built out to show a team roster with a
        present/absent checkbox per player.
      </p>
    </div>
  )
}

export default CoachAttendancePage
