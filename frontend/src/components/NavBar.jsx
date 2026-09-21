import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import logo from '../assets/mt-eden-fc-logo.svg'

const NAV_LINKS = {
  Player: [
    ['/players/register', 'Register'],
    ['/players/team', 'Roster'],
    ['/players/attendance', 'My Attendance'],
    ['/schedule', 'Schedule'],
    ['/stats', 'Stats'],
  ],
  Volunteer: [
    ['/volunteers/profile', 'Profile'],
    ['/volunteers/schedule', 'Schedule'],
    ['/schedule', 'Club Schedule'],
    ['/stats', 'Stats'],
  ],
  Coach: [
    ['/coaches/roster', 'Roster'],
    ['/coaches/attendance', 'Attendance'],
    ['/attendance/history', 'History'],
    ['/schedule', 'Schedule'],
    ['/stats', 'Stats'],
  ],
  Admin: [
    ['/admin/dashboard', 'Dashboard'],
    ['/admin/players', 'Players'],
    ['/admin/volunteers', 'Volunteers'],
    // Admins are already allowed on the attendance route (see App.jsx) and
    // can record attendance via the API; the page just had no menu entry.
    ['/coaches/attendance', 'Attendance'],
    ['/attendance/history', 'History'],
    ['/admin/schedule', 'Schedule'],
    ['/admin/stats', 'Stats'],
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
      <img src={logo} alt="Mt Eden FC" className="brand-logo" />
      <span className="brand">Mt Eden FC</span>
      {user &&
        (NAV_LINKS[user.role] ?? []).map(([to, label]) => (
          <Link key={to} to={to}>
            {label}
          </Link>
        ))}
      {/* Every role has the same own-notifications page, unlike the
          role-specific links above. */}
      {user && <Link to="/notifications">Notifications</Link>}
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
