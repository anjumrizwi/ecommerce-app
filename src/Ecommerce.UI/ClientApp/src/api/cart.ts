import api from './client'

// ─── Types (match Ecommerce.API CartResponse records) ────────────────────────

export type CartItem = {
  productId: string   // Guid
  productName: string
  unitPrice: number
  quantity: number
  totalPrice: number
}

export type Cart = {
  id: string          // Guid
  userId: string      // Guid
  items: CartItem[]
  totalAmount: number
}

export type AddToCartRequest = {
  productId: string
  quantity: number
}

export type CheckoutRequest = {
  paymentMethod: 'Upi' | 'CashOnDelivery'
  paymentReference?: string
}

export type CheckoutResponse = {
  orderId: string
  paymentMethod: string
  paymentStatus: string
}

// ─── API calls ────────────────────────────────────────────────────────────────

export const getCart = async (): Promise<Cart> => {
  const { data } = await api.get<Cart>('/cart')
  return data
}

export const addToCart = async (
  productId: string,
  quantity: number,
): Promise<void> => {
  await api.post('/cart/items', { productId, quantity } satisfies AddToCartRequest)
}

export const removeFromCart = async (productId: string): Promise<void> => {
  await api.delete(`/cart/items/${productId}`)
}

export const checkoutCart = async (
  request: CheckoutRequest,
): Promise<CheckoutResponse> => {
  const { data } = await api.post<CheckoutResponse>('/cart/checkout', request)
  return data
}
