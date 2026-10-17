import { createSlice, createAsyncThunk } from '@reduxjs/toolkit'
import { toast } from 'react-toastify'

const initialState = { currentUser: null }

export const loginUserAPI = createAsyncThunk(
  'user/loginUserAPI',
  async (data) => {
    const { loginAPI } = await import('~/common/apis/services/userService')
    const response = await loginAPI(data)
    return response
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