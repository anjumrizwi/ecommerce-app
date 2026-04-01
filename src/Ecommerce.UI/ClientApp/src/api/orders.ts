import api from './client'

export type OrderItem = {
  productId: string
  productName: string
  unitPrice: number
  quantity: number
  totalPrice: number
}

export type Order = {
  id: string
  customerId: string
  status: string
  paymentMethod: string
  paymentStatus: string
  paymentReference?: string | null
  totalAmount: number
  items: OrderItem[]
  createdAt: string
}

export const getOrders = async (): Promise<Order[]> => {
  const { data } = await api.get<Order[]>('/orders')
  return data
}

export const getOrderById = async (id: string): Promise<Order> => {
  const { data } = await api.get<Order>(`/orders/${id}`)
  return data
}

export const markOrderAsDelivered = async (id: string): Promise<void> => {
  await api.patch(`/orders/${id}/deliver`)
}
