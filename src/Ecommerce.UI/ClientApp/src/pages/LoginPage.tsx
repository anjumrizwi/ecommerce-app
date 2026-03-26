import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import axios from 'axios'
import { useAuth } from '../auth/AuthContext'

const schema = z.object({
  email: z.string().email('Enter a valid email'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
})

type FormData = z.infer<typeof schema>

export default function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const { login } = useAuth()
  const [serverError, setServerError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({ resolver: zodResolver(schema) })

  const onSubmit = async (data: FormData) => {
    setServerError(null)
    try {
      await login(data)
      const from =
        (location.state as { from?: string } | null)?.from ?? '/products'
      navigate(from, { replace: true })
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        if (!err.response) {
          setServerError(
            'Cannot reach the API. Make sure Ecommerce.API is running (dotnet run in Ecommerce.API/).'
          )
        } else if (err.response.status === 401) {
          setServerError('Invalid email or password.')
        } else {
          const msg = (err.response.data as { message?: string })?.message
          setServerError(msg ?? `Request failed (${err.response.status}). Please try again.`)
        }
      } else {
        setServerError('An unexpected error occurred. Please try again.')
      }
    }
  }

  return (
    <main
      className="container"
      style={{ maxWidth: 460, paddingTop: 48, paddingBottom: 48 }}
    >
      <div className="card">
        <h2 className="page-title">Sign in to your account</h2>

        <form onSubmit={handleSubmit(onSubmit)} noValidate>
          <div className="field">
            <label htmlFor="email">Email address</label>
            <input
              id="email"
              type="email"
              autoComplete="email"
              {...register('email')}
            />
            {errors.email && <small>{errors.email.message}</small>}
          </div>

          <div className="field">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              {...register('password')}
            />
            {errors.password && <small>{errors.password.message}</small>}
          </div>

          {serverError && <div className="error-msg">{serverError}</div>}

          <button
            type="submit"
            className="btn-primary"
            disabled={isSubmitting}
            style={{ width: '100%', marginTop: 4 }}
          >
            {isSubmitting ? 'Signing in…' : 'Sign in'}
          </button>
        </form>

        <p style={{ marginTop: 16, fontSize: 14, textAlign: 'center' }}>
          Don&apos;t have an account?{' '}
          <Link to="/register">Create one</Link>
        </p>
      </div>
    </main>
  )
}
