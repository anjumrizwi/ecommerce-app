import {
  createContext,
  type ReactNode,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react'
import {
  getMe,
  login as apiLogin,
  register as apiRegister,
} from '../api/auth'
import type { LoginRequest, RegisterRequest } from '../api/auth'
import { tokenService } from './tokenService'

// ─── Types ────────────────────────────────────────────────────────────────────

export type AuthUser = {
  userId?: string
  email?: string
  role?: string
}

type AuthContextValue = {
  isAuthenticated: boolean
  /** True while the initial JWT bootstrap check is in progress. */
  isBootstrapping: boolean
  user: AuthUser | null
  login: (payload: LoginRequest) => Promise<void>
  register: (payload: RegisterRequest) => Promise<void>
  logout: () => void
}

// ─── Context ──────────────────────────────────────────────────────────────────

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

// ─── Provider ─────────────────────────────────────────────────────────────────

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isBootstrapping, setIsBootstrapping] = useState(true)
  const [user, setUser] = useState<AuthUser | null>(null)

  // Restore session from stored token on first mount
  useEffect(() => {
    const restore = async () => {
      const token = tokenService.getAccessToken()
      if (!token || tokenService.isExpired(token)) {
        tokenService.clear()
        setIsBootstrapping(false)
        return
      }
      try {
        const me = await getMe()
        setUser(me)
      } catch {
        tokenService.clear()
        setUser(null)
      } finally {
        setIsBootstrapping(false)
      }
    }
    void restore()
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      isAuthenticated: !!user,
      isBootstrapping,
      user,

      login: async (payload: LoginRequest) => {
        const { token, email, firstName, lastName, role } =
          await apiLogin(payload)
        tokenService.setAccessToken(token)
        setUser({ email, role, userId: `${firstName} ${lastName}` })
      },

      register: async (payload: RegisterRequest) => {
        const { token, email, firstName, lastName, role } =
          await apiRegister(payload)
        tokenService.setAccessToken(token)
        setUser({ email, role, userId: `${firstName} ${lastName}` })
      },

      logout: () => {
        tokenService.clear()
        setUser(null)
      },
    }),
    [isBootstrapping, user],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

// ─── Hook ─────────────────────────────────────────────────────────────────────

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within <AuthProvider>')
  return ctx
}
