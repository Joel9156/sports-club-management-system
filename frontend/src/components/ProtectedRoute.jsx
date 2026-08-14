import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

// Wraps a group of routes (via <Route element={<ProtectedRoute roles={...} />}>
// with nested <Route> children). Reads the role off the in-memory JWT claims
// stored in AuthContext - not by decoding a token from storage - and:
//  - redirects to /login if nobody is signed in
//  - redirects home if signed in but the role isn't allowed for this section
function ProtectedRoute({ roles }) {
  const { user } = useAuth()

  if (!user) {
    return <Navigate to="/login" replace />
  }

  if (roles && !roles.includes(user.role)) {
    return <Navigate to="/" replace />
  }

  return <Outlet />
}

export default ProtectedRoute
