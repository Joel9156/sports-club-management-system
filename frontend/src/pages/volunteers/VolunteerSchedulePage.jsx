import { useEffect, useState } from 'react'
import { getVolunteers } from '../../api/volunteers'

// There's no dedicated schedule/shift endpoint on the backend - the closest
// available data is each volunteer's free-text `availability` field, so this
// shows that instead of assigned shifts. A real schedule feature would need
// a new backend model (e.g. a Shift table keyed by date/session/volunteerId)
// and endpoints to match.
function VolunteerSchedulePage() {
  const [volunteers, setVolunteers] = useState([])
  const [error, setError] = useState(null)

  useEffect(() => {
    getVolunteers()
      .then(setVolunteers)
      .catch((err) => setError(err.message))
  }, [])

  if (error) return <p className="error">Failed to load: {error}</p>

  return (
    <div className="page">
      <h1>Volunteer Schedule</h1>
      <p className="hint">
        No dedicated schedule endpoint exists on the backend yet, so this shows each
        volunteer's declared availability instead of assigned shifts.
      </p>
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Role</th>
            <th>Availability</th>
          </tr>
        </thead>
        <tbody>
          {volunteers.map((v) => (
            <tr key={v.id}>
              <td>{v.fullName}</td>
              <td>{v.role}</td>
              <td>{v.availability}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export default VolunteerSchedulePage
