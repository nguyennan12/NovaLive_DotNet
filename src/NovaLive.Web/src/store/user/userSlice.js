import { createSlice, createAsyncThunk } from '@reduxjs/toolkit'
import { toast } from 'react-toastify'

const initialState = { currentUser: null }

export const loginUserAPI = createAsyncThunk(
  'user/loginUserAPI',
  async (data, { rejectWithValue }) => {
    const { loginAPI } = await import('~/common/apis/services/userService')
    try {
      return await loginAPI(data)
    } catch (error) {
      const status = error?.response?.status
      const problem = error?.response?.data
      const message = status === 401
        ? 'Email hoặc số điện thoại hoặc mật khẩu không đúng.'
        : status === 423
          ? problem?.detail || 'Tài khoản đang bị khóa. Vui lòng thử lại sau.'
          : status === 429
            ? problem?.detail || 'Bạn thử đăng nhập quá nhiều lần. Vui lòng thử lại sau.'
            : problem?.detail || problem?.message || (error?.response
                ? 'Đăng nhập thất bại. Vui lòng thử lại.'
                : 'Không thể kết nối máy chủ. Vui lòng kiểm tra mạng và thử lại.')

      return rejectWithValue(message)
    }
  }
)

export const logoutUserAPI = createAsyncThunk(
  'user/logoutUserAPI',
  async (showSuccessMessage = true) => {
    const { logoutAPI } = await import('~/common/apis/services/userService')
    await logoutAPI()
    if (showSuccessMessage) toast.success('Logged out successfully!')
  }
)

export const userSlice = createSlice({
  name: 'user',
  initialState,
  reducers: {
    setCurrentUser: (state, action) => { state.currentUser = action.payload },
    clearCurrentUser: (state) => { state.currentUser = null }
  },
  extraReducers: (builder) => {
    builder.addCase(loginUserAPI.fulfilled, (state, action) => {
      const payload = action.payload?.data || action.payload?.metadata || action.payload || {}
      const user = payload?.user || payload
      const tokens = payload?.tokens || {}
      state.currentUser = {
        ...user,
        _id: user?.id || user?._id,
        id: user?.id || user?._id,
        user_shop: user?.shopId || user?.user_shop,
        user_name: user?.fullName || user?.user_name,
        user_email: user?.email || user?.user_email,
        user_phone: user?.phone || user?.user_phone,
        user_avatar: user?.avatarUrl || user?.user_avatar,
        accessToken: tokens.accessToken || payload?.accessToken || user?.accessToken
      }
    })
    builder.addCase(logoutUserAPI.fulfilled, (state) => {
      state.currentUser = null
    })
  }
})

export const { setCurrentUser, clearCurrentUser } = userSlice.actions
export const selectCurrentUser = (state) => state.user.currentUser
export const UserReducer = userSlice.reducer
