import { useEffect, useState } from 'react'
import { getPendingAccounts, approveAccount, rejectAccount } from '../../api/users'

// Self-registered Player/Volunteer accounts waiting for a decision. Approving
// one creates its roster/volunteer record and lets the person into the app.
// A rejected account can still be approved later.
function AdminApprovalsPage() {
  const [accounts, setAccounts] = useState([])
  const [error, setError] = useState(null)

  function reload() {
    getPendingAccounts()
      .then(setAccounts)
      .catch((err) => setError(err.message))
  }

  useEffect(reload, [])

  async function act(action, id) {
    setError(null)
    try {
      await action(id)
      reload()
    } catch (err) {
      setError(err.response?.data?.message ?? err.response?.data?.title ?? 'Action failed.')
    }
  }

  return (
    <div className="page">
      <h1>Pending Approvals</h1>
      {error && <p className="error">{error}</p>}
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Email</th>
            <th>Role</th>
            <th>Status</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {accounts.map((a) => (
            <tr key={a.id}>
              <td>{a.fullName}</td>
              <td>{a.email}</td>
              <td>{a.role}</td>
              <td>{a.isRejected ? 'Rejected' : 'Pending'}</td>
              <td>
                <button type="button" onClick={() => act(approveAccount, a.id)}>
                  Approve
                </button>
                {!a.isRejected && (
                  <>
                    {' '}
                    <button type="button" onClick={() => act(rejectAccount, a.id)}>
                      Reject
                    </button>
                  </>
                )}
              </td>
            </tr>
          ))}
          {accounts.length === 0 && (
            <tr>
              <td colSpan={5}>No accounts waiting for a decision.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  )
}

export default AdminApprovalsPage
