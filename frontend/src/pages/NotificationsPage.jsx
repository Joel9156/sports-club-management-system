import { useEffect, useState } from 'react'
import { getNotifications, markNotificationRead } from '../api/notifications'

// Available to every authenticated role - each user only ever sees their own
// notifications (enforced server-side, not just hidden in the UI). Right now
// the only thing that generates a notification is having attendance recorded
// against a Player record whose email matches your account (see
// AttendanceController) - a stand-in for the fuller "schedule changes, new
// role assignments" scope described in docs/03-proposed-solution.md.
function NotificationsPage() {
  const [notifications, setNotifications] = useState([])
  const [error, setError] = useState(null)

  function reload() {
    getNotifications()
      .then(setNotifications)
      .catch((err) => setError(err.message))
  }

  useEffect(reload, [])

  async function handleMarkRead(id) {
    try {
      await markNotificationRead(id)
      reload()
    } catch (err) {
      setError(err.message)
    }
  }

  if (error) return <p className="error">Failed to load: {error}</p>

  return (
    <div className="page">
      <h1>Notifications</h1>
      <p className="hint">
        Notifications are currently generated only when attendance is recorded against a
        player record matching your account email. Schedule-change and role-assignment
        notifications described in the proposed solution are not yet implemented.
      </p>
      <table>
        <thead>
          <tr>
            <th>Message</th>
            <th>Received</th>
            <th>Status</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {notifications.map((n) => (
            <tr key={n.id}>
              <td>{n.message}</td>
              <td>{new Date(n.createdAt).toLocaleString()}</td>
              <td>{n.isRead ? 'Read' : 'Unread'}</td>
              <td>
                {!n.isRead && (
                  <button type="button" onClick={() => handleMarkRead(n.id)}>
                    Mark as read
                  </button>
                )}
              </td>
            </tr>
          ))}
          {notifications.length === 0 && (
            <tr>
              <td colSpan={4}>No notifications yet.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  )
}

export default NotificationsPage
