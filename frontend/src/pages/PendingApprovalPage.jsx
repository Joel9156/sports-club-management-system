import { useAuth } from '../context/AuthContext'

// Shown instead of the normal app to a self-registered account an Admin
// hasn't approved yet (see backend/SportsClubApi/Controllers/UsersController.cs).
// There's nothing to do here but wait or log out - no page is reachable
// until the account is approved.
function PendingApprovalPage() {
  const { user } = useAuth()

  return (
    <div className="page page-narrow">
      <h1>Waiting for approval</h1>
      <p>
        Thanks for registering, {user?.fullName}. An Admin needs to review and approve your{' '}
        {user?.role.toLowerCase()} account before you can use the rest of the site. Check back
        later or contact the club if this takes longer than expected.
      </p>
    </div>
  )
}

export default PendingApprovalPage
