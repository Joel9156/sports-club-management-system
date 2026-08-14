import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const emptyForm = { email: '', password: '', fullName: '', role: 'Player' }

// Not one of the explicitly requested routes, but added so there's a way to
// actually create the Player/Volunteer accounts that /login depends on -
// the backend's POST /api/auth/register only allows those two roles
// (Coach/Admin accounts are provisioned directly in the database).
function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState(emptyForm)
  const [error, setError] = useState(null)
  const [submitting, setSubmitting] = useState(false)

  function update(field) {
    return (e) => setForm((f) => ({ ...f, [field]: e.target.value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      const user = await register(form)
      navigate(user.role === 'Player' ? '/players/team' : '/volunteers/profile', {
        replace: true,
      })
    } catch (err) {
      setError(err.response?.data?.message ?? 'Registration failed.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="page page-narrow">
      <h1>Register</h1>
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
          Password
          <input
            type="password"
            value={form.password}
            onChange={update('password')}
            required
            minLength={8}
          />
        </label>
        <label>
          I am a
          <select value={form.role} onChange={update('role')}>
            <option value="Player">Player</option>
            <option value="Volunteer">Volunteer</option>
          </select>
        </label>
        {error && <p className="error">{error}</p>}
        <button type="submit" disabled={submitting}>
          {submitting ? 'Registering...' : 'Register'}
        </button>
      </form>
      <p style={{ marginTop: 12 }}>
        Already have an account? <Link to="/login">Log in</Link>
      </p>
    </div>
  )
}

export default RegisterPage
