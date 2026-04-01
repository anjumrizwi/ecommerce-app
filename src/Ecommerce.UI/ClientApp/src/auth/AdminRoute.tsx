import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from './AuthContext'

/**
 * Wraps admin-only routes. Redirects:
 * - Unauthenticated users to /login
 * - Non-admin users to /products (home for authenticated users)
 */
export default function AdminRoute() {
  const { isAuthenticated, isBootstrapping, user } = useAuth()
  const location = useLocation()
  const isAdmin = user?.role === 'Admin'

  if (isBootstrapping) {
    return <div className="spinner">Loading session…</div>
  }

  if (!isAuthenticated) {
    return (
      <Navigate to="/login" replace state={{ from: location.pathname }} />
    )
  }

  if (!isAdmin) {
    return (
      <main className="container" style={{ paddingTop: 48, textAlign: 'center' }}>
        <h2>Access Denied</h2>
        <p style={{ color: '#6b7280', marginBottom: 20 }}>
          You do not have permission to access this page. Admin privileges required.
        </p>
        <a href="/products" style={{ color: '#3b82f6', textDecoration: 'none' }}>
          ← Back to Products
        </a>
      </main>
    )
  }

  return <Outlet />
}
