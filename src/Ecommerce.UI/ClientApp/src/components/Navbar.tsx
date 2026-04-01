import { Link, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

export default function Navbar() {
  const { isAuthenticated, user, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login', { replace: true })
  }

  const navLinkStyle = ({ isActive }: { isActive: boolean }) => ({
    fontWeight: isActive ? 600 : 400,
    color: isActive ? '#111827' : '#6b7280',
    textDecoration: 'none' as const,
  })

  return (
    <header
      style={{
        borderBottom: '1px solid #e5e7eb',
        padding: '0',
        background: '#ffffff',
        position: 'sticky' as const,
        top: 0,
        zIndex: 100,
        boxShadow: '0 1px 3px rgba(0,0,0,0.05)',
      }}
    >
      <nav
        className="container"
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          height: 56,
        }}
      >
        <Link
          to="/"
          style={{ fontWeight: 700, fontSize: 18, color: '#111827', textDecoration: 'none' }}
        >
          🛒 Ecommerce.UI
        </Link>

        <div style={{ display: 'flex', gap: 20, alignItems: 'center' }}>
          <NavLink to="/" end style={navLinkStyle}>
            Home
          </NavLink>
          <NavLink to="/products" style={navLinkStyle}>
            Products
          </NavLink>

          {isAuthenticated && (
            <>
              <NavLink to="/cart" style={navLinkStyle}>
                Cart
              </NavLink>
              <NavLink to="/orders" style={navLinkStyle}>
                Orders
              </NavLink>
            </>
          )}

          {!isAuthenticated ? (
            <>
              <NavLink to="/login" style={navLinkStyle}>
                Login
              </NavLink>
              <NavLink to="/register" style={navLinkStyle}>
                <span
                  style={{
                    background: '#111827',
                    color: '#fff',
                    padding: '6px 14px',
                    borderRadius: 6,
                    fontWeight: 500,
                  }}
                >
                  Register
                </span>
              </NavLink>
            </>
          ) : (
            <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
              <Link
                to="/profile"
                style={{
                  fontSize: 13,
                  color: '#6b7280',
                  textDecoration: 'none',
                }}
              >
                {user?.displayName ?? user?.email}
              </Link>
              <button
                onClick={handleLogout}
                style={{
                  padding: '6px 14px',
                  fontSize: 13,
                }}
              >
                Logout
              </button>
            </div>
          )}
        </div>
      </nav>
    </header>
  )
}
