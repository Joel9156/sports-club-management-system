import { useEffect, useState } from 'react'
import { getNotifications, markNotificationRead } from '../api/notifications'

// Available to every authenticated role - each user only ever sees their own
// notifications (enforced server-side, not just hidden in the UI). They come
// from Coaches/Admins sending a notice (SendNotificationPage) or from having
// attendance recorded (see AttendanceController).
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
        Notices sent by a Coach or Admin, and confirmations when attendance is recorded
        against a player record matching your account email, appear here.
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
