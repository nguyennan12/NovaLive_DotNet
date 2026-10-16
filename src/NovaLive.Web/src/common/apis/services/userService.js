import authorizedAxiosInstance from '../custom/authorizeAxios'
import { toast } from 'react-toastify'

// ─── Auth ────────────────────────────────────────────────────────────────────

export const loginAPI = async (data) => {
  const payload = {
    emailOrPhone: data.emailOrPhone || data.email,
    password: data.password
  }
  const response = await authorizedAxiosInstance.post('auth/login', payload)
  return response
}

export const logoutAPI = async () => {
  const response = await authorizedAxiosInstance.post('auth/logout')
  return response
}

export const refreshTokenAPI = async () => {
  const response = await authorizedAxiosInstance.post('auth/refresh')
  return response
}

export const registerUserAPI = async (data) => {
  const payload = {
    email: data.email,
    phone: data.phone,
    fullName: data.fullName,
    password: data.password
  }
  const response = await authorizedAxiosInstance.post('auth/register', payload)
  toast.success('Account created successfully! Please check and verify your OTP', { theme: 'colored' })
  return response
}

export const verifyUserAPI = async (data) => {
  const payload = {
    userId: data.userId,
    otpCode: String(data.otpCode || data.otpToken || '')
  }
  const response = await authorizedAxiosInstance.post('auth/verify-otp', payload)
  toast.success('Account verified successfully! Now you can login to enjoy our services!', { theme: 'colored' })
  return response
}

export const resendMailAPI = async (data) => {
  const payload = {
    emailOrPhone: data.emailOrPhone || data.email
  }
  const response = await authorizedAxiosInstance.post('auth/resend-otp', payload)
  return response
}

export const changePasswordAPI = async (data) => {
  const payload = {
    oldPassword: data.oldPassword || data.password,
    newPassword: data.newPassword
  }
  const response = await authorizedAxiosInstance.post('auth/change-password', payload)
  return response.metadata ?? response.data ?? response
}


export const getMyProfileAPI = async () => {
  const response = await authorizedAxiosInstance.get('users/me')
  return response.metadata ?? response.data ?? response
}

export const logoutAllDevicesAPI = async () => {
  const response = await authorizedAxiosInstance.post('auth/logout')
  return response.metadata ?? response.data ?? response
}

export const updateMyProfileAPI = async (data) => {
  const response = await authorizedAxiosInstance.put('users/me', data)
  return response.metadata ?? response.data ?? response
}

