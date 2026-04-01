import api from './client'

export type Profile = {
  userId: string
  firstName: string
  lastName: string
  email: string
  role: string
  physicalAddress?: string | null
  pinCode?: string | null
  country?: string | null
  state?: string | null
  googleMapLink?: string | null
}

export type UpdateProfileRequest = {
  physicalAddress?: string | null
  pinCode?: string | null
  country?: string | null
  state?: string | null
  googleMapLink?: string | null
}

export const getProfile = async (): Promise<Profile> => {
  const { data } = await api.get<Profile>('/profile')
  return data
}

export const updateProfile = async (payload: UpdateProfileRequest): Promise<Profile> => {
  const { data } = await api.put<Profile>('/profile', payload)
  return data
}
