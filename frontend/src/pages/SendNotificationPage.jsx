import { useState } from 'react'
import { sendNotification } from '../api/notifications'

// Coaches and Admins broadcast a short notice; the backend delivers a copy to
// every account in the chosen audience (they read it on the Notifications page).
function SendNotificationPage() {
  const [message, setMessage] = useState('')
  const [audience, setAudience] = useState('Players')
  const [result, setResult] = useState(null)
  const [error, setError] = useState(null)

  async function handleSubmit(e) {
    e.preventDefault()
    setResult(null)
    setError(null)
    try {
      const { sent } = await sendNotification(message, audience)
      setResult(`Sent to ${sent} ${sent === 1 ? 'person' : 'people'}.`)
      setMessage('')
    } catch (err) {
      setError(err.response?.data?.message ?? err.response?.data?.title ?? 'Could not send.')
    }
  }

  return (
    <div className="page page-narrow">
      <h1>Send Notice</h1>
      <form onSubmit={handleSubmit}>
        <label>
          Send to
          <select value={audience} onChange={(e) => setAudience(e.target.value)}>
            <option value="Players">All players</option>
            <option value="Volunteers">All volunteers</option>
            <option value="Everyone">Everyone</option>
          </select>
        </label>
        <label>
          Message
          <textarea
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            maxLength={500}
            rows={4}
            required
          />
        </label>
        <button type="submit">Send</button>
      </form>
      {result && <p className="success">{result}</p>}
      {error && <p className="error">{error}</p>}
    </div>
  )
}

export default SendNotificationPage
