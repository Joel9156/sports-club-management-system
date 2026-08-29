import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider, useAuth } from './context/AuthContext'
import NavBar from './components/NavBar'
import ProtectedRoute from './components/ProtectedRoute'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import PlayerRegisterPage from './pages/players/PlayerRegisterPage'
import PlayerTeamPage from './pages/players/PlayerTeamPage'
import VolunteerProfilePage from './pages/volunteers/VolunteerProfilePage'
import VolunteerSchedulePage from './pages/volunteers/VolunteerSchedulePage'
import CoachRosterPage from './pages/coaches/CoachRosterPage'
import CoachAttendancePage from './pages/coaches/CoachAttendancePage'
import AdminDashboardPage from './pages/admin/AdminDashboardPage'
import AdminPlayersPage from './pages/admin/AdminPlayersPage'
import AdminVolunteersPage from './pages/admin/AdminVolunteersPage'
import './App.css'

const ROLE_HOME = {
  Player: '/players/team',
  Volunteer: '/volunteers/profile',
  Coach: '/coaches/roster',
  Admin: '/admin/dashboard',
}

// Sends "/" (and any unknown path) to the signed-in user's default page, or
// to /login if nobody is signed in.
function Home() {
  const { user } = useAuth()
  return <Navigate to={user ? (ROLE_HOME[user.role] ?? '/login') : '/login'} replace />
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />

      <Route element={<ProtectedRoute roles={['Player', 'Admin']} />}>
        <Route path="/players/register" element={<PlayerRegisterPage />} />
        <Route path="/players/team" element={<PlayerTeamPage />} />
      </Route>

      <Route element={<ProtectedRoute roles={['Volunteer', 'Admin']} />}>
        <Route path="/volunteers/profile" element={<VolunteerProfilePage />} />
        <Route path="/volunteers/schedule" element={<VolunteerSchedulePage />} />
      </Route>

      <Route element={<ProtectedRoute roles={['Coach', 'Admin']} />}>
        <Route path="/coaches/roster" element={<CoachRosterPage />} />
        <Route path="/coaches/attendance" element={<CoachAttendancePage />} />
      </Route>

      <Route element={<ProtectedRoute roles={['Admin']} />}>
        <Route path="/admin/dashboard" element={<AdminDashboardPage />} />
        <Route path="/admin/players" element={<AdminPlayersPage />} />
        <Route path="/admin/volunteers" element={<AdminVolunteersPage />} />
      </Route>

      <Route path="/" element={<Home />} />
      <Route path="*" element={<Home />} />
    </Routes>
  )
}

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <NavBar />
        <AppRoutes />
      </AuthProvider>
    </BrowserRouter>
  )
}

export default App
