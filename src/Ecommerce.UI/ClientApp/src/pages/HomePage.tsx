import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

export default function HomePage() {
  const { isAuthenticated, user } = useAuth()

  return (
    <main className="container" style={{ paddingTop: 48, paddingBottom: 48 }}>
      {/* Hero */}
      <section
        style={{
          background: 'linear-gradient(135deg, #eff6ff 0%, #f0fdf4 100%)',
          border: '1px solid #bfdbfe',
          borderRadius: 16,
          padding: '48px 40px',
          marginBottom: 40,
        }}
      >
        <h1 style={{ fontSize: 36, fontWeight: 800, marginBottom: 12 }}>
          {isAuthenticated
            ? `Welcome back, ${user?.displayName ?? 'there'}! 👋`
            : 'Modern Ecommerce'}
        </h1>
        <p
          style={{
            color: '#4b5563',
            fontSize: 16,
            lineHeight: 1.7,
            maxWidth: 560,
            marginBottom: 24,
          }}
        >
          React + TypeScript SPA hosted by ASP.NET Core, powered by{' '}
          <strong>Ecommerce.API</strong>. Browse products, manage your cart
          with full JWT authentication.
        </p>

        <div style={{ display: 'flex', gap: 12 }}>
          <Link to="/products">
            <button className="btn-primary">Browse Products</button>
          </Link>
          {!isAuthenticated && (
            <Link to="/register">
              <button>Create Account</button>
            </Link>
          )}
        </div>
      </section>

      {/* Feature cards */}
      <section
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fill, minmax(240px, 1fr))',
          gap: 16,
        }}
      >
        {[
          {
            icon: '🔐',
            title: 'JWT Auth',
            desc: 'Login and register with secure JWT tokens.',
          },
          {
            icon: '🛍️',
            title: 'Products',
            desc: 'Browse catalogue with real-time stock info.',
          },
          {
            icon: '🛒',
            title: 'Cart',
            desc: 'Add/remove items with persisted cart state.',
          },
          {
            icon: '⚡',
            title: 'Vite + React',
            desc: 'Lightning-fast dev server with hot module reload.',
          },
        ].map((f) => (
          <div key={f.title} className="card">
            <div style={{ fontSize: 32, marginBottom: 12 }}>{f.icon}</div>
            <h3 style={{ marginBottom: 6 }}>{f.title}</h3>
            <p style={{ fontSize: 14, color: '#6b7280' }}>{f.desc}</p>
          </div>
        ))}
      </section>
    </main>
  )
}
