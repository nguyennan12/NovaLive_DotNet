import authorizedAxiosInstance from '../custom/authorizeAxios'
import { toast } from 'react-toastify'

// ─── Auth ────────────────────────────────────────────────────────────────────

export const loginAPI = async (data) => {
  const payload = {
    emailOrPhone: data.emailOrPhone || data.email,
    password: data.password
  }
  return await authorizedAxiosInstance.post('auth/login', payload)
}

export const registerUserAPI = async (data) => {
  const payload = {
    email: data.email,
    phone: data.phone || '',
    password: data.password,
    fullName: data.fullName || data.name || data.email.split('@')[0]
  }
  const response = await authorizedAxiosInstance.post('auth/register', payload)
  toast.success('Đăng ký tài khoản thành công! Vui lòng kiểm tra mã OTP để kích hoạt.', { theme: 'colored' })
  return response
}

export const verifyOtpAPI = async (data) => {
  const payload = {
    userId: data.userId,
    otpCode: data.otpCode || data.otpToken || data.otp
  }
  const response = await authorizedAxiosInstance.post('auth/verify-otp', payload)
  toast.success('Xác thực tài khoản thành công! Bạn có thể đăng nhập ngay.', { theme: 'colored' })
  return response
}

export const resendOtpAPI = async (data) => {
  const payload = {
    emailOrPhone: typeof data === 'string' ? data : (data.emailOrPhone || data.email)
  }
  const response = await authorizedAxiosInstance.post('auth/resend-otp', payload)
  toast.success('Mã OTP mới đã được gửi!', { theme: 'colored' })
  return response
}

export const refreshTokenAPI = async (refreshToken) => {
  return await authorizedAxiosInstance.post('auth/refresh', refreshToken ? { refreshToken } : {})
}

export const logoutAPI = async () => {
  return await authorizedAxiosInstance.post('auth/logout')
}

export const changePasswordAPI = async (data) => {
  const payload = {
    oldPassword: data.oldPassword || data.currentPassword,
    newPassword: data.newPassword
  }
  return await authorizedAxiosInstance.post('auth/change-password', payload)
}

export const forgotPasswordAPI = async (data) => {
  const payload = {
    email: typeof data === 'string' ? data : (data.email || data.emailOrPhone)
  }
  return await authorizedAxiosInstance.post('auth/forgot-password', payload)
}

export const resetPasswordAPI = async (data) => {
  const payload = {
    email: data.email,
    otpCode: data.otpCode || data.otp,
    newPassword: data.newPassword
  }
  return await authorizedAxiosInstance.post('auth/reset-password', payload)
}

export const getMyProfileAPI = async () => {
  return await authorizedAxiosInstance.get('auth/me')
}

export const updateMyProfileAPI = async (data) => {
  return await authorizedAxiosInstance.put('users/me', data)
}

// ─── Backward compatibility aliases ──────────────────────────────────────────
export const verifyUserAPI = verifyOtpAPI
export const resendMailAPI = resendOtpAPI
export const logoutAllDevicesAPI = logoutAPI
