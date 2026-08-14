import { useEffect, useState } from 'react'
import PlayerForm from '../../components/PlayerForm'
import { getPlayers, createPlayer, updatePlayer, deletePlayer } from '../../api/players'

function AdminPlayersPage() {
  const [players, setPlayers] = useState([])
  const [editing, setEditing] = useState(null) // player being edited, or null to create
  const [error, setError] = useState(null)

  function reload() {
    getPlayers()
      .then(setPlayers)
      .catch((err) => setError(err.message))
  }

  useEffect(reload, [])

  async function handleSubmit(player) {
    setError(null)
    try {
      if (editing) {
        await updatePlayer(editing.id, { ...player, id: editing.id })
      } else {
        await createPlayer(player)
      }
      setEditing(null)
      reload()
    } catch (err) {
      setError(err.response?.data?.title ?? 'Save failed.')
    }
  }

  async function handleDelete(id) {
    if (!window.confirm('Delete this player?')) return
    try {
      await deletePlayer(id)
      reload()
    } catch (err) {
      setError(err.response?.data?.title ?? 'Delete failed.')
    }
  }

  return (
    <div className="page">
      <h1>Manage Players</h1>
      {error && <p className="error">{error}</p>}
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Team</th>
            <th>Active</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {players.map((p) => (
            <tr key={p.id}>
              <td>{p.fullName}</td>
              <td>{p.teamId ?? '—'}</td>
              <td>{p.isActive ? 'Yes' : 'No'}</td>
              <td>
                <button type="button" onClick={() => setEditing(p)}>
                  Edit
                </button>{' '}
                <button type="button" onClick={() => handleDelete(p.id)}>
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <h2>{editing ? `Edit ${editing.fullName}` : 'Add player'}</h2>
      <PlayerForm
        key={editing?.id ?? 'new'}
        initial={editing ?? undefined}
        onSubmit={handleSubmit}
        submitLabel={editing ? 'Save changes' : 'Add player'}
      />
      {editing && (
        <button type="button" onClick={() => setEditing(null)}>
          Cancel edit
        </button>
      )}
    </div>
  )
}

export default AdminPlayersPage
