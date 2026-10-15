import { useMemo } from 'react'
import { useSelector } from 'react-redux'
import { selectSelectedIds } from '~/store/cart/cartSlice'

export const useCartCalc = (shopGroups) => {
  const selectedIds = useSelector(selectSelectedIds)

  const allItems = useMemo(
    () => shopGroups?.flatMap(g =>
      g.items.map(item => ({ ...item, shopId: g.shopId, shopName: g.shopName }))
    ),
    [shopGroups]
  )

  const selectedItems = useMemo(
    () => allItems?.filter(i => selectedIds.includes(String(i.skuId))),
    [allItems, selectedIds]
  )

  const isAllSelected =
    allItems?.length > 0 && allItems.every(i => selectedIds.includes(String(i.skuId)))

  const subtotal = useMemo(
    () => selectedItems?.reduce((s, i) => s + (i.isFlashSale ? i.flashSalePrice : i.price) * i.quantity, 0),
    [selectedItems]
  )


  return {
    selectedIds,
    allItems,
    selectedItems,
    subtotal,
    isAllSelected
  }
}
