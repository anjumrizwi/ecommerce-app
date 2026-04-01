import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { getOrders } from '../api/orders'
import { toOrderReference } from '../utils/orderReference'

const statusStyle = (value: string) => {
  const normalized = value.toLowerCase()

  if (normalized === 'paid' || normalized === 'confirmed') {
    return {
      background: '#ecfdf5',
      color: '#065f46',
      border: '1px solid #a7f3d0',
    }
  }

  if (normalized === 'pending') {
    return {
      background: '#fffbeb',
      color: '#92400e',
      border: '1px solid #fde68a',
    }
  }

  if (normalized === 'cancelled' || normalized === 'failed') {
    return {
      background: '#fef2f2',
      color: '#991b1b',
      border: '1px solid #fecaca',
    }
  }

  return {
    background: '#eff6ff',
    color: '#1d4ed8',
    border: '1px solid #bfdbfe',
  }
}

export default function OrdersPage() {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['orders'],
    queryFn: getOrders,
  })

  if (isLoading) return <div className="spinner">Loading orders…</div>

  if (isError) {
    return (
      <main className="container" style={{ paddingTop: 24, paddingBottom: 48 }}>
        <div className="error-msg">Could not load your orders.</div>
      </main>
    )
  }

  const orders = data ?? []

  return (
    <main className="container" style={{ paddingTop: 24, paddingBottom: 48 }}>
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          gap: 16,
          marginBottom: 20,
          flexWrap: 'wrap',
        }}
      >
        <div>
          <h1 className="page-title" style={{ marginBottom: 4 }}>
            My Orders
          </h1>
          <p style={{ color: '#6b7280' }}>Review your orders and payment status.</p>
        </div>
        <Link to="/products">
          <button className="btn-primary">Shop more</button>
        </Link>
      </div>

      {orders.length === 0 ? (
        <div className="card" style={{ textAlign: 'center', padding: '48px 24px' }}>
          <div style={{ fontSize: 40, marginBottom: 12 }}>📦</div>
          <p style={{ color: '#6b7280', marginBottom: 16 }}>You have not placed any orders yet.</p>
          <Link to="/products">
            <button className="btn-primary">Start shopping</button>
          </Link>
        </div>
      ) : (
        <div style={{ display: 'grid', gap: 16 }}>
          {orders.map((order) => (
            <Link key={order.id} to={`/orders/${order.id}`} style={{ textDecoration: 'none', color: 'inherit' }}>
              <section className="card" style={{ cursor: 'pointer', transition: 'box-shadow 0.2s' }} onMouseEnter={(e) => {
                e.currentTarget.style.boxShadow = '0 4px 6px rgba(0, 0, 0, 0.1)'
              }} onMouseLeave={(e) => {
                e.currentTarget.style.boxShadow = '0 1px 3px rgba(0, 0, 0, 0.1)'
              }}>
              <div
                style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  gap: 16,
                  alignItems: 'flex-start',
                  flexWrap: 'wrap',
                  marginBottom: 16,
                }}
              >
                <div>
                  <div style={{ fontSize: 12, color: '#6b7280', textTransform: 'uppercase', letterSpacing: '0.08em' }}>
                    Order Reference
                  </div>
                  <div style={{ fontWeight: 700, marginBottom: 6 }}>{toOrderReference(order.id)}</div>
                  <div style={{ fontSize: 12, color: '#6b7280', marginBottom: 6 }}>
                    System ID: {order.id}
                  </div>
                  <div style={{ fontSize: 14, color: '#6b7280' }}>
                    Placed on {new Date(order.createdAt).toLocaleString()}
                  </div>
                </div>

                <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                  <span
                    style={{
                      ...statusStyle(order.status),
                      borderRadius: 999,
                      padding: '6px 10px',
                      fontSize: 12,
                      fontWeight: 700,
                    }}
                  >
                    {order.status}
                  </span>
                  <span
                    style={{
                      ...statusStyle(order.paymentStatus),
                      borderRadius: 999,
                      padding: '6px 10px',
                      fontSize: 12,
                      fontWeight: 700,
                    }}
                  >
                    Payment {order.paymentStatus}
                  </span>
                </div>
              </div>

              <div
                style={{
                  display: 'grid',
                  gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
                  gap: 12,
                  marginBottom: 16,
                }}
              >
                <div>
                  <div style={{ fontSize: 12, color: '#6b7280', textTransform: 'uppercase', letterSpacing: '0.08em' }}>
                    Payment Method
                  </div>
                  <div style={{ fontWeight: 600 }}>{order.paymentMethod}</div>
                </div>
                <div>
                  <div style={{ fontSize: 12, color: '#6b7280', textTransform: 'uppercase', letterSpacing: '0.08em' }}>
                    Payment Reference
                  </div>
                  <div style={{ fontWeight: 600 }}>{order.paymentReference || 'Not applicable'}</div>
                </div>
                <div>
                  <div style={{ fontSize: 12, color: '#6b7280', textTransform: 'uppercase', letterSpacing: '0.08em' }}>
                    Total
                  </div>
                  <div style={{ fontWeight: 700 }}>${order.totalAmount.toFixed(2)}</div>
                </div>
              </div>

              <div style={{ display: 'grid', gap: 10 }}>
                {order.items.map((item) => (
                  <div
                    key={`${order.id}-${item.productId}`}
                    style={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      gap: 12,
                      borderTop: '1px solid #f3f4f6',
                      paddingTop: 10,
                    }}
                  >
                    <div>
                      <div style={{ fontWeight: 600 }}>{item.productName}</div>
                      <div style={{ fontSize: 14, color: '#6b7280' }}>
                        ${item.unitPrice.toFixed(2)} × {item.quantity}
                      </div>
                    </div>
                    <strong>${item.totalPrice.toFixed(2)}</strong>
                  </div>
                ))}
              </div>
            </section>
          </Link>
          ))}
        </div>
      )}
    </main>
  )
}
