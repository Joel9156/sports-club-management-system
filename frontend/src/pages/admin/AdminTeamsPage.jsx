import { useEffect, useState } from 'react'
import { getTeams, createTeam } from '../../api/teams'

const blank = { name: '', ageGroup: '', coachName: '', season: '' }

function AdminTeamsPage() {
  const [teams, setTeams] = useState([])
  const [form, setForm] = useState(blank)
  const [error, setError] = useState(null)

  function reload() {
    getTeams()
      .then(setTeams)
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
      await createTeam(form)
      setForm(blank)
      reload()
    } catch (err) {
      setError(err.response?.data?.title ?? 'Could not create team.')
    }
  }

  return (
    <div className="page">
      <h1>Manage Teams</h1>
      {/* TeamsController only has GET/POST right now - no PUT/DELETE - so
          editing or removing a team isn't possible from the UI yet. */}
      {error && <p className="error">{error}</p>}
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Age group</th>
            <th>Coach</th>
            <th>Season</th>
          </tr>
        </thead>
        <tbody>
          {teams.map((t) => (
            <tr key={t.id}>
              <td>{t.name}</td>
              <td>{t.ageGroup}</td>
              <td>{t.coachName}</td>
              <td>{t.season}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <h2>Add team</h2>
      <p className="hint">
        Editing and deleting teams isn't available yet - the backend's TeamsController
        doesn't have PUT/DELETE endpoints (see backend/README.md).
      </p>
      <form onSubmit={handleSubmit}>
        <label>
          Name
          <input value={form.name} onChange={update('name')} required />
        </label>
        <label>
          Age group
          <input value={form.ageGroup} onChange={update('ageGroup')} required />
        </label>
        <label>
          Coach name
          <input value={form.coachName} onChange={update('coachName')} required />
        </label>
        <label>
          Season
          <input value={form.season} onChange={update('season')} required />
        </label>
        <button type="submit">Add team</button>
      </form>
    </div>
  )
}

export default AdminTeamsPage
