import { useEffect, useState } from 'react'
import { getEvents, createEvent, updateEvent, deleteEvent } from '../../api/events'
import EventTable from '../../components/EventTable'
import { useAuth } from '../../context/AuthContext'

const blank = {
  type: 'Match',
  date: new Date().toISOString().slice(0, 10),
  location: '',
  opponent: '',
  goalsFor: '',
  goalsAgainst: '',
  notes: '',
}

// Turns the form's strings into what the API expects: numbers or null, and
// no opponent/score at all for training (the backend rejects those).
function toPayload(form) {
  const isMatch = form.type === 'Match'
  const num = (v) => (v === '' || v == null ? null : Number(v))
  return {
    ...(form.id ? { id: form.id } : {}),
    type: form.type,
    date: form.date,
    location: form.location,
    opponent: isMatch ? form.opponent : null,
    goalsFor: isMatch ? num(form.goalsFor) : null,
    goalsAgainst: isMatch ? num(form.goalsAgainst) : null,
    notes: form.notes || null,
  }
}

function fromEvent(e) {
  return {
    id: e.id,
    type: e.type,
    date: e.date,
    location: e.location,
    opponent: e.opponent ?? '',
    goalsFor: e.goalsFor ?? '',
    goalsAgainst: e.goalsAgainst ?? '',
    notes: e.notes ?? '',
  }
}

// Admin: add, edit and delete matches and training sessions. Leave the score
// blank until a match has been played - that's what marks it as upcoming.
function AdminSchedulePage() {
  const { user } = useAuth()
  const [events, setEvents] = useState([])
  const [form, setForm] = useState(blank)
  const [error, setError] = useState(null)

  function reload() {
    getEvents()
      .then(setEvents)
      .catch((err) => setError(err.message))
  }

  useEffect(reload, [])

  function update(field) {
    return (e) => setForm((f) => ({ ...f, [field]: e.target.value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)
    try {
      const payload = toPayload(form)
      if (form.id) {
        await updateEvent(form.id, payload)
      } else {
        await createEvent(payload)
      }
      setForm(blank)
      reload()
    } catch (err) {
      setError(err.response?.data?.message ?? err.response?.data?.title ?? 'Save failed.')
    }
  }

  async function handleDelete(id) {
    if (!window.confirm('Delete this event? Any stats recorded for it are deleted too.')) return
    try {
      await deleteEvent(id)
      if (form.id === id) setForm(blank)
      reload()
    } catch (err) {
      setError(err.response?.data?.title ?? 'Delete failed.')
    }
  }

  const isMatch = form.type === 'Match'

  return (
    <div className="page">
      <h1>Manage Schedule</h1>
      {error && <p className="error">{error}</p>}

      <EventTable
        events={events}
        emptyText="Nothing scheduled yet."
        renderActions={(e) => (
          <>
            <button type="button" onClick={() => setForm(fromEvent(e))}>
              Edit
            </button>
            {user?.role === 'Admin' && (
              <>
                {' '}
                <button type="button" onClick={() => handleDelete(e.id)}>
                  Delete
                </button>
              </>
            )}
          </>
        )}
      />

      <h2>{form.id ? 'Edit event' : 'Add event'}</h2>
      <form onSubmit={handleSubmit}>
        <label>
          Type
          <select value={form.type} onChange={update('type')}>
            <option value="Match">Match</option>
            <option value="Training">Training</option>
          </select>
        </label>
        <label>
          Date
          <input type="date" value={form.date} onChange={update('date')} required />
        </label>
        <label>
          Location
          <input value={form.location} onChange={update('location')} required />
        </label>
        {isMatch && (
          <>
            <label>
              Opponent
              <input value={form.opponent} onChange={update('opponent')} required />
            </label>
            <label>
              Goals for (leave blank until played)
              <input type="number" min="0" value={form.goalsFor} onChange={update('goalsFor')} />
            </label>
            <label>
              Goals against (leave blank until played)
              <input
                type="number"
                min="0"
                value={form.goalsAgainst}
                onChange={update('goalsAgainst')}
              />
            </label>
          </>
        )}
        <label>
          Notes
          <input value={form.notes} onChange={update('notes')} />
        </label>
        <button type="submit">{form.id ? 'Save changes' : 'Add event'}</button>
        {form.id && (
          <button type="button" onClick={() => setForm(blank)}>
            Cancel edit
          </button>
        )}
      </form>
    </div>
  )
}

export default AdminSchedulePage
