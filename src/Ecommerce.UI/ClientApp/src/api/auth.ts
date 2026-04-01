import api from './client'

// ─── Request types ────────────────────────────────────────────────────────────

export type LoginRequest = {
  email: string
  password: string
}

export type RegisterRequest = {
  firstName: string
  lastName: string
  email: string
  password: string
  role?: string
}

// ─── Response types (matches Ecommerce.API AuthResponse record) ───────────────

/** AuthResponse: { token, email, firstName, lastName, role } */
export type AuthResponse = {
  token: string
  email: string
  firstName: string
  lastName: string
  role: string
}

/** /api/auth/me response: { userId, email, role } */
export type MeResponse = {
  userId?: string
  email?: string
  role?: string
  displayName?: string
}

// ─── API calls ────────────────────────────────────────────────────────────────

export const login = async (payload: LoginRequest): Promise<AuthResponse> => {
  const { data } = await api.post<AuthResponse>('/auth/login', payload)
  return data
}

export const register = async (
  payload: RegisterRequest,
): Promise<AuthResponse> => {
  const { data } = await api.post<AuthResponse>('/auth/register', payload)
  return data
}

export const getMe = async (): Promise<MeResponse> => {
  const { data } = await api.get<MeResponse>('/auth/me')
  return data
}
