import { useState } from 'react'
import VolunteerForm from '../../components/VolunteerForm'
import { createVolunteer } from '../../api/volunteers'

function VolunteerProfilePage() {
  const [status, setStatus] = useState(null)
  const [error, setError] = useState(null)
  const [formKey, setFormKey] = useState(0)

  async function handleCreate(volunteer) {
    setStatus(null)
    try {
      await createVolunteer(volunteer)
      setStatus('success')
      setFormKey((k) => k + 1)
    } catch (err) {
      setStatus('error')
      setError(err.response?.data?.title ?? 'Could not save profile.')
    }
  }

  return (
    <div className="page page-narrow">
      <h1>Volunteer Profile</h1>
      <VolunteerForm key={formKey} onSubmit={handleCreate} submitLabel="Save profile" />
      {status === 'success' && <p className="success">Profile saved.</p>}
      {status === 'error' && <p className="error">{error}</p>}
    </div>
  )
}

export default VolunteerProfilePage
