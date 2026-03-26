import api from './client'

// ─── Types (match Ecommerce.API ProductResponse record) ──────────────────────

export type Product = {
  id: string          // Guid
  name: string
  description: string
  price: number
  stockQuantity: number
  status: string
  createdAt: string
}

export type PaginatedProducts = {
  items: Product[]
  totalCount: number
  pageNumber: number
  pageSize: number
}

export type CreateProductRequest = {
  name: string
  description?: string
  price: number
  stockQuantity: number
}

// ─── API calls ────────────────────────────────────────────────────────────────

export const getProducts = async (): Promise<Product[]> => {
  const { data } = await api.get<Product[]>('/products')
  return data
}

export const getProductsPaged = async (
  pageNumber = 1,
  pageSize = 20,
): Promise<PaginatedProducts> => {
  const { data } = await api.get<PaginatedProducts>('/products/paged', {
    params: { pageNumber, pageSize },
  })
  return data
}

export const getProductById = async (id: string): Promise<Product> => {
  const { data } = await api.get<Product>(`/products/${id}`)
  return data
}
