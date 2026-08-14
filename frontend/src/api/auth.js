import apiClient from './client'

// Matches backend/SportsClubApi/Dtos/AuthDtos.cs

export const login = (email, password) =>
  apiClient.post('/auth/login', { email, password }).then((res) => res.data)

// role must be "Player" or "Volunteer" - the backend rejects "Coach"/"Admin"
// at self-registration (see backend/README.md).
export const register = ({ email, password, fullName, role }) =>
  apiClient
    .post('/auth/register', { email, password, fullName, role })
    .then((res) => res.data)
