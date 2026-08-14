import axios from 'axios'

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5235/api',
})

// The JWT is kept in memory only (see context/AuthContext.jsx), never in
// localStorage/sessionStorage. This module-level variable is the one place
// that memory lives so this axios instance (created outside React) can read
// it; AuthContext calls setAuthToken() whenever the token changes.
let currentToken = null

export function setAuthToken(token) {
  currentToken = token
}

apiClient.interceptors.request.use((config) => {
  if (currentToken) {
    config.headers.Authorization = `Bearer ${currentToken}`
  }
  return config
})

export default apiClient
