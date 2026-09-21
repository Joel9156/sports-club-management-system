import { useEffect, useState } from 'react'
import { getEvents } from '../api/events'
import EventTable from '../components/EventTable'

function todayIso() {
  return new Date().toISOString().slice(0, 10)
}

// The club schedule, read-only, for every role (Admins manage it on
// /admin/schedule). Mt Eden FC has a single team, so this is one club-wide
// list rather than a per-team one.
function SchedulePage() {
  const [events, setEvents] = useState([])
  const [filter, setFilter] = useState('')
  const [error, setError] = useState(null)

  useEffect(() => {
    getEvents()
      .then(setEvents)
      .catch((err) => setError(err.message))
  }, [])

  if (error) return <p className="error">Failed to load: {error}</p>

  const shown = filter ? events.filter((e) => e.type === filter) : events
  const today = todayIso()
  const upcoming = shown.filter((e) => e.date >= today)
  // Most recent first for past events - that's what people look for.
  const past = shown.filter((e) => e.date < today).reverse()

  return (
    <div className="page">
      <h1>Schedule</h1>
      <label>
        Show
        <select value={filter} onChange={(e) => setFilter(e.target.value)}>
          <option value="">Matches and training</option>
          <option value="Match">Matches only</option>
          <option value="Training">Training only</option>
        </select>
      </label>

      <h2>Upcoming</h2>
      <EventTable events={upcoming} emptyText="Nothing scheduled." />

      <h2>Past</h2>
      <EventTable events={past} emptyText="No past events." />
    </div>
  )
}

export default SchedulePage
