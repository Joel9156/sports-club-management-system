import axios from 'axios'

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5235/api',
})

// The current JWT lives in this module-level variable so this axios instance
// (created outside React) can read it; AuthContext calls setAuthToken() whenever
// the token changes (and restores it from sessionStorage after a refresh).
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
