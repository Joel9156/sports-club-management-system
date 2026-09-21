import { useEffect, useState } from 'react'
import { getVolunteers } from '../../api/volunteers'
import { useAuth } from '../../context/AuthContext'

// Registering and editing volunteer records is Admin-only, so a Volunteer just
// sees the record an Admin created for them (matched by account email).
function VolunteerProfilePage() {
  const { user } = useAuth()
  const [profile, setProfile] = useState(null)
  const [loaded, setLoaded] = useState(false)
  const [error, setError] = useState(null)

  useEffect(() => {
    getVolunteers()
      .then((list) => setProfile(list.find((v) => v.email === user?.email) ?? null))
      .catch((err) => setError(err.message))
      .finally(() => setLoaded(true))
  }, [user?.email])

  if (error) return <p className="error">Failed to load: {error}</p>

  return (
    <div className="page page-narrow">
      <h1>Volunteer Profile</h1>
      {!loaded ? null : profile ? (
        <table>
          <tbody>
            <tr>
              <th>Name</th>
              <td>{profile.fullName}</td>
            </tr>
            <tr>
              <th>Email</th>
              <td>{profile.email}</td>
            </tr>
            <tr>
              <th>Phone</th>
              <td>{profile.phone}</td>
            </tr>
            <tr>
              <th>Role</th>
              <td>{profile.role}</td>
            </tr>
            <tr>
              <th>Availability</th>
              <td>{profile.availability}</td>
            </tr>
            <tr>
              <th>Active</th>
              <td>{profile.isActive ? 'Yes' : 'No'}</td>
            </tr>
          </tbody>
        </table>
      ) : (
        <p className="hint">
          Your volunteer profile has not been set up yet. Please ask an Admin to add you.
        </p>
      )}
    </div>
  )
}

export default VolunteerProfilePage
