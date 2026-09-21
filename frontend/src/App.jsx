import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider, useAuth } from './context/AuthContext'
import NavBar from './components/NavBar'
import ProtectedRoute from './components/ProtectedRoute'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import PlayerRegisterPage from './pages/players/PlayerRegisterPage'
import PlayerTeamPage from './pages/players/PlayerTeamPage'
import PlayerAttendancePage from './pages/players/PlayerAttendancePage'
import VolunteerProfilePage from './pages/volunteers/VolunteerProfilePage'
import VolunteerSchedulePage from './pages/volunteers/VolunteerSchedulePage'
import CoachRosterPage from './pages/coaches/CoachRosterPage'
import CoachAttendancePage from './pages/coaches/CoachAttendancePage'
import AdminDashboardPage from './pages/admin/AdminDashboardPage'
import AdminPlayersPage from './pages/admin/AdminPlayersPage'
import AdminVolunteersPage from './pages/admin/AdminVolunteersPage'
import NotificationsPage from './pages/NotificationsPage'
import SchedulePage from './pages/SchedulePage'
import StatsPage from './pages/StatsPage'
import AttendanceHistoryPage from './pages/AttendanceHistoryPage'
import AdminSchedulePage from './pages/admin/AdminSchedulePage'
import AdminStatsPage from './pages/admin/AdminStatsPage'
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

      {/* Player-only: an Admin has no player record of their own to show. */}
      <Route element={<ProtectedRoute roles={['Player']} />}>
        <Route path="/players/attendance" element={<PlayerAttendancePage />} />
      </Route>

      <Route element={<ProtectedRoute roles={['Volunteer', 'Admin']} />}>
        <Route path="/volunteers/profile" element={<VolunteerProfilePage />} />
        <Route path="/volunteers/schedule" element={<VolunteerSchedulePage />} />
      </Route>

      <Route element={<ProtectedRoute roles={['Coach', 'Admin']} />}>
        <Route path="/coaches/roster" element={<CoachRosterPage />} />
        <Route path="/coaches/attendance" element={<CoachAttendancePage />} />
        <Route path="/attendance/history" element={<AttendanceHistoryPage />} />
      </Route>

      <Route element={<ProtectedRoute roles={['Admin']} />}>
        <Route path="/admin/dashboard" element={<AdminDashboardPage />} />
        <Route path="/admin/players" element={<AdminPlayersPage />} />
        <Route path="/admin/volunteers" element={<AdminVolunteersPage />} />
        <Route path="/admin/schedule" element={<AdminSchedulePage />} />
        <Route path="/admin/stats" element={<AdminStatsPage />} />
      </Route>

      {/* No roles prop - any authenticated user, of any role, can see their
          own notifications, and read the club schedule and stats. */}
      <Route element={<ProtectedRoute />}>
        <Route path="/notifications" element={<NotificationsPage />} />
        <Route path="/schedule" element={<SchedulePage />} />
        <Route path="/stats" element={<StatsPage />} />
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
