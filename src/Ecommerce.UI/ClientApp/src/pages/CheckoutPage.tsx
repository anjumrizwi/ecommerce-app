import { FormEvent, useState } from 'react'
import { Link } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { checkoutCart, getCart, type CheckoutRequest } from '../api/cart'
import { toOrderReference } from '../utils/orderReference'

type PaymentMethod = CheckoutRequest['paymentMethod']

type CheckoutSuccess = {
  orderId: string
  paymentMethod: string
  paymentStatus: string
}

export default function CheckoutPage() {
  const queryClient = useQueryClient()
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>('Upi')
  const [paymentReference, setPaymentReference] = useState('')
  const [formError, setFormError] = useState<string | null>(null)
  const [success, setSuccess] = useState<CheckoutSuccess | null>(null)

  const { data, isLoading, isError } = useQuery({
    queryKey: ['cart'],
    queryFn: getCart,
  })

  const checkoutMutation = useMutation({
    mutationFn: checkoutCart,
    onSuccess: async (result) => {
      setSuccess(result)
      setFormError(null)
      await queryClient.invalidateQueries({ queryKey: ['cart'] })
    },
    onError: (error: any) => {
      setFormError(error?.response?.data?.detail ?? 'Checkout failed. Please try again.')
    },
  })

  const items = data?.items ?? []

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()

    if (items.length === 0) {
      setFormError('Your cart is empty.')
      return
    }

    if (paymentMethod === 'Upi' && !paymentReference.trim()) {
      setFormError('UPI ID is required for UPI checkout.')
      return
    }

    await checkoutMutation.mutateAsync({
      paymentMethod,
      paymentReference: paymentMethod === 'Upi' ? paymentReference.trim() : undefined,
    })
  }

  if (isLoading) return <div className="spinner">Loading checkout…</div>

  if (isError) {
    return (
      <main className="container" style={{ paddingTop: 24, paddingBottom: 48 }}>
        <div className="error-msg">Could not load your cart for checkout.</div>
      </main>
    )
  }

  if (success) {
    return (
      <main className="container" style={{ paddingTop: 24, paddingBottom: 48 }}>
        <div className="card" style={{ maxWidth: 720, margin: '0 auto' }}>
          <h1 className="page-title">Order placed</h1>
          <p style={{ marginBottom: 12 }}>
            Order <strong>{toOrderReference(success.orderId)}</strong> has been created.
          </p>
          <p style={{ marginBottom: 12, color: '#6b7280', fontSize: 13 }}>
            System ID: {success.orderId}
          </p>
          <p style={{ marginBottom: 8 }}>Payment method: {success.paymentMethod}</p>
          <p style={{ marginBottom: 24 }}>Payment status: {success.paymentStatus}</p>
          <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
            <Link to="/orders">
              <button className="btn-primary">View orders</button>
            </Link>
            <Link to="/products">
              <button>Continue shopping</button>
            </Link>
            <Link to="/cart">
              <button>Back to cart</button>
            </Link>
          </div>
        </div>
      </main>
    )
  }

  return (
    <main className="container" style={{ paddingTop: 24, paddingBottom: 48 }}>
      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'minmax(0, 1fr) minmax(320px, 360px)',
          gap: 24,
        }}
      >
        <form className="card" onSubmit={handleSubmit}>
          <h1 className="page-title">Checkout</h1>

          {formError ? <div className="error-msg">{formError}</div> : null}

          <div className="field">
            <label>Payment method</label>
            <div style={{ display: 'grid', gap: 12 }}>
              <label
                style={{
                  display: 'flex',
                  gap: 10,
                  alignItems: 'flex-start',
                  border: '1px solid #e5e7eb',
                  borderRadius: 10,
                  padding: 14,
                }}
              >
                <input
                  type="radio"
                  name="paymentMethod"
                  checked={paymentMethod === 'Upi'}
                  onChange={() => setPaymentMethod('Upi')}
                  style={{ width: 16, marginTop: 4 }}
                />
                <span>
                  <strong>UPI</strong>
                  <span
                    style={{ display: 'block', color: '#6b7280', fontWeight: 400 }}
                  >
                    Pay instantly using your UPI ID.
                  </span>
                </span>
              </label>

              <label
                style={{
                  display: 'flex',
                  gap: 10,
                  alignItems: 'flex-start',
                  border: '1px solid #e5e7eb',
                  borderRadius: 10,
                  padding: 14,
                }}
              >
                <input
                  type="radio"
                  name="paymentMethod"
                  checked={paymentMethod === 'CashOnDelivery'}
                  onChange={() => setPaymentMethod('CashOnDelivery')}
                  style={{ width: 16, marginTop: 4 }}
                />
                <span>
                  <strong>Cash on delivery</strong>
                  <span
                    style={{ display: 'block', color: '#6b7280', fontWeight: 400 }}
                  >
                    Pay when your order arrives.
                  </span>
                </span>
              </label>
            </div>
          </div>

          {paymentMethod === 'Upi' ? (
            <div className="field">
              <label htmlFor="upiId">UPI ID</label>
              <input
                id="upiId"
                type="text"
                placeholder="name@bank"
                value={paymentReference}
                onChange={(event) => setPaymentReference(event.target.value)}
              />
            </div>
          ) : null}

          <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
            <button className="btn-primary" type="submit" disabled={checkoutMutation.isPending}>
              {checkoutMutation.isPending ? 'Placing order…' : 'Place order'}
            </button>
            <Link to="/cart">
              <button type="button">Back to cart</button>
            </Link>
          </div>
        </form>

        <aside className="card" style={{ height: 'fit-content' }}>
          <h2 style={{ fontSize: 20, marginBottom: 16 }}>Order summary</h2>
          <div style={{ display: 'grid', gap: 14 }}>
            {items.map((item) => (
              <div
                key={item.productId}
                style={{ display: 'flex', justifyContent: 'space-between', gap: 12 }}
              >
                <div>
                  <div style={{ fontWeight: 600 }}>{item.productName}</div>
                  <div style={{ color: '#6b7280', fontSize: 14 }}>
                    ${item.unitPrice.toFixed(2)} × {item.quantity}
                  </div>
                </div>
                <strong>${item.totalPrice.toFixed(2)}</strong>
              </div>
            ))}
          </div>

          <hr
            style={{ border: 'none', borderTop: '1px solid #e5e7eb', margin: '16px 0' }}
          />
          <div
            style={{ display: 'flex', justifyContent: 'space-between', fontWeight: 700, fontSize: 18 }}
          >
            <span>Total</span>
            <span>${data?.totalAmount.toFixed(2)}</span>
          </div>
        </aside>
      </div>
    </main>
  )
}
