import { useEffect, useState } from 'react'
import { getPlayerTotals, getTeamStats } from '../api/stats'
import { getPlayers } from '../api/players'
import { useAuth } from '../context/AuthContext'

// Team record + player totals. Shared by the read-only Stats page (every
// role) and the Admin stats page (which shows it under the entry form).
// `refreshKey` lets the admin page re-fetch after saving.
//
// A Player sees their own line pulled out on top. As with the attendance
// page there's no formal User-to-Player link, so "mine" is the player record
// whose email matches the logged-in account.
function StatsTables({ refreshKey = 0 }) {
  const { user } = useAuth()
  const [team, setTeam] = useState(null)
  const [totals, setTotals] = useState([])
  const [myPlayerId, setMyPlayerId] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => {
    const loads = [getTeamStats(), getPlayerTotals()]
    if (user?.role === 'Player') loads.push(getPlayers())

    Promise.all(loads)
      .then(([teamData, totalsData, players]) => {
        setTeam(teamData)
        setTotals(totalsData)
        setMyPlayerId(players?.find((p) => p.email === user.email)?.id ?? null)
      })
      .catch((err) => setError(err.message))
  }, [refreshKey, user?.role, user?.email])

  if (error) return <p className="error">Failed to load: {error}</p>
  if (!team) return <p>Loading...</p>

  const mine = totals.find((t) => t.playerId === myPlayerId)
  // The player's own row goes first; everyone else keeps the server's order.
  const rows = mine ? [mine, ...totals.filter((t) => t !== mine)] : totals

  return (
    <>
      {user?.role === 'Player' && (
        <>
          <h2>My stats</h2>
          {mine ? (
            <div className="stat-row">
              <div className="stat-card">
                <span>{mine.matches}</span>Matches
              </div>
              <div className="stat-card">
                <span>{mine.goals}</span>Goals
              </div>
              <div className="stat-card">
                <span>{mine.assists}</span>Assists
              </div>
            </div>
          ) : (
            <p className="hint">No player record matches your account email, so there are no stats to show.</p>
          )}
        </>
      )}

      <h2>Team record</h2>
      <div className="stat-row">
        <div className="stat-card">
          <span>{team.matchesPlayed}</span>Played
        </div>
        <div className="stat-card">
          <span>
            {team.wins}-{team.draws}-{team.losses}
          </span>
          W-D-L
        </div>
        <div className="stat-card">
          <span>
            {team.goalsFor}:{team.goalsAgainst}
          </span>
          Goals for:against
        </div>
        <div className="stat-card">
          <span>{team.upcomingMatches}</span>Upcoming matches
        </div>
      </div>

      <h2>Player stats</h2>
      <table>
        <thead>
          <tr>
            <th>Player</th>
            <th>Matches</th>
            <th>Goals</th>
            <th>Assists</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((t) => (
            <tr key={t.playerId}>
              <td>
                {t.fullName}
                {t.playerId === myPlayerId ? ' (you)' : ''}
              </td>
              <td>{t.matches}</td>
              <td>{t.goals}</td>
              <td>{t.assists}</td>
            </tr>
          ))}
          {totals.length === 0 && (
            <tr>
              <td colSpan={4}>No players yet.</td>
            </tr>
          )}
        </tbody>
      </table>
    </>
  )
}

export default StatsTables
