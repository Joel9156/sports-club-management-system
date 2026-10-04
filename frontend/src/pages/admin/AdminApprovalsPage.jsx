import { useEffect, useState } from 'react'
import { getPendingAccounts, approveAccount } from '../../api/users'

// Self-registered Player/Volunteer accounts waiting for review. Approving one
// creates its roster/volunteer record and lets the person into the app.
function AdminApprovalsPage() {
  const [accounts, setAccounts] = useState([])
  const [error, setError] = useState(null)

  function reload() {
    getPendingAccounts()
      .then(setAccounts)
      .catch((err) => setError(err.message))
  }

  useEffect(reload, [])

  async function handleApprove(id) {
    setError(null)
    try {
      await approveAccount(id)
      reload()
    } catch (err) {
      setError(err.response?.data?.title ?? 'Approval failed.')
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
            <th></th>
          </tr>
        </thead>
        <tbody>
          {accounts.map((a) => (
            <tr key={a.id}>
              <td>{a.fullName}</td>
              <td>{a.email}</td>
              <td>{a.role}</td>
              <td>
                <button type="button" onClick={() => handleApprove(a.id)}>
                  Approve
                </button>
              </td>
            </tr>
          ))}
          {accounts.length === 0 && (
            <tr>
              <td colSpan={4}>No accounts waiting for approval.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  )
}

export default AdminApprovalsPage
