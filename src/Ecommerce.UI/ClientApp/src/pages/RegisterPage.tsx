import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useAuth } from '../auth/AuthContext'

const schema = z
  .object({
    firstName: z.string().min(2, 'At least 2 characters'),
    lastName: z.string().min(2, 'At least 2 characters'),
    email: z.string().email('Enter a valid email'),
    password: z.string().min(8, 'At least 8 characters'),
    confirmPassword: z.string().min(8, 'At least 8 characters'),
  })
  .refine((v) => v.password === v.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  })

type FormData = z.infer<typeof schema>

export default function RegisterPage() {
  const navigate = useNavigate()
  const { register: doRegister } = useAuth()
  const [serverError, setServerError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormData>({ resolver: zodResolver(schema) })

  const onSubmit = async (data: FormData) => {
    setServerError(null)
    try {
      await doRegister({
        firstName: data.firstName,
        lastName: data.lastName,
        email: data.email,
        password: data.password,
      })
      navigate('/products', { replace: true })
    } catch (err: unknown) {
      const msg =
        (
          err as {
            response?: { data?: { message?: string } }
          }
        )?.response?.data?.message ?? 'Registration failed. Please try again.'
      setServerError(msg)
    }
  }

  return (
    <main
      className="container"
      style={{ maxWidth: 520, paddingTop: 48, paddingBottom: 48 }}
    >
      <div className="card">
        <h2 className="page-title">Create your account</h2>

        <form onSubmit={handleSubmit(onSubmit)} noValidate>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <div className="field">
              <label htmlFor="firstName">First name</label>
              <input id="firstName" autoComplete="given-name" {...register('firstName')} />
              {errors.firstName && <small>{errors.firstName.message}</small>}
            </div>
            <div className="field">
              <label htmlFor="lastName">Last name</label>
              <input id="lastName" autoComplete="family-name" {...register('lastName')} />
              {errors.lastName && <small>{errors.lastName.message}</small>}
            </div>
          </div>

          <div className="field">
            <label htmlFor="email">Email address</label>
            <input id="email" type="email" autoComplete="email" {...register('email')} />
            {errors.email && <small>{errors.email.message}</small>}
          </div>

          <div className="field">
            <label htmlFor="password">Password</label>
            <input id="password" type="password" autoComplete="new-password" {...register('password')} />
            {errors.password && <small>{errors.password.message}</small>}
          </div>

          <div className="field">
            <label htmlFor="confirmPassword">Confirm password</label>
            <input
              id="confirmPassword"
              type="password"
              autoComplete="new-password"
              {...register('confirmPassword')}
            />
            {errors.confirmPassword && (
              <small>{errors.confirmPassword.message}</small>
            )}
          </div>

          {serverError && <div className="error-msg">{serverError}</div>}

          <button
            type="submit"
            className="btn-primary"
            disabled={isSubmitting}
            style={{ width: '100%', marginTop: 4 }}
          >
            {isSubmitting ? 'Creating account…' : 'Create account'}
          </button>
        </form>

        <p style={{ marginTop: 16, fontSize: 14, textAlign: 'center' }}>
          Already have an account? <Link to="/login">Sign in</Link>
        </p>
      </div>
    </main>
  )
}
