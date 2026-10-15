import { createSlice } from '@reduxjs/toolkit'

const initialState = {
  appliedshopDiscounts: {},
  appliedProductVoucher: null,
  appliedFreeshipVoucher: null
}

export const discountSlice = createSlice({
  name: 'discount',
  initialState,
  reducers: {
    setShopDiscount: (state, action) => {
      const { shopId, discount } = action.payload
      if (!state.appliedshopDiscounts) state.appliedshopDiscounts = {}
      state.appliedshopDiscounts[String(shopId)] = discount
    },
    clearShopDiscount: (state, action) => {
      if (!state.appliedshopDiscounts) return
      delete state.appliedshopDiscounts[String(action.payload)]
    },
    setProductVoucher: (state, action) => {
      state.appliedProductVoucher = action.payload
    },
    clearProductVoucher: (state) => {
      state.appliedProductVoucher = null
    },
    setFreeshipVoucher: (state, action) => {
      state.appliedFreeshipVoucher = action.payload
    },
    clearFreeshipVoucher: (state) => {
      state.appliedFreeshipVoucher = null
    }
  }
})

export const {
  setShopDiscount, clearShopDiscount,
  setProductVoucher, clearProductVoucher,
  setFreeshipVoucher, clearFreeshipVoucher
} = discountSlice.actions

export const selectAppliedShopDiscounts = (state) => state.discount.appliedshopDiscounts
export const selectAppliedProductVoucher = (state) => state.discount.appliedProductVoucher
export const selectAppliedFreeshipVoucher = (state) => state.discount.appliedFreeshipVoucher

export const DiscountReducer = discountSlice.reducer
