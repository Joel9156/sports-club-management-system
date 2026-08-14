import { useEffect, useState } from 'react'

// Fields match backend/SportsClubApi/Models/Volunteer.cs exactly.
const blank = {
  fullName: '',
  email: '',
  phone: '',
  role: '',
  availability: '',
  isActive: true,
}

// Shared create/edit form for a Volunteer record, used by the volunteer
// self-service profile page and the admin volunteers page.
function VolunteerForm({ initial, onSubmit, submitLabel }) {
  const [form, setForm] = useState(initial ?? blank)

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
    onSubmit(form)
  }

  return (
    <form onSubmit={handleSubmit}>
      <label>
        Full name
        <input value={form.fullName} onChange={update('fullName')} required />
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
        Role
        <input
          value={form.role}
          onChange={update('role')}
          placeholder="e.g. First Aid, Team Manager"
          required
        />
      </label>
      <label>
        Availability
        <input
          value={form.availability}
          onChange={update('availability')}
          placeholder="e.g. Weekday evenings"
          required
        />
      </label>
      <label className="checkbox">
        <input type="checkbox" checked={form.isActive} onChange={update('isActive')} />
        Active
      </label>
      <button type="submit">{submitLabel}</button>
    </form>
  )
}

export default VolunteerForm
