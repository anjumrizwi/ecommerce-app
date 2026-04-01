import { useQuery, useQueryClient } from '@tanstack/react-query'
import { getCart, removeFromCart } from '../api/cart'
import { Link } from 'react-router-dom'

export default function CartPage() {
  const queryClient = useQueryClient()
  const { data, isLoading, isError } = useQuery({
    queryKey: ['cart'],
    queryFn: getCart,
  })

  const handleRemove = async (productId: string) => {
    await removeFromCart(productId)
    await queryClient.invalidateQueries({ queryKey: ['cart'] })
  }

  if (isLoading) return <div className="spinner">Loading cart…</div>
  if (isError)
    return (
      <div className="container" style={{ paddingTop: 24 }}>
        <div className="error-msg">Could not load cart.</div>
      </div>
    )

  const items = data?.items ?? []

  return (
    <main className="container" style={{ paddingTop: 24, paddingBottom: 48 }}>
      <h1 className="page-title">My Cart</h1>

      {items.length === 0 ? (
        <div
          style={{
            textAlign: 'center',
            padding: '48px 0',
            color: '#9ca3af',
          }}
        >
          <div style={{ fontSize: 48, marginBottom: 12 }}>🛒</div>
          <p style={{ marginBottom: 16 }}>Your cart is empty.</p>
          <Link to="/products">
            <button className="btn-primary">Browse Products</button>
          </Link>
        </div>
      ) : (
        <div style={{ maxWidth: 720 }}>
          {/* Items table */}
          <div className="card" style={{ marginBottom: 20 }}>
            {items.map((item, idx) => (
              <div
                key={item.productId}
                style={{
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  padding: '14px 0',
                  borderBottom:
                    idx < items.length - 1 ? '1px solid #f3f4f6' : 'none',
                }}
              >
                <div>
                  <div style={{ fontWeight: 500, marginBottom: 2 }}>
                    {item.productName}
                  </div>
                  <div style={{ fontSize: 13, color: '#6b7280' }}>
                    ${item.unitPrice.toFixed(2)} × {item.quantity}
                  </div>
                </div>

                <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
                  <strong>${item.totalPrice.toFixed(2)}</strong>
                  <button
                    className="btn-danger"
                    style={{ padding: '4px 12px', fontSize: 12 }}
                    onClick={() => handleRemove(item.productId)}
                  >
                    Remove
                  </button>
                </div>
              </div>
            ))}
          </div>

          {/* Order summary */}
          <div className="card">
            <div
              style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 8 }}
            >
              <span style={{ color: '#6b7280' }}>
                Subtotal ({items.reduce((s, i) => s + i.quantity, 0)} items)
              </span>
              <span>${data?.totalAmount.toFixed(2)}</span>
            </div>
            <hr style={{ border: 'none', borderTop: '1px solid #e5e7eb', margin: '12px 0' }} />
            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                fontWeight: 700,
                fontSize: 18,
                marginBottom: 16,
              }}
            >
              <span>Total</span>
              <span>${data?.totalAmount.toFixed(2)}</span>
            </div>
            <Link to="/checkout" style={{ display: 'block' }}>
              <button className="btn-primary" style={{ width: '100%' }}>
                Proceed to Checkout
              </button>
            </Link>
          </div>
        </div>
      )}
    </main>
  )
}
