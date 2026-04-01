import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getProductsPaged, updateProduct, type Product, type UpdateProductRequest } from '../api/products'

type EditingProduct = Partial<Product> & { id: string }

export default function AdminProductsPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [page, setPage] = useState(1)
  const pageSize = 20
  const [editingMap, setEditingMap] = useState<Record<string, EditingProduct>>({})
  const [savingId, setSavingId] = useState<string | null>(null)
  const [saveError, setSaveError] = useState<string | null>(null)

  const { data, isLoading, isError } = useQuery({
    queryKey: ['admin-products', page],
    queryFn: () => getProductsPaged(page, pageSize),
  })

  const updateMutation = useMutation({
    mutationFn: (vars: { id: string; request: UpdateProductRequest }) =>
      updateProduct(vars.id, vars.request),
    onSuccess: () => {
      setSavingId(null)
      setSaveError(null)
      setEditingMap((prev) => {
        const updated = { ...prev }
        delete updated[savingId!]
        return updated
      })
      queryClient.invalidateQueries({ queryKey: ['admin-products'] })
    },
    onError: (error: any) => {
      setSaveError(error?.response?.data?.message || 'Failed to save product')
      setSavingId(null)
    },
  })

  const handleEdit = (product: Product) => {
    setEditingMap((prev) => ({
      ...prev,
      [product.id]: { ...product },
    }))
  }

  const handleCancel = (id: string) => {
    setEditingMap((prev) => {
      const updated = { ...prev }
      delete updated[id]
      return updated
    })
  }

  const handleSave = async (id: string) => {
    const edited = editingMap[id]
    if (!edited) return

    setSavingId(id)
    updateMutation.mutate({
      id,
      request: {
        name: edited.name || '',
        description: edited.description || '',
        price: edited.price || 0,
        stockQuantity: edited.stockQuantity || 0,
      },
    })
  }

  const handleFieldChange = (id: string, field: keyof Product, value: any) => {
    setEditingMap((prev) => ({
      ...prev,
      [id]: {
        ...prev[id],
        [field]: field === 'price' || field === 'stockQuantity' ? Number(value) || 0 : value,
      },
    }))
  }

  const totalPages = data ? Math.ceil(data.totalCount / pageSize) : 1

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
        <h1 className="page-title" style={{ margin: 0 }}>
          Admin – Manage Products
        </h1>
        <button
          onClick={() => navigate('/admin/products/add')}
          style={{
            padding: '8px 16px',
            background: '#111827',
            color: '#fff',
            border: 'none',
            borderRadius: 6,
            cursor: 'pointer',
            fontWeight: 500,
          }}
        >
          + Add Product
        </button>
      </div>

      {saveError && (
        <div
          style={{
            background: '#fee2e2',
            color: '#991b1b',
            padding: 12,
            borderRadius: 6,
            marginBottom: 16,
          }}
        >
          {saveError}
        </div>
      )}

      <div style={{ overflowX: 'auto' }}>
        <table
          style={{
            width: '100%',
            borderCollapse: 'collapse',
            fontSize: 14,
          }}
        >
          <thead>
            <tr style={{ borderBottom: '2px solid #e5e7eb', background: '#f9fafb' }}>
              <th style={{ padding: 12, textAlign: 'left' }}>Product Name</th>
              <th style={{ padding: 12, textAlign: 'left' }}>Description</th>
              <th style={{ padding: 12, textAlign: 'right' }}>Price</th>
              <th style={{ padding: 12, textAlign: 'right' }}>Stock</th>
              <th style={{ padding: 12, textAlign: 'center' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {data?.items.map((product) => {
              const isEditing = !!editingMap[product.id]
              const edited = editingMap[product.id] || product
              return (
                <tr key={product.id} style={{ borderBottom: '1px solid #e5e7eb' }}>
                  <td style={{ padding: 12 }}>
                    {isEditing ? (
                      <input
                        type="text"
                        value={edited.name || ''}
                        onChange={(e) => handleFieldChange(product.id, 'name', e.target.value)}
                        style={{
                          width: '100%',
                          padding: 6,
                          border: '1px solid #d1d5db',
                          borderRadius: 4,
                          fontSize: 14,
                        }}
                      />
                    ) : (
                      product.name
                    )}
                  </td>
                  <td style={{ padding: 12, maxWidth: 300 }}>
                    {isEditing ? (
                      <textarea
                        value={edited.description || ''}
                        onChange={(e) => handleFieldChange(product.id, 'description', e.target.value)}
                        style={{
                          width: '100%',
                          padding: 6,
                          border: '1px solid #d1d5db',
                          borderRadius: 4,
                          fontSize: 14,
                          minHeight: 40,
                          fontFamily: 'inherit',
                        }}
                      />
                    ) : (
                      <span
                        style={{
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                          display: 'block',
                          whiteSpace: 'nowrap',
                        }}
                        title={product.description}
                      >
                        {product.description || '—'}
                      </span>
                    )}
                  </td>
                  <td style={{ padding: 12, textAlign: 'right' }}>
                    {isEditing ? (
                      <input
                        type="number"
                        step="0.01"
                        min="0"
                        value={edited.price || 0}
                        onChange={(e) => handleFieldChange(product.id, 'price', e.target.value)}
                        style={{
                          width: 100,
                          padding: 6,
                          border: '1px solid #d1d5db',
                          borderRadius: 4,
                          fontSize: 14,
                          textAlign: 'right',
                        }}
                      />
                    ) : (
                      `$${product.price.toFixed(2)}`
                    )}
                  </td>
                  <td style={{ padding: 12, textAlign: 'right' }}>
                    {isEditing ? (
                      <input
                        type="number"
                        min="0"
                        value={edited.stockQuantity || 0}
                        onChange={(e) => handleFieldChange(product.id, 'stockQuantity', e.target.value)}
                        style={{
                          width: 80,
                          padding: 6,
                          border: '1px solid #d1d5db',
                          borderRadius: 4,
                          fontSize: 14,
                          textAlign: 'right',
                        }}
                      />
                    ) : (
                      product.stockQuantity
                    )}
                  </td>
                  <td style={{ padding: 12, textAlign: 'center' }}>
                    {isEditing ? (
                      <div style={{ display: 'flex', gap: 8, justifyContent: 'center' }}>
                        <button
                          onClick={() => handleSave(product.id)}
                          disabled={savingId === product.id}
                          style={{
                            padding: '6px 12px',
                            background: '#10b981',
                            color: '#fff',
                            border: 'none',
                            borderRadius: 4,
                            cursor: savingId === product.id ? 'not-allowed' : 'pointer',
                            fontSize: 12,
                            fontWeight: 500,
                            opacity: savingId === product.id ? 0.7 : 1,
                          }}
                        >
                          {savingId === product.id ? 'Saving…' : 'Save'}
                        </button>
                        <button
                          onClick={() => handleCancel(product.id)}
                          disabled={savingId === product.id}
                          style={{
                            padding: '6px 12px',
                            background: '#e5e7eb',
                            color: '#111827',
                            border: 'none',
                            borderRadius: 4,
                            cursor: savingId === product.id ? 'not-allowed' : 'pointer',
                            fontSize: 12,
                            opacity: savingId === product.id ? 0.7 : 1,
                          }}
                        >
                          Cancel
                        </button>
                      </div>
                    ) : (
                      <button
                        onClick={() => handleEdit(product)}
                        style={{
                          padding: '6px 12px',
                          background: '#3b82f6',
                          color: '#fff',
                          border: 'none',
                          borderRadius: 4,
                          cursor: 'pointer',
                          fontSize: 12,
                          fontWeight: 500,
                        }}
                      >
                        Edit
                      </button>
                    )}
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      <div style={{ display: 'flex', gap: 8, justifyContent: 'center', marginTop: 24 }}>
        <button
          onClick={() => setPage(1)}
          disabled={page === 1}
          style={{
            padding: '8px 12px',
            cursor: page === 1 ? 'not-allowed' : 'pointer',
            opacity: page === 1 ? 0.5 : 1,
          }}
        >
          First
        </button>
        <button
          onClick={() => setPage((p) => Math.max(1, p - 1))}
          disabled={page === 1}
          style={{
            padding: '8px 12px',
            cursor: page === 1 ? 'not-allowed' : 'pointer',
            opacity: page === 1 ? 0.5 : 1,
          }}
        >
          Previous
        </button>
        <span style={{ padding: '8px 12px', fontWeight: 500 }}>
          Page {page} of {totalPages}
        </span>
        <button
          onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
          disabled={page === totalPages}
          style={{
            padding: '8px 12px',
            cursor: page === totalPages ? 'not-allowed' : 'pointer',
            opacity: page === totalPages ? 0.5 : 1,
          }}
        >
          Next
        </button>
        <button
          onClick={() => setPage(totalPages)}
          disabled={page === totalPages}
          style={{
            padding: '8px 12px',
            cursor: page === totalPages ? 'not-allowed' : 'pointer',
            opacity: page === totalPages ? 0.5 : 1,
          }}
        >
          Last
        </button>
      </div>
    </main>
  )
}
