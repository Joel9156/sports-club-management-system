import { useEffect, useState } from 'react'
import PlayerForm from '../../components/PlayerForm'
import { getPlayers, createPlayer, updatePlayer } from '../../api/players'
import { useAuth } from '../../context/AuthContext'

// Registering a Player account (GitHub #2) now auto-creates a roster record
// with the account's name/email, so this page's job is usually to fill in
// what that couldn't know - date of birth, phone - on the existing record
// (PUT), rather than create a new one (POST). It falls back to creating a
// record if one somehow doesn't exist yet.
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
      if (existing) {
        await updatePlayer(existing.id, { ...player, id: existing.id })
      } else {
        await createPlayer(player)
      }
      setStatus('success')
      setFormKey((k) => k + 1) // remount PlayerForm to reset it
    } catch (err) {
      setStatus('error')
      setError(err.response?.data?.title ?? err.response?.data?.message ?? 'Registration failed.')
    }
  }

  return (
    <div className="page page-narrow">
      <h1>Player Registration</h1>
      {/* docs/03-proposed-solution.md calls for a required guardian/parent
          contact field when the registrant is under 18, but the Player model
          doesn't have one yet - that would need a backend model change. */}
      <PlayerForm
        key={formKey}
        initial={existing ?? undefined}
        defaultEmail={user?.email}
        onSubmit={handleSave}
        submitLabel={existing ? 'Save details' : 'Register'}
      />
      {status === 'success' && <p className="success">Details saved.</p>}
      {status === 'error' && <p className="error">{error}</p>}
    </div>
  )
}

export default PlayerRegisterPage
