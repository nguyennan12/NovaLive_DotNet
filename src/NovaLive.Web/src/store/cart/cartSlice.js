import { createSlice } from '@reduxjs/toolkit'

// Cart slice chỉ lưu UI state — server là source of truth cho data giỏ hàng
const initialState = {
  selectedIds: [], // cartItemId[] đang được chọn để thanh toán
  guestItems: []
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
    },
    addGuestItem: (state, action) => {
      const { skuId, quantity = 1, livestreamSessionId = null, guestProduct = {} } = action.payload
      const cartItemId = `guest-${skuId}`
      const existing = state.guestItems.find(item => item.cartItemId === cartItemId)
      if (existing) {
        existing.quantity += quantity
        if (livestreamSessionId) existing.livestreamSessionId = livestreamSessionId
        return
      }
      state.guestItems.push({
        ...guestProduct,
        cartItemId,
        id: cartItemId,
        _id: cartItemId,
        skuId,
        quantity,
        livestreamSessionId,
        price: guestProduct.price ?? guestProduct.livePrice ?? 0,
        name: guestProduct.name ?? guestProduct.spuName ?? 'Sản phẩm',
        image: guestProduct.image ?? guestProduct.thumbnailUrl ?? '',
        shopId: guestProduct.shopId ?? `live-${livestreamSessionId || 'guest'}`,
        shopName: guestProduct.shopName ?? 'Sản phẩm livestream'
      })
    },
    updateGuestQuantity: (state, action) => {
      const item = state.guestItems.find(entry => entry.cartItemId === String(action.payload.cartItemId))
      if (item && action.payload.quantity > 0) item.quantity = action.payload.quantity
    },
    removeGuestItems: (state, action) => {
      const ids = action.payload.map(String)
      state.guestItems = state.guestItems.filter(item => !ids.includes(String(item.cartItemId)))
      state.selectedIds = state.selectedIds.filter(id => !ids.includes(String(id)))
    },
    clearGuestCart: (state) => {
      const guestIds = new Set(state.guestItems.map(item => String(item.cartItemId)))
      state.guestItems = []
      state.selectedIds = state.selectedIds.filter(id => !guestIds.has(String(id)))
    }
  }
})

export const {
  toggleSelect, setSelectedIds, deselectAll,
  addGuestItem, updateGuestQuantity, removeGuestItems, clearGuestCart
} = cartSlice.actions

export const selectSelectedIds = (state) => state.cart.selectedIds
export const selectGuestItems = (state) => state.cart.guestItems ?? []

export const CartReducer = cartSlice.reducer
