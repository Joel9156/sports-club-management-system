import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const ROLE_HOME = {
  Player: '/players/team',
  Volunteer: '/volunteers/profile',
  Coach: '/coaches/roster',
  Admin: '/admin/dashboard',
}

function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState(null)
  const [submitting, setSubmitting] = useState(false)

  async function handleSubmit(e) {
    e.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      const user = await login(email, password)
      navigate(ROLE_HOME[user.role] ?? '/', { replace: true })
    } catch (err) {
      setError(err.response?.data?.message ?? 'Login failed.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="page page-narrow">
      <h1>Log in</h1>
      <form onSubmit={handleSubmit}>
        <label>
          Email
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </label>
        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </label>
        {error && <p className="error">{error}</p>}
        <button type="submit" disabled={submitting}>
          {submitting ? 'Logging in...' : 'Log in'}
        </button>
      </form>
      <p style={{ marginTop: 12 }}>
        No account yet? <Link to="/register">Register</Link>
      </p>
    </div>
  )
}

export default LoginPage
