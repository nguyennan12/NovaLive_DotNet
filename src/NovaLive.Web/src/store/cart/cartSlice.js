import { createSlice } from '@reduxjs/toolkit'

// Cart slice chỉ lưu UI state — server là source of truth cho data giỏ hàng
const initialState = {
  selectedIds: [] // skuId[] đang được chọn để thanh toán
}

export const cartSlice = createSlice({
  name: 'cart',
  initialState,
  reducers: {
    toggleSelect: (state, action) => {
      const id = String(action.payload)
      if (state.selectedIds.includes(id)) {
        state.selectedIds = state.selectedIds.filter(i => i !== id)
      } else {
        state.selectedIds.push(id)
      }
    },
    setSelectedIds: (state, action) => {
      state.selectedIds = action.payload.map(String)
    },
    deselectAll: (state) => {
      state.selectedIds = []
    }
  }
})

export const { toggleSelect, setSelectedIds, deselectAll } = cartSlice.actions

export const selectSelectedIds = (state) => state.cart.selectedIds

export const CartReducer = cartSlice.reducer
