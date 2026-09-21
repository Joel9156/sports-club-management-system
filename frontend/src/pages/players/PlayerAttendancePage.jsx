import { useEffect, useState } from 'react'
import { getPlayers } from '../../api/players'
import { getAttendance } from '../../api/attendance'
import { useAuth } from '../../context/AuthContext'

// A player's own attendance history. There's no formal User-to-Player link in
// the data model, so - like PlayerRegisterPage - this finds "my" player record
// by matching the logged-in account's email, then asks the API for that
// player's attendance (GET /api/attendance?playerId=). The filtering happens
// here; the API itself doesn't restrict a Player to their own records.
function PlayerAttendancePage() {
  const { user } = useAuth()
  const [player, setPlayer] = useState(null)
  const [records, setRecords] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    if (!user?.email) return
    getPlayers()
      .then((players) => {
        const mine = players.find((p) => p.email === user.email) ?? null
        setPlayer(mine)
        return mine ? getAttendance({ playerId: mine.id }) : []
      })
      .then((data) => setRecords([...data].sort((a, b) => b.sessionDate.localeCompare(a.sessionDate))))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }, [user?.email])

  if (loading) return <p>Loading...</p>
  if (error) return <p className="error">Failed to load: {error}</p>

  const presentCount = records.filter((r) => r.isPresent).length

  return (
    <div className="page">
      <h1>My Attendance</h1>
      {!player ? (
        <p className="hint">
          No player record matches your account email ({user?.email}), so there's no attendance
          to show yet.
        </p>
      ) : (
        <>
          <p>
            {presentCount} present out of {records.length} recorded session(s).
          </p>
          <table>
            <thead>
              <tr>
                <th>Date</th>
                <th>Status</th>
                <th>Notes</th>
              </tr>
            </thead>
            <tbody>
              {records.map((r) => (
                <tr key={r.id}>
                  <td>{r.sessionDate}</td>
                  <td>{r.isPresent ? 'Present' : 'Absent'}</td>
                  <td>{r.notes || '—'}</td>
                </tr>
              ))}
              {records.length === 0 && (
                <tr>
                  <td colSpan={3}>No attendance has been recorded for you yet.</td>
                </tr>
              )}
            </tbody>
          </table>
        </>
      )}
    </div>
  )
}

export default PlayerAttendancePage
