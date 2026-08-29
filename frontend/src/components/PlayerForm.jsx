import { useEffect, useState } from 'react'
import { getTeams } from '../api/teams'

// Fields match backend/SportsClubApi/Models/Player.cs exactly.
const blank = {
  fullName: '',
  dateOfBirth: '',
  email: '',
  phone: '',
  teamId: '',
  registrationDate: new Date().toISOString().slice(0, 10),
  isActive: true,
}

// Shared create/edit form for a Player record, used by the player
// self-registration page and the admin players page.
//
// `defaultEmail` pre-fills the email field on first render only (e.g. with
// the logged-in user's account email, so a self-registering player doesn't
// have to retype what they already entered at signup) - it's read once via
// the lazy useState initializer, not re-applied on every render, so it won't
// fight the player while they're editing the form. `initial` is different:
// it's for switching which existing record is being edited (Admin CRUD) and
// does re-apply whenever it changes.
function PlayerForm({ initial, defaultEmail, onSubmit, submitLabel }) {
  const [teams, setTeams] = useState([])
  const [form, setForm] = useState(() => initial ?? { ...blank, email: defaultEmail ?? '' })

  useEffect(() => {
    getTeams().then(setTeams).catch(() => {})
  }, [])

  useEffect(() => {
    if (initial) setForm(initial)
  }, [initial])

  function update(field) {
    return (e) => {
      const value = field === 'isActive' ? e.target.checked : e.target.value
      setForm((f) => ({ ...f, [field]: value }))
    }
  }

  function handleSubmit(e) {
    e.preventDefault()
    onSubmit({ ...form, teamId: form.teamId ? Number(form.teamId) : null })
  }

  return (
    <form onSubmit={handleSubmit}>
      <label>
        Full name
        <input value={form.fullName} onChange={update('fullName')} required />
      </label>
      <label>
        Date of birth
        <input type="date" value={form.dateOfBirth} onChange={update('dateOfBirth')} required />
      </label>
      <label>
        Email
        <input type="email" value={form.email} onChange={update('email')} required />
      </label>
      <label>
        Phone
        <input value={form.phone} onChange={update('phone')} required />
      </label>
      <label>
        Team
        <select value={form.teamId ?? ''} onChange={update('teamId')}>
          <option value="">Unassigned</option>
          {teams.map((t) => (
            <option key={t.id} value={t.id}>
              {t.name} ({t.ageGroup})
            </option>
          ))}
        </select>
      </label>
      <label className="checkbox">
        <input type="checkbox" checked={form.isActive} onChange={update('isActive')} />
        Active
      </label>
      <button type="submit">{submitLabel}</button>
    </form>
  )
}

export default PlayerForm
