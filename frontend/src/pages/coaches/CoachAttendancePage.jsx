import { useEffect, useState } from 'react'
import { getPlayers } from '../../api/players'
import { getAttendance, recordAttendance } from '../../api/attendance'

function todayIso() {
  return new Date().toISOString().slice(0, 10)
}

// Marks attendance for the Mt Eden FC roster on a chosen date. Existing
// records for that date come from GET /api/attendance?date=; new ones are
// saved with POST /api/attendance. There's no PUT endpoint yet, so a player
// who already has a record for the selected date can't be re-marked here -
// their checkbox is disabled and shows what was already recorded instead.
function CoachAttendancePage() {
  const [players, setPlayers] = useState([])
  const [sessionDate, setSessionDate] = useState(todayIso())
  const [existing, setExisting] = useState([])
  const [presentMap, setPresentMap] = useState({})
  const [status, setStatus] = useState(null)
  const [error, setError] = useState(null)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    getPlayers()
      .then(setPlayers)
      .catch((err) => setError(err.message))
  }, [])

  useEffect(() => {
    if (!sessionDate) {
      setExisting([])
      return
    }
    getAttendance({ date: sessionDate })
      .then(setExisting)
      .catch((err) => setError(err.message))
  }, [sessionDate])

  const existingByPlayer = Object.fromEntries(existing.map((a) => [a.playerId, a]))
  const unrecorded = players.filter((p) => !existingByPlayer[p.id])

  function toggle(playerId) {
    setPresentMap((m) => ({ ...m, [playerId]: !m[playerId] }))
  }

  async function handleSave() {
    setStatus(null)
    setError(null)
    setSaving(true)
    try {
      await Promise.all(
        unrecorded.map((p) =>
          recordAttendance({
            playerId: p.id,
            sessionDate,
            isPresent: Boolean(presentMap[p.id]),
            notes: '',
          }),
        ),
      )
      const refreshed = await getAttendance({ date: sessionDate })
      setExisting(refreshed)
      setPresentMap({})
      setStatus('success')
    } catch (err) {
      setError(err.response?.data?.title ?? err.message)
      setStatus('error')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="page">
      <h1>Attendance</h1>
      <label>
        Session date
        <input type="date" value={sessionDate} onChange={(e) => setSessionDate(e.target.value)} />
      </label>

      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Present</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {players.map((p) => {
            const record = existingByPlayer[p.id]
            return (
              <tr key={p.id}>
                <td>{p.fullName}</td>
                <td>
                  <input
                    type="checkbox"
                    checked={record ? record.isPresent : Boolean(presentMap[p.id])}
                    disabled={Boolean(record)}
                    onChange={() => toggle(p.id)}
                  />
                </td>
                <td>{record ? 'Recorded' : 'Not yet recorded'}</td>
              </tr>
            )
          })}
          {players.length === 0 && (
            <tr>
              <td colSpan={3}>No players registered yet.</td>
            </tr>
          )}
        </tbody>
      </table>

      {unrecorded.length > 0 && (
        <button type="button" onClick={handleSave} disabled={saving}>
          {saving ? 'Saving...' : `Save attendance for ${unrecorded.length} player(s)`}
        </button>
      )}
      {players.length > 0 && unrecorded.length === 0 && (
        <p className="hint">All players already have an attendance record for this date.</p>
      )}

      {status === 'success' && <p className="success">Attendance saved.</p>}
      {status === 'error' && <p className="error">{error}</p>}

      <p className="hint">
        Editing a previously recorded entry isn't supported yet - <code>AttendanceController</code>{' '}
        has no PUT endpoint. Already-recorded players show as read-only above.
      </p>
    </div>
  )
}

export default CoachAttendancePage
