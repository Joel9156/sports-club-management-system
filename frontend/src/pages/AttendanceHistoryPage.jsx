import { useEffect, useState } from 'react'
import { getPlayers } from '../api/players'
import { getAttendance } from '../api/attendance'

// Everyone's attendance across all sessions, for Admins and Coaches. (The
// Attendance page only shows one date at a time, for marking.) Filter by
// player and/or date; the summary is worked out from whatever's shown.
function AttendanceHistoryPage() {
  const [players, setPlayers] = useState([])
  const [records, setRecords] = useState([])
  const [playerId, setPlayerId] = useState('')
  const [date, setDate] = useState('')
  const [error, setError] = useState(null)

  useEffect(() => {
    getPlayers()
      .then(setPlayers)
      .catch((err) => setError(err.message))
  }, [])

  useEffect(() => {
    getAttendance({ playerId: playerId || undefined, date: date || undefined })
      .then((data) => setRecords([...data].sort((a, b) => b.sessionDate.localeCompare(a.sessionDate))))
      .catch((err) => setError(err.message))
  }, [playerId, date])

  if (error) return <p className="error">Failed to load: {error}</p>

  const nameById = Object.fromEntries(players.map((p) => [p.id, p.fullName]))

  // Per-player present/total for the records currently shown.
  const summary = {}
  for (const r of records) {
    const s = (summary[r.playerId] ??= { present: 0, total: 0 })
    s.total += 1
    if (r.isPresent) s.present += 1
  }

  return (
    <div className="page">
      <h1>Attendance History</h1>

      <label>
        Player
        <select value={playerId} onChange={(e) => setPlayerId(e.target.value)}>
          <option value="">All players</option>
          {players.map((p) => (
            <option key={p.id} value={p.id}>
              {p.fullName}
            </option>
          ))}
        </select>
      </label>
      <label>
        Date
        <input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
      </label>
      {date && (
        <button type="button" onClick={() => setDate('')}>
          Clear date
        </button>
      )}

      <h2>Summary</h2>
      <table>
        <thead>
          <tr>
            <th>Player</th>
            <th>Present</th>
            <th>Sessions</th>
            <th>Rate</th>
          </tr>
        </thead>
        <tbody>
          {Object.entries(summary).map(([id, s]) => (
            <tr key={id}>
              <td>{nameById[id] ?? `Player #${id}`}</td>
              <td>{s.present}</td>
              <td>{s.total}</td>
              <td>{Math.round((s.present / s.total) * 100)}%</td>
            </tr>
          ))}
          {records.length === 0 && (
            <tr>
              <td colSpan={4}>No attendance recorded for this selection.</td>
            </tr>
          )}
        </tbody>
      </table>

      <h2>Records</h2>
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Player</th>
            <th>Status</th>
            <th>Notes</th>
          </tr>
        </thead>
        <tbody>
          {records.map((r) => (
            <tr key={r.id}>
              <td>{r.sessionDate}</td>
              <td>{nameById[r.playerId] ?? `Player #${r.playerId}`}</td>
              <td>{r.isPresent ? 'Present' : 'Absent'}</td>
              <td>{r.notes || '—'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export default AttendanceHistoryPage
