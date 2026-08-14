import { createContext, useCallback, useContext, useState } from 'react'
import { login as loginRequest, register as registerRequest } from '../api/auth'
import { setAuthToken } from '../api/client'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  // The logged-in user (email, fullName, role, token) lives only in React
  // state - not localStorage/sessionStorage - per the requirement to keep
  // the JWT in memory. The trade-off: refreshing the page logs the user out,
  // since there's nowhere persistent to read the token back from.
  const [user, setUser] = useState(null)

  const login = useCallback(async (email, password) => {
    const data = await loginRequest(email, password)
    setAuthToken(data.token)
    setUser(data)
    return data
  }, [])

  const register = useCallback(async (payload) => {
    const data = await registerRequest(payload)
    setAuthToken(data.token)
    setUser(data)
    return data
  }, [])

  const logout = useCallback(() => {
    setAuthToken(null)
    setUser(null)
  }, [])

  return (
    <AuthContext.Provider value={{ user, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return ctx
}
