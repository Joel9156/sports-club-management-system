import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const NAV_LINKS = {
  Player: [
    ['/players/register', 'Register'],
    ['/players/team', 'My Team'],
  ],
  Volunteer: [
    ['/volunteers/profile', 'Profile'],
    ['/volunteers/schedule', 'Schedule'],
  ],
  Coach: [
    ['/coaches/roster', 'Roster'],
    ['/coaches/attendance', 'Attendance'],
  ],
  Admin: [
    ['/admin/dashboard', 'Dashboard'],
    ['/admin/players', 'Players'],
    ['/admin/volunteers', 'Volunteers'],
    ['/admin/teams', 'Teams'],
  ],
}

function NavBar() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/login')
  }

  return (
    <nav className="navbar">
      <span className="brand">Sports Club</span>
      {user &&
        (NAV_LINKS[user.role] ?? []).map(([to, label]) => (
          <Link key={to} to={to}>
            {label}
          </Link>
        ))}
      <span className="spacer" />
      {user ? (
        <>
          <span className="user-badge">
            {user.fullName} ({user.role})
          </span>
          <button type="button" onClick={handleLogout}>
            Log out
          </button>
        </>
      ) : (
        <Link to="/login">Log in</Link>
      )}
    </nav>
  )
}

export default NavBar
