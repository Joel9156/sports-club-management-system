import { useEffect, useState } from 'react'
import { getPlayers } from '../../api/players'
import { getVolunteers } from '../../api/volunteers'

// There's no dedicated dashboard/metrics endpoint on the backend, so these
// numbers are computed client-side from the existing list endpoints. That's
// fine at prototype scale, but docs/03-proposed-solution.md's "load within 3
// seconds at 500+ records" target would be safer served by a real aggregate
// endpoint (computed server-side) as data grows - flagging that as a backend
// follow-up rather than building it here.
function AdminDashboardPage() {
  const [players, setPlayers] = useState([])
  const [volunteers, setVolunteers] = useState([])
  const [error, setError] = useState(null)

  useEffect(() => {
    Promise.all([getPlayers(), getVolunteers()])
      .then(([p, v]) => {
        setPlayers(p)
        setVolunteers(v)
      })
      .catch((err) => setError(err.message))
  }, [])

  if (error) return <p className="error">Failed to load: {error}</p>

  const activePlayers = players.filter((p) => p.isActive).length
  const activeVolunteers = volunteers.filter((v) => v.isActive).length

  return (
    <div className="page">
      {/* No page-specific banner here - the club background image is applied
          app-wide on body (see index.css) instead. */}
      <h1>Admin Dashboard</h1>
      <div className="stat-row">
        <div className="stat-card">
          <span>{players.length}</span>
          Players ({activePlayers} active)
        </div>
        <div className="stat-card">
          <span>{volunteers.length}</span>
          Volunteers ({activeVolunteers} active)
        </div>
      </div>
    </div>
  )
}

export default AdminDashboardPage
