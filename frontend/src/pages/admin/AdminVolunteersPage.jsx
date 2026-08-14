import { useEffect, useState } from 'react'
import VolunteerForm from '../../components/VolunteerForm'
import {
  getVolunteers,
  createVolunteer,
  updateVolunteer,
  deleteVolunteer,
} from '../../api/volunteers'

function AdminVolunteersPage() {
  const [volunteers, setVolunteers] = useState([])
  const [editing, setEditing] = useState(null)
  const [error, setError] = useState(null)

  function reload() {
    getVolunteers()
      .then(setVolunteers)
      .catch((err) => setError(err.message))
  }

  useEffect(reload, [])

  async function handleSubmit(volunteer) {
    setError(null)
    try {
      if (editing) {
        await updateVolunteer(editing.id, { ...volunteer, id: editing.id })
      } else {
        await createVolunteer(volunteer)
      }
      setEditing(null)
      reload()
    } catch (err) {
      setError(err.response?.data?.title ?? 'Save failed.')
    }
  }

  async function handleDelete(id) {
    if (!window.confirm('Delete this volunteer?')) return
    try {
      await deleteVolunteer(id)
      reload()
    } catch (err) {
      setError(err.response?.data?.title ?? 'Delete failed.')
    }
  }

  return (
    <div className="page">
      <h1>Manage Volunteers</h1>
      {error && <p className="error">{error}</p>}
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Role</th>
            <th>Active</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {volunteers.map((v) => (
            <tr key={v.id}>
              <td>{v.fullName}</td>
              <td>{v.role}</td>
              <td>{v.isActive ? 'Yes' : 'No'}</td>
              <td>
                <button type="button" onClick={() => setEditing(v)}>
                  Edit
                </button>{' '}
                <button type="button" onClick={() => handleDelete(v.id)}>
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <h2>{editing ? `Edit ${editing.fullName}` : 'Add volunteer'}</h2>
      <VolunteerForm
        key={editing?.id ?? 'new'}
        initial={editing ?? undefined}
        onSubmit={handleSubmit}
        submitLabel={editing ? 'Save changes' : 'Add volunteer'}
      />
      {editing && (
        <button type="button" onClick={() => setEditing(null)}>
          Cancel edit
        </button>
      )}
    </div>
  )
}

export default AdminVolunteersPage
