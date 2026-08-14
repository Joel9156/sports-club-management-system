import { useEffect, useState } from 'react'
import { getTeams } from '../api/teams'
import { getPlayers } from '../api/players'

// Shared by the Player "My Team" page and the Coach "Roster" page.
//
// There's no link yet between a logged-in User account and a specific Player
// or Team record (the User model has no PlayerId/TeamId), so this can't
// automatically scope to "my team" - it lets the signed-in user pick any
// team from a dropdown and see that roster instead. Wiring up real ownership
// would need a backend change (see backend/README.md's auth scoping note).
function TeamRosterView({ title }) {
  const [teams, setTeams] = useState([])
  const [players, setPlayers] = useState([])
  const [selectedTeamId, setSelectedTeamId] = useState('')
  const [error, setError] = useState(null)

  useEffect(() => {
    Promise.all([getTeams(), getPlayers()])
      .then(([teamsData, playersData]) => {
        setTeams(teamsData)
        setPlayers(playersData)
      })
      .catch((err) => setError(err.message))
  }, [])

  if (error) return <p className="error">Failed to load: {error}</p>

  const roster = players.filter((p) => String(p.teamId) === String(selectedTeamId))

  return (
    <div className="page">
      <h1>{title}</h1>
      <label>
        Team
        <select value={selectedTeamId} onChange={(e) => setSelectedTeamId(e.target.value)}>
          <option value="">Select a team</option>
          {teams.map((t) => (
            <option key={t.id} value={t.id}>
              {t.name} ({t.ageGroup})
            </option>
          ))}
        </select>
      </label>
      {selectedTeamId && (
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Active</th>
            </tr>
          </thead>
          <tbody>
            {roster.map((p) => (
              <tr key={p.id}>
                <td>{p.fullName}</td>
                <td>{p.email}</td>
                <td>{p.isActive ? 'Yes' : 'No'}</td>
              </tr>
            ))}
            {roster.length === 0 && (
              <tr>
                <td colSpan={3}>No players on this team yet.</td>
              </tr>
            )}
          </tbody>
        </table>
      )}
    </div>
  )
}

export default TeamRosterView
