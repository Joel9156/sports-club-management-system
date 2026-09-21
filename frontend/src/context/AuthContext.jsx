import { createContext, useCallback, useContext, useState } from 'react'
import { login as loginRequest, register as registerRequest } from '../api/auth'
import { setAuthToken } from '../api/client'

const AuthContext = createContext(null)

const STORAGE_KEY = 'auth.user'

// Reads the saved login back on page load. Returns null (and clears the entry)
// if there's nothing saved, it can't be parsed, or its JWT has already expired,
// so a stale token never leaves the user "logged in" but rejected by the API.
function loadSavedUser() {
  try {
    const saved = JSON.parse(sessionStorage.getItem(STORAGE_KEY))
    if (!saved?.token) return null
    const { exp } = JSON.parse(atob(saved.token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')))
    if (exp && exp * 1000 <= Date.now()) {
      sessionStorage.removeItem(STORAGE_KEY)
      return null
    }
    return saved
  } catch {
    return null
  }
}

function saveUser(user) {
  try {
    if (user) sessionStorage.setItem(STORAGE_KEY, JSON.stringify(user))
    else sessionStorage.removeItem(STORAGE_KEY)
  } catch {
    // Storage can be unavailable (e.g. private mode) - login still works, it
    // just won't survive a refresh.
  }
}

export function AuthProvider({ children }) {
  // The logged-in user (email, fullName, role, token) is kept in React state
  // and mirrored to sessionStorage so a page refresh doesn't log the user out.
  // sessionStorage (not localStorage) means the login still ends when the tab
  // is closed. Trade-off: the JWT is readable by any script on the page, so it
  // is exposed if the app ever has an XSS hole.
  const [user, setUser] = useState(() => {
    const saved = loadSavedUser()
    setAuthToken(saved?.token ?? null)
    return saved
  })

  const login = useCallback(async (email, password) => {
    const data = await loginRequest(email, password)
    setAuthToken(data.token)
    saveUser(data)
    setUser(data)
    return data
  }, [])

  const register = useCallback(async (payload) => {
    const data = await registerRequest(payload)
    setAuthToken(data.token)
    saveUser(data)
    setUser(data)
    return data
  }, [])

  const logout = useCallback(() => {
    setAuthToken(null)
    saveUser(null)
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
