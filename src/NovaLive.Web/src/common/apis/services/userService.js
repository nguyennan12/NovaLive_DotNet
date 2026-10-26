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
    email: data.email.trim(),
    phone: data.phone.trim(),
    password: data.password,
    fullName: data.fullName.trim()
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

const normalizeUserProfile = (user) => user && ({
  ...user,
  id: user.id || user.userId || user._id,
  _id: user.id || user.userId || user._id,
  user_name: user.fullName || user.user_name || '',
  user_email: user.email || user.user_email || '',
  user_phone: user.phone || user.user_phone || '',
  user_gender: user.gender || user.user_gender || '',
  user_birthday: user.birthday || user.user_birthday || '',
  user_avatar: user.avatarUrl || user.user_avatar || ''
})

export const getMyProfileAPI = async () => {
  return normalizeUserProfile(await authorizedAxiosInstance.get('users/me'))
}

export const updateMyProfileAPI = async (data) => {
  const request = {
    fullName: data.fullName || data.user_name,
    phone: data.phone ?? data.user_phone ?? null,
    birthday: data.birthday ?? data.user_birthday ?? null,
    gender: data.gender ?? data.user_gender ?? null,
    avatarUrl: data.avatarUrl ?? data.user_avatar ?? null
  }
  return normalizeUserProfile(await authorizedAxiosInstance.put('users/me', request))
}

// ─── Backward compatibility aliases ──────────────────────────────────────────
export const verifyUserAPI = verifyOtpAPI
export const resendMailAPI = resendOtpAPI
export const logoutAllDevicesAPI = logoutAPI
