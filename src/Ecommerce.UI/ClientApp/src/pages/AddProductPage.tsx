import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import { createProduct, type CreateProductRequest } from '../api/products'

export default function AddProductPage() {
  const navigate = useNavigate()
  const [formData, setFormData] = useState<CreateProductRequest>({
    name: '',
    description: '',
    price: 0,
    stockQuantity: 0,
  })
  const [errors, setErrors] = useState<Record<string, string>>({})

  const createMutation = useMutation({
    mutationFn: (request: CreateProductRequest) => createProduct(request),
    onSuccess: () => {
      navigate('/admin/products', { replace: true })
    },
    onError: (error: any) => {
      const message = error?.response?.data?.message || 'Failed to create product'
      setErrors({ form: message })
    },
  })

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {}

    if (!formData.name?.trim()) {
      newErrors.name = 'Product name is required'
    }
    if (formData.price < 0) {
      newErrors.price = 'Price must be 0 or greater'
    }
    if (formData.stockQuantity < 0) {
      newErrors.stockQuantity = 'Stock quantity must be 0 or greater'
    }

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!validateForm()) return
    createMutation.mutate(formData)
  }

  const handleChange = (field: keyof CreateProductRequest, value: any) => {
    setFormData((prev) => ({
      ...prev,
      [field]: field === 'price' || field === 'stockQuantity' ? Number(value) || 0 : value,
    }))
    if (errors[field]) {
      const newErrors = { ...errors }
      delete newErrors[field]
      setErrors(newErrors)
    }
  }

  const inputStyle = {
    width: '100%',
    padding: '10px 12px',
    border: '1px solid #d1d5db',
    borderRadius: 6,
    fontSize: 14,
    fontFamily: 'inherit',
    boxSizing: 'border-box' as const,
  }

  const labelStyle = {
    display: 'block',
    marginBottom: 6,
    fontWeight: 500,
    fontSize: 14,
    color: '#111827',
  }

  const fieldWrapperStyle = {
    marginBottom: 20,
  }

  return (
    <main className="container" style={{ paddingTop: 24, paddingBottom: 48, maxWidth: 600 }}>
      <h1 className="page-title" style={{ marginBottom: 24 }}>Add New Product</h1>

      {errors.form && (
        <div
          style={{
            background: '#fee2e2',
            color: '#991b1b',
            padding: 12,
            borderRadius: 6,
            marginBottom: 20,
          }}
        >
          {errors.form}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        {/* Product Name */}
        <div style={fieldWrapperStyle}>
          <label style={labelStyle}>
            Product Name <span style={{ color: '#ef4444' }}>*</span>
          </label>
          <input
            type="text"
            value={formData.name}
            onChange={(e) => handleChange('name', e.target.value)}
            placeholder="e.g., Wireless Headphones"
            style={{
              ...inputStyle,
              borderColor: errors.name ? '#ef4444' : '#d1d5db',
            }}
          />
          {errors.name && (
            <div style={{ color: '#ef4444', fontSize: 12, marginTop: 4 }}>{errors.name}</div>
          )}
        </div>

        {/* Description */}
        <div style={fieldWrapperStyle}>
          <label style={labelStyle}>Description</label>
          <textarea
            value={formData.description || ''}
            onChange={(e) => handleChange('description', e.target.value)}
            placeholder="Product description (optional)"
            style={{
              ...inputStyle,
              minHeight: 100,
            }}
          />
        </div>

        {/* Price */}
        <div style={fieldWrapperStyle}>
          <label style={labelStyle}>
            Price (USD) <span style={{ color: '#ef4444' }}>*</span>
          </label>
          <input
            type="number"
            step="0.01"
            min="0"
            value={formData.price}
            onChange={(e) => handleChange('price', e.target.value)}
            placeholder="0.00"
            style={{
              ...inputStyle,
              borderColor: errors.price ? '#ef4444' : '#d1d5db',
            }}
          />
          {errors.price && (
            <div style={{ color: '#ef4444', fontSize: 12, marginTop: 4 }}>{errors.price}</div>
          )}
        </div>

        {/* Stock Quantity */}
        <div style={fieldWrapperStyle}>
          <label style={labelStyle}>
            Stock Quantity <span style={{ color: '#ef4444' }}>*</span>
          </label>
          <input
            type="number"
            min="0"
            value={formData.stockQuantity}
            onChange={(e) => handleChange('stockQuantity', e.target.value)}
            placeholder="0"
            style={{
              ...inputStyle,
              borderColor: errors.stockQuantity ? '#ef4444' : '#d1d5db',
            }}
          />
          {errors.stockQuantity && (
            <div style={{ color: '#ef4444', fontSize: 12, marginTop: 4 }}>{errors.stockQuantity}</div>
          )}
        </div>

        {/* Actions */}
        <div style={{ display: 'flex', gap: 12, marginTop: 32 }}>
          <button
            type="submit"
            disabled={createMutation.isPending}
            style={{
              flex: 1,
              padding: '12px 24px',
              background: '#111827',
              color: '#fff',
              border: 'none',
              borderRadius: 6,
              cursor: createMutation.isPending ? 'not-allowed' : 'pointer',
              fontWeight: 600,
              fontSize: 14,
              opacity: createMutation.isPending ? 0.7 : 1,
            }}
          >
            {createMutation.isPending ? 'Creating…' : 'Create Product'}
          </button>
          <button
            type="button"
            onClick={() => navigate('/admin/products')}
            style={{
              flex: 1,
              padding: '12px 24px',
              background: '#e5e7eb',
              color: '#111827',
              border: 'none',
              borderRadius: 6,
              cursor: 'pointer',
              fontWeight: 600,
              fontSize: 14,
            }}
          >
            Cancel
          </button>
        </div>
      </form>
    </main>
  )
}
