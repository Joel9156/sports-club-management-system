import { useEffect, useState } from 'react'
import { getPlayers } from '../api/players'

// Shared by the Player "Roster" page and the Coach "Roster" page.
//
// Mt Eden FC is a single-team club, so this just lists every registered
// player - there's no team to pick between.
function TeamRosterView({ title }) {
  const [players, setPlayers] = useState([])
  const [error, setError] = useState(null)

  useEffect(() => {
    getPlayers()
      .then(setPlayers)
      .catch((err) => setError(err.message))
  }, [])

  if (error) return <p className="error">Failed to load: {error}</p>

  return (
    <div className="page">
      <h1>{title}</h1>
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Active</th>
          </tr>
        </thead>
        <tbody>
          {players.map((p) => (
            <tr key={p.id}>
              <td>{p.fullName}</td>
              <td>{p.email}</td>
              <td>{p.isActive ? 'Yes' : 'No'}</td>
            </tr>
          ))}
          {players.length === 0 && (
            <tr>
              <td colSpan={3}>No players registered yet.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  )
}

export default TeamRosterView
