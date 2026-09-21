import { useEffect, useState } from 'react'
import PlayerForm from '../../components/PlayerForm'
import { getPlayers, updatePlayer } from '../../api/players'
import { useAuth } from '../../context/AuthContext'

// Registering a Player account (GitHub #2) now auto-creates a roster record
// with the account's name/email, so this page's job is usually to fill in
// what that couldn't know - date of birth, phone - on the existing record
// (PUT). Creating roster records is Admin-only, so if none exists yet the
// player is told to ask an Admin.
function PlayerRegisterPage() {
  const { user } = useAuth()
  const [existing, setExisting] = useState(null)
  const [status, setStatus] = useState(null)
  const [error, setError] = useState(null)
  const [formKey, setFormKey] = useState(0)

  useEffect(() => {
    if (!user?.email) return
    getPlayers()
      .then((players) => setExisting(players.find((p) => p.email === user.email) ?? null))
      .catch(() => {}) // keep the form usable even if this lookup fails
  }, [user?.email])

  async function handleSave(player) {
    setStatus(null)
    try {
      await updatePlayer(existing.id, { ...player, id: existing.id })
      setStatus('success')
      setFormKey((k) => k + 1) // remount PlayerForm to reset it
    } catch (err) {
      setStatus('error')
      setError(err.response?.data?.title ?? err.response?.data?.message ?? 'Registration failed.')
    }
  }

  return (
    <div className="page page-narrow">
      <h1>My Details</h1>
      {/* docs/03-proposed-solution.md calls for a required guardian/parent
          contact field when the registrant is under 18, but the Player model
          doesn't have one yet - that would need a backend model change. */}
      {existing ? (
        <PlayerForm
          key={formKey}
          initial={existing}
          defaultEmail={user?.email}
          onSubmit={handleSave}
          submitLabel="Save details"
        />
      ) : (
        <p className="hint">
          Your player record has not been set up yet. Please ask an Admin to add you.
        </p>
      )}
      {status === 'success' && <p className="success">Details saved.</p>}
      {status === 'error' && <p className="error">{error}</p>}
    </div>
  )
}

export default PlayerRegisterPage
