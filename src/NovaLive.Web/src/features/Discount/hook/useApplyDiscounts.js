import { useDispatch, useSelector } from 'react-redux'
import {
  clearFreeshipVoucher, clearProductVoucher, clearShopDiscount,
  selectAppliedFreeshipVoucher,
  selectAppliedProductVoucher,
  selectAppliedShopDiscounts,
  setFreeshipVoucher, setProductVoucher, setShopDiscount
} from '~/store/discount/discountSlice'

export const useApplyDiscounts = () => {
  const dispatch = useDispatch()
  const appliedshopDiscounts = useSelector(selectAppliedShopDiscounts)
  const appliedProductVoucher = useSelector(selectAppliedProductVoucher)
  const appliedFreeshipVoucher = useSelector(selectAppliedFreeshipVoucher)

  return {
    //Set and Clear Discount
    setShopDiscount: (shopId, discount) => dispatch(setShopDiscount({ shopId, discount })),
    clearShopDiscount: (shopId) => dispatch(clearShopDiscount(shopId)),
    setProductVoucher: (v) => dispatch(setProductVoucher(v)),
    clearProductVoucher: () => dispatch(clearProductVoucher()),
    setFreeshipVoucher: (v) => dispatch(setFreeshipVoucher(v)),
    clearFreeshipVoucher: () => dispatch(clearFreeshipVoucher()),
    //discount applied
    appliedFreeshipVoucher,
    appliedProductVoucher,
    appliedshopDiscounts
  }
}