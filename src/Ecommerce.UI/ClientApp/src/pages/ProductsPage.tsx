import { useState } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { getProductsPaged } from '../api/products'
import { addToCart } from '../api/cart'

export default function ProductsPage() {
  const queryClient = useQueryClient()
  const [page, setPage] = useState(1)
  const pageSize = 12

  const { data, isLoading, isError } = useQuery({
    queryKey: ['products', page],
    queryFn: () => getProductsPaged(page, pageSize),
  })

  const [adding, setAdding] = useState<string | null>(null)
  const [addedMap, setAddedMap] = useState<Record<string, boolean>>({})

  const handleAddToCart = async (productId: string) => {
    setAdding(productId)
    try {
      await addToCart(productId, 1)
      setAddedMap((prev) => ({ ...prev, [productId]: true }))
      await queryClient.invalidateQueries({ queryKey: ['cart'] })
      setTimeout(() => {
        setAddedMap((prev) => ({ ...prev, [productId]: false }))
      }, 2000)
    } finally {
      setAdding(null)
    }
  }

  const totalPages = data
    ? Math.ceil(data.totalCount / pageSize)
    : 1

  if (isLoading) return <div className="spinner">Loading products…</div>
  if (isError)
    return (
      <div className="container" style={{ paddingTop: 24 }}>
        <div className="error-msg">Could not load products. Make sure Ecommerce.API is running.</div>
      </div>
    )

  return (
    <main className="container" style={{ paddingTop: 24, paddingBottom: 48 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', marginBottom: 20 }}>
        <h1 className="page-title" style={{ margin: 0 }}>Products</h1>
        <span style={{ color: '#6b7280', fontSize: 14 }}>
          {data?.totalCount ?? 0} items
        </span>
      </div>

      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))',
          gap: 16,
          marginBottom: 32,
        }}
      >
        {data?.items.map((p) => (
          <article key={p.id} className="card" style={{ display: 'flex', flexDirection: 'column' }}>
            {/* Placeholder image */}
            <div
              style={{
                background: '#f3f4f6',
                borderRadius: 8,
                height: 140,
                marginBottom: 12,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                fontSize: 40,
              }}
            >
              🛍️
            </div>

            <h3 style={{ fontSize: 15, marginBottom: 4, flex: 1 }}>{p.name}</h3>
            <p
              style={{
                fontSize: 13,
                color: '#6b7280',
                marginBottom: 8,
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                display: '-webkit-box',
                WebkitLineClamp: 2,
                WebkitBoxOrient: 'vertical' as const,
              }}
            >
              {p.description || 'No description'}
            </p>

            <div
              style={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                marginBottom: 12,
              }}
            >
              <strong style={{ fontSize: 16 }}>${p.price.toFixed(2)}</strong>
              <span
                style={{
                  fontSize: 11,
                  background: p.stockQuantity > 0 ? '#dcfce7' : '#fee2e2',
                  color: p.stockQuantity > 0 ? '#16a34a' : '#dc2626',
                  padding: '2px 8px',
                  borderRadius: 999,
                }}
              >
                {p.stockQuantity > 0 ? `${p.stockQuantity} in stock` : 'Out of stock'}
              </span>
            </div>

            <button
              className="btn-primary"
              disabled={adding === p.id || p.stockQuantity === 0}
              onClick={() => handleAddToCart(p.id)}
            >
              {addedMap[p.id]
                ? '✓ Added!'
                : adding === p.id
                ? 'Adding…'
                : 'Add to cart'}
            </button>
          </article>
        ))}
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div style={{ display: 'flex', justifyContent: 'center', gap: 8 }}>
          <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1}>
            ← Prev
          </button>
          <span style={{ lineHeight: '34px', fontSize: 14, color: '#6b7280' }}>
            Page {page} of {totalPages}
          </span>
          <button
            onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            disabled={page === totalPages}
          >
            Next →
          </button>
        </div>
      )}
    </main>
  )
}
