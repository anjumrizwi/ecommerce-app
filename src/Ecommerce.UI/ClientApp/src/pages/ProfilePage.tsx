import { FormEvent, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { getProfile, updateProfile } from '../api/profile'
import { getOrders } from '../api/orders'
import { toOrderReference } from '../utils/orderReference'

export default function ProfilePage() {
  const queryClient = useQueryClient()
  const [physicalAddress, setPhysicalAddress] = useState('')
  const [pinCode, setPinCode] = useState('')
  const [country, setCountry] = useState('')
  const [state, setState] = useState('')
  const [googleMapLink, setGoogleMapLink] = useState('')
  const [formError, setFormError] = useState<string | null>(null)

  const {
    data: profile,
    isLoading: isProfileLoading,
    isError: isProfileError,
  } = useQuery({
    queryKey: ['profile'],
    queryFn: getProfile,
  })

  const {
    data: orders,
    isLoading: isOrdersLoading,
    isError: isOrdersError,
  } = useQuery({
    queryKey: ['orders'],
    queryFn: getOrders,
  })

  useEffect(() => {
    if (!profile) {
      return
    }

    setPhysicalAddress(profile.physicalAddress ?? '')
    setPinCode(profile.pinCode ?? '')
    setCountry(profile.country ?? '')
    setState(profile.state ?? '')
    setGoogleMapLink(profile.googleMapLink ?? '')
  }, [profile])

  const saveProfileMutation = useMutation({
    mutationFn: updateProfile,
    onSuccess: async (updated) => {
      setFormError(null)
      setPhysicalAddress(updated.physicalAddress ?? '')
      setPinCode(updated.pinCode ?? '')
      setCountry(updated.country ?? '')
      setState(updated.state ?? '')
      setGoogleMapLink(updated.googleMapLink ?? '')
      await queryClient.invalidateQueries({ queryKey: ['profile'] })
    },
    onError: (error: any) => {
      setFormError(error?.response?.data?.message ?? 'Could not save profile. Please try again.')
    },
  })

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    await saveProfileMutation.mutateAsync({
      physicalAddress: physicalAddress.trim() || null,
      pinCode: pinCode.trim() || null,
      country: country.trim() || null,
      state: state.trim() || null,
      googleMapLink: googleMapLink.trim() || null,
    })
  }

  if (isProfileLoading) return <div className="spinner">Loading profile…</div>

  if (isProfileError || !profile) {
    return (
      <main className="container" style={{ paddingTop: 24, paddingBottom: 48 }}>
        <div className="error-msg">Could not load your profile.</div>
      </main>
    )
  }

  const recentOrders = (orders ?? []).slice(0, 5)

  return (
    <main className="container" style={{ paddingTop: 24, paddingBottom: 48 }}>
      <h1 className="page-title">My Profile</h1>

      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'minmax(0, 1fr) minmax(0, 1fr)',
          gap: 24,
        }}
      >
        <section className="card">
          <h2 style={{ marginBottom: 16 }}>Profile Details</h2>

          <div style={{ display: 'grid', gap: 12, marginBottom: 20 }}>
            <div>
              <div style={{ fontSize: 12, color: '#6b7280', textTransform: 'uppercase' }}>Name</div>
              <div style={{ fontWeight: 600 }}>{profile.firstName} {profile.lastName}</div>
            </div>
            <div>
              <div style={{ fontSize: 12, color: '#6b7280', textTransform: 'uppercase' }}>Email</div>
              <div style={{ fontWeight: 600 }}>{profile.email}</div>
            </div>
            <div>
              <div style={{ fontSize: 12, color: '#6b7280', textTransform: 'uppercase' }}>Role</div>
              <div style={{ fontWeight: 600 }}>{profile.role}</div>
            </div>
          </div>

          <form onSubmit={handleSubmit}>
            {formError ? <div className="error-msg">{formError}</div> : null}

            <div className="field">
              <label htmlFor="physicalAddress">Physical Address</label>
              <textarea
                id="physicalAddress"
                value={physicalAddress}
                onChange={(event) => setPhysicalAddress(event.target.value)}
                placeholder="House / street / city / postal code"
                rows={4}
                style={{ resize: 'vertical' }}
              />
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 12 }}>
              <div className="field" style={{ marginBottom: 0 }}>
                <label htmlFor="pinCode">PinCode</label>
                <input
                  id="pinCode"
                  type="text"
                  value={pinCode}
                  onChange={(event) => setPinCode(event.target.value)}
                  placeholder="400001"
                />
              </div>

              <div className="field" style={{ marginBottom: 0 }}>
                <label htmlFor="state">State</label>
                <input
                  id="state"
                  type="text"
                  value={state}
                  onChange={(event) => setState(event.target.value)}
                  placeholder="Maharashtra"
                />
              </div>

              <div className="field" style={{ marginBottom: 0 }}>
                <label htmlFor="country">Country</label>
                <input
                  id="country"
                  type="text"
                  value={country}
                  onChange={(event) => setCountry(event.target.value)}
                  placeholder="India"
                />
              </div>
            </div>

            <div className="field">
              <label htmlFor="googleMapLink">Google Map Link</label>
              <input
                id="googleMapLink"
                type="url"
                value={googleMapLink}
                onChange={(event) => setGoogleMapLink(event.target.value)}
                placeholder="https://maps.google.com/..."
              />
            </div>

            <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
              <button className="btn-primary" type="submit" disabled={saveProfileMutation.isPending}>
                {saveProfileMutation.isPending ? 'Saving…' : 'Save Profile'}
              </button>
              {profile.googleMapLink && (
                <a href={profile.googleMapLink} target="_blank" rel="noreferrer">
                  Open saved map
                </a>
              )}
            </div>
          </form>
        </section>

        <section className="card">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
            <h2>Order History</h2>
            <Link to="/orders">View all orders</Link>
          </div>

          {isOrdersLoading ? <div className="spinner">Loading order history…</div> : null}

          {isOrdersError ? <div className="error-msg">Could not load order history.</div> : null}

          {!isOrdersLoading && !isOrdersError && recentOrders.length === 0 ? (
            <p style={{ color: '#6b7280' }}>No orders yet.</p>
          ) : null}

          <div style={{ display: 'grid', gap: 10 }}>
            {recentOrders.map((order) => (
              <Link
                key={order.id}
                to={`/orders/${order.id}`}
                style={{
                  textDecoration: 'none',
                  color: 'inherit',
                  border: '1px solid #e5e7eb',
                  borderRadius: 8,
                  padding: 12,
                  display: 'block',
                }}
              >
                <div style={{ fontWeight: 700 }}>{toOrderReference(order.id)}</div>
                <div style={{ fontSize: 12, color: '#6b7280' }}>System ID: {order.id}</div>
                <div style={{ fontSize: 13, color: '#6b7280', marginTop: 6 }}>
                  {order.status} • {new Date(order.createdAt).toLocaleString()} • ${order.totalAmount.toFixed(2)}
                </div>
              </Link>
            ))}
          </div>
        </section>
      </div>
    </main>
  )
}
