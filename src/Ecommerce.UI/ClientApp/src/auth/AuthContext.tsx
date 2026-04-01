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
  displayName?: string
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

function formatDisplayName(firstName?: string, lastName?: string): string | undefined {
  const normalizedFirstName = firstName?.trim()
  const normalizedLastName = lastName?.trim()

  if (!normalizedFirstName && !normalizedLastName) {
    return undefined
  }

  if (!normalizedFirstName) {
    return normalizedLastName?.toLowerCase()
  }

  if (!normalizedLastName) {
    return normalizedFirstName.toLowerCase()
  }

  return `${normalizedFirstName[0]}${normalizedLastName}`.toLowerCase()
}

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
        setUser({
          userId: me.userId,
          email: me.email,
          role: me.role,
          displayName: me.displayName,
        })
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
        setUser({
          email,
          role,
          displayName: formatDisplayName(firstName, lastName),
        })
      },

      register: async (payload: RegisterRequest) => {
        const { token, email, firstName, lastName, role } =
          await apiRegister(payload)
        tokenService.setAccessToken(token)
        setUser({
          email,
          role,
          displayName: formatDisplayName(firstName, lastName),
        })
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
