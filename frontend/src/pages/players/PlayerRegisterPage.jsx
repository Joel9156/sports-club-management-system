import { useState } from 'react'
import PlayerForm from '../../components/PlayerForm'
import { createPlayer } from '../../api/players'

// Creates a Player roster record (POST /api/players) - distinct from the
// auth account created at /register. A player logs in first, then submits
// this to register themselves on the club roster.
function PlayerRegisterPage() {
  const [status, setStatus] = useState(null)
  const [error, setError] = useState(null)
  const [formKey, setFormKey] = useState(0)

  async function handleCreate(player) {
    setStatus(null)
    try {
      await createPlayer(player)
      setStatus('success')
      setFormKey((k) => k + 1) // remount PlayerForm to reset it
    } catch (err) {
      setStatus('error')
      setError(err.response?.data?.title ?? 'Registration failed.')
    }
  }

  return (
    <div className="page page-narrow">
      <h1>Player Registration</h1>
      {/* docs/03-proposed-solution.md calls for a required guardian/parent
          contact field when the registrant is under 18, but the Player model
          doesn't have one yet - that would need a backend model change. */}
      <PlayerForm key={formKey} onSubmit={handleCreate} submitLabel="Register" />
      {status === 'success' && <p className="success">Registration submitted.</p>}
      {status === 'error' && <p className="error">{error}</p>}
    </div>
  )
}

export default PlayerRegisterPage
