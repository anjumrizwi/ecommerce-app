// ─── Token storage keys ───────────────────────────────────────────────────────
const ACCESS_TOKEN_KEY = 'ecommerce.accessToken'

export const tokenService = {
  getAccessToken: (): string | null => localStorage.getItem(ACCESS_TOKEN_KEY),

  setAccessToken: (token: string): void =>
    localStorage.setItem(ACCESS_TOKEN_KEY, token),

  clear: (): void => localStorage.removeItem(ACCESS_TOKEN_KEY),

  /** Returns true if the token is missing or within 30 s of expiring. */
  isExpired: (token: string): boolean => {
    try {
      const payload = JSON.parse(atob(token.split('.')[1])) as {
        exp?: number
      }
      if (!payload.exp) return true
      return payload.exp * 1000 <= Date.now() + 30_000
    } catch {
      return true
    }
  },
}
