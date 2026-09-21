import { useEffect, useState } from 'react'
import { getEvents } from '../../api/events'
import { getPlayers } from '../../api/players'
import {
  getPlayerStats,
  createPlayerStat,
  updatePlayerStat,
  deletePlayerStat,
} from '../../api/stats'
import StatsTables from '../../components/StatsTables'

// Admin: record who played in a match and their goals/assists. Ticking
// "Played" and saving creates that player's stat row; unticking deletes it;
// changing the numbers updates it. The totals below refresh after each save.
function AdminStatsPage() {
  const [matches, setMatches] = useState([])
  const [players, setPlayers] = useState([])
  const [matchId, setMatchId] = useState('')
  const [rows, setRows] = useState({}) // playerId -> { played, goals, assists, statId }
  const [status, setStatus] = useState(null)
  const [error, setError] = useState(null)
  const [saving, setSaving] = useState(false)
  const [refreshKey, setRefreshKey] = useState(0)

  useEffect(() => {
    Promise.all([getEvents({ type: 'Match' }), getPlayers()])
      .then(([m, p]) => {
        setMatches(m)
        setPlayers(p)
      })
      .catch((err) => setError(err.message))
  }, [])

  // Build one editable row per player from whatever's already saved for the match.
  function loadRows(selectedMatchId, playerList = players) {
    return getPlayerStats({ eventId: selectedMatchId }).then((saved) => {
      const byPlayer = Object.fromEntries(saved.map((s) => [s.playerId, s]))
      setRows(
        Object.fromEntries(
          playerList.map((p) => {
            const s = byPlayer[p.id]
            return [
              p.id,
              s
                ? { played: true, goals: s.goals, assists: s.assists, statId: s.id }
                : { played: false, goals: 0, assists: 0, statId: null },
            ]
          }),
        ),
      )
    })
  }

  function handleMatchChange(e) {
    const id = e.target.value
    setMatchId(id)
    setStatus(null)
    setError(null)
    if (!id) {
      setRows({})
      return
    }
    loadRows(id).catch((err) => setError(err.message))
  }

  function setField(playerId, field, value) {
    setRows((r) => ({ ...r, [playerId]: { ...r[playerId], [field]: value } }))
  }

  async function handleSave() {
    setStatus(null)
    setError(null)
    setSaving(true)
    try {
      const ops = []
      for (const p of players) {
        const row = rows[p.id]
        const goals = Number(row.goals) || 0
        const assists = Number(row.assists) || 0
        const base = { playerId: p.id, scheduledEventId: Number(matchId), goals, assists }

        if (row.played && !row.statId) ops.push(createPlayerStat(base))
        else if (row.played) ops.push(updatePlayerStat(row.statId, { id: row.statId, ...base }))
        else if (row.statId) ops.push(deletePlayerStat(row.statId))
      }
      await Promise.all(ops)
      await loadRows(matchId)
      setRefreshKey((k) => k + 1)
      setStatus('success')
    } catch (err) {
      setError(err.response?.data?.message ?? err.response?.data?.title ?? err.message)
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="page">
      <h1>Manage Stats</h1>

      <label>
        Match
        <select value={matchId} onChange={handleMatchChange}>
          <option value="">Select a match</option>
          {matches.map((m) => (
            <option key={m.id} value={m.id}>
              {m.date} vs {m.opponent}
            </option>
          ))}
        </select>
      </label>
      {matches.length === 0 && <p className="hint">Add a match on the Schedule page first.</p>}

      {matchId && (
        <>
          <table>
            <thead>
              <tr>
                <th>Player</th>
                <th>Played</th>
                <th>Goals</th>
                <th>Assists</th>
              </tr>
            </thead>
            <tbody>
              {players.map((p) => {
                const row = rows[p.id]
                if (!row) return null
                return (
                  <tr key={p.id}>
                    <td>{p.fullName}</td>
                    <td>
                      <input
                        type="checkbox"
                        checked={row.played}
                        onChange={(e) => setField(p.id, 'played', e.target.checked)}
                      />
                    </td>
                    <td>
                      <input
                        type="number"
                        min="0"
                        value={row.goals}
                        disabled={!row.played}
                        onChange={(e) => setField(p.id, 'goals', e.target.value)}
                      />
                    </td>
                    <td>
                      <input
                        type="number"
                        min="0"
                        value={row.assists}
                        disabled={!row.played}
                        onChange={(e) => setField(p.id, 'assists', e.target.value)}
                      />
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
          <button type="button" onClick={handleSave} disabled={saving}>
            {saving ? 'Saving...' : 'Save stats'}
          </button>
        </>
      )}

      {status === 'success' && <p className="success">Stats saved.</p>}
      {error && <p className="error">{error}</p>}

      <StatsTables refreshKey={refreshKey} />
    </div>
  )
}

export default AdminStatsPage
