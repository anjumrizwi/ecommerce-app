import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { getOrderById, markOrderAsDelivered } from '../api/orders'
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

export default function OrderDetailsPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [actionError, setActionError] = useState<string | null>(null)

  const { data: order, isLoading, isError } = useQuery({
    queryKey: ['order', id],
    queryFn: () => getOrderById(id!),
    enabled: !!id,
  })

  const deliverMutation = useMutation({
    mutationFn: () => markOrderAsDelivered(id!),
    onSuccess: async () => {
      setActionError(null)
      await queryClient.invalidateQueries({ queryKey: ['order', id] })
      await queryClient.invalidateQueries({ queryKey: ['orders'] })
    },
    onError: () => {
      setActionError('Could not update delivery status. Please try again.')
    },
  })

  if (isLoading) return <div className="spinner">Loading order…</div>

  if (isError || !order) {
    return (
      <main className="container" style={{ paddingTop: 24, paddingBottom: 48 }}>
        <button
          onClick={() => navigate('/orders')}
          style={{
            background: '#f3f4f6',
            border: '1px solid #e5e7eb',
            borderRadius: 4,
            padding: '8px 16px',
            cursor: 'pointer',
            fontSize: 14,
            marginBottom: 16,
          }}
        >
          ← Back to Orders
        </button>
        <div className="error-msg">Could not load order details.</div>
      </main>
    )
  }

  const normalizedStatus = order.status.toLowerCase()
  const isCod = order.paymentMethod.toLowerCase() === 'cashondelivery'
  const isPaymentPending = order.paymentStatus.toLowerCase() === 'pending'
  const canMarkDelivered = isCod
    && isPaymentPending
    && (normalizedStatus === 'confirmed' || normalizedStatus === 'processing' || normalizedStatus === 'shipped')

  return (
    <main className="container" style={{ paddingTop: 24, paddingBottom: 48 }}>
      <button
        onClick={() => navigate('/orders')}
        style={{
          background: '#f3f4f6',
          border: '1px solid #e5e7eb',
          borderRadius: 4,
          padding: '8px 16px',
          cursor: 'pointer',
          fontSize: 14,
          marginBottom: 20,
          fontWeight: 500,
        }}
      >
        ← Back to Orders
      </button>

      <div style={{ marginBottom: 24 }}>
        <h1 className="page-title" style={{ marginBottom: 8 }}>
          Order {toOrderReference(order.id)}
        </h1>
        <p style={{ color: '#6b7280' }}>
          Placed on {new Date(order.createdAt).toLocaleString()}
        </p>
        <p style={{ color: '#6b7280', fontSize: 13, marginTop: 6 }}>
          System ID: {order.id}
        </p>

        {canMarkDelivered && (
          <div style={{ marginTop: 16 }}>
            <button
              type="button"
              className="btn-primary"
              onClick={() => {
                setActionError(null)
                deliverMutation.mutate()
              }}
              disabled={deliverMutation.isPending}
            >
              {deliverMutation.isPending ? 'Updating delivery…' : 'Confirm Delivery (COD)'}
            </button>
            {actionError && (
              <div className="error-msg" style={{ marginTop: 12 }}>
                {actionError}
              </div>
            )}
          </div>
        )}
      </div>

      <div className="card" style={{ marginBottom: 24 }}>
        <div
          style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
            gap: 24,
            marginBottom: 24,
          }}
        >
          <div>
            <div style={{ fontSize: 12, color: '#6b7280', textTransform: 'uppercase', letterSpacing: '0.08em', marginBottom: 6 }}>
              Order Status
            </div>
            <span
              style={{
                ...statusStyle(order.status),
                borderRadius: 999,
                padding: '8px 12px',
                fontSize: 13,
                fontWeight: 700,
                display: 'inline-block',
              }}
            >
              {order.status}
            </span>
          </div>

          <div>
            <div style={{ fontSize: 12, color: '#6b7280', textTransform: 'uppercase', letterSpacing: '0.08em', marginBottom: 6 }}>
              Payment Status
            </div>
            <span
              style={{
                ...statusStyle(order.paymentStatus),
                borderRadius: 999,
                padding: '8px 12px',
                fontSize: 13,
                fontWeight: 700,
                display: 'inline-block',
              }}
            >
              Payment {order.paymentStatus}
            </span>
          </div>

          <div>
            <div style={{ fontSize: 12, color: '#6b7280', textTransform: 'uppercase', letterSpacing: '0.08em', marginBottom: 6 }}>
              Payment Method
            </div>
            <div style={{ fontWeight: 600, fontSize: 14 }}>{order.paymentMethod}</div>
          </div>
        </div>

        {order.paymentReference && (
          <div style={{ paddingTop: 16, borderTop: '1px solid #e5e7eb', marginBottom: 16 }}>
            <div style={{ fontSize: 12, color: '#6b7280', textTransform: 'uppercase', letterSpacing: '0.08em', marginBottom: 6 }}>
              Payment Reference
            </div>
            <div style={{ fontWeight: 600, fontSize: 14, fontFamily: 'monospace', color: '#374151' }}>
              {order.paymentReference}
            </div>
          </div>
        )}
      </div>

      <div className="card" style={{ marginBottom: 24 }}>
        <h2 style={{ marginBottom: 16, fontSize: 18, fontWeight: 700 }}>Order Items</h2>

        <div style={{ display: 'grid', gap: 16 }}>
          {order.items.map((item) => (
            <div
              key={`${order.id}-${item.productId}`}
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'flex-start',
                gap: 16,
                paddingBottom: 16,
                borderBottom: '1px solid #e5e7eb',
              }}
            >
              <div style={{ flex: 1 }}>
                <div style={{ fontWeight: 600, marginBottom: 4 }}>{item.productName}</div>
                <div style={{ fontSize: 13, color: '#6b7280' }}>
                  ${item.unitPrice.toFixed(2)} each
                </div>
              </div>

              <div style={{ textAlign: 'right' }}>
                <div style={{ fontSize: 13, color: '#6b7280', marginBottom: 4 }}>
                  Qty: {item.quantity}
                </div>
                <div style={{ fontWeight: 700, fontSize: 14 }}>
                  ${item.totalPrice.toFixed(2)}
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="card" style={{ background: '#f9fafb' }}>
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            gap: 12,
          }}
        >
          <div style={{ fontSize: 16, fontWeight: 600 }}>Order Total</div>
          <div style={{ fontSize: 24, fontWeight: 700 }}>${order.totalAmount.toFixed(2)}</div>
        </div>
      </div>
    </main>
  )
}
