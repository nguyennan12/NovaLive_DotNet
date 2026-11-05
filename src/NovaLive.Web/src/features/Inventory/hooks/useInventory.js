import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'react-toastify'
import { LIMIT } from '~/common/utils/constant'
import {
  addInventoryAPI,
  getChartArtInventoryAPI,
  getHistoryInventoryAPI,
  getReservedStocAPI,
  getSellerInventoryAPI
} from '~/common/apis/services/inventoryService'

export const useInventoryStock = (params) => {
  const { data, isLoading, isPlaceholderData } = useQuery({
    queryKey: ['skus', params],
    queryFn: async () => {
      const query = {
        ...(params.keyword ? { keyword: params.keyword } : {}),
        ...(params.stock === 'low' ? { lowStock: true } : {})
      }
      let result
      let rawItems
      if (params.stock === 'in' || params.stock === 'out') {
        const firstPage = await getSellerInventoryAPI({ ...query, page: 1, size: 100 })
        const laterPages = firstPage.totalPages > 1
          ? await Promise.all(Array.from({ length: firstPage.totalPages - 1 }, (_, index) =>
            getSellerInventoryAPI({ ...query, page: index + 2, size: 100 })
          ))
          : []
        rawItems = [...firstPage.items, ...laterPages.flatMap(page => page.items)]
        result = { ...firstPage, items: rawItems }
      } else {
        result = await getSellerInventoryAPI({ ...query, page: params.page, size: params.limit })
        rawItems = result.items
      }
      let items = rawItems.map(item => ({
        ...item,
        _id: item.skuId,
        id: item.skuId,
        sku_spuId: item.skuId,
        spu_name: item.spuName,
        sku_id: item.skuCode,
        sku_name: item.skuCode,
        sku_stock: item.availableQty ?? (Number(item.qtyOnHand || 0) - Number(item.reservedQty || 0)),
        total_reserved: item.reservedQty,
        product: { spu_name: item.spuName },
        sku: { sku_name: item.skuCode }
      }))

      if (params.stock === 'in') items = items.filter(item => item.sku_stock > 0)
      if (params.stock === 'out') items = items.filter(item => item.sku_stock <= 0)

      if (params.stock === 'in' || params.stock === 'out') {
        const totalCount = items.length
        const size = params.limit
        const start = (params.page - 1) * size
        return { ...result, items: items.slice(start, start + size), totalCount, totalPages: Math.max(1, Math.ceil(totalCount / size)) }
      }

      return { ...result, items }
    },
    placeholderData: (previousData) => previousData,
    staleTime: 30 * 1000
  })

  return {
    skus: data?.items || [],
    totalPages: data?.totalPages || 1,
    totalCount: data?.totalCount || 0,
    isLoading,
    isPlaceholderData
  }
}

export const useInventoryAdjustmentOptions = () => {
  const { data: skus = [], isLoading } = useQuery({
    queryKey: ['inventory_adjustment_options'],
    queryFn: async () => {
      const firstPage = await getSellerInventoryAPI({ page: 1, size: 100 })
      const remainingPages = firstPage.totalPages > 1
        ? await Promise.all(Array.from({ length: firstPage.totalPages - 1 }, (_, index) =>
          getSellerInventoryAPI({ page: index + 2, size: 100 })
        ))
        : []
      return [...firstPage.items, ...remainingPages.flatMap(page => page.items)].map(item => ({
        ...item,
        _id: item.skuId,
        id: item.skuId,
        sku_spuId: item.skuId,
        spu_name: item.spuName,
        sku_id: item.skuCode,
        sku_name: item.skuCode,
        sku_stock: item.availableQty,
        total_reserved: item.reservedQty,
        product: { spu_name: item.spuName },
        sku: { sku_name: item.skuCode }
      }))
    },
    staleTime: 30 * 1000
  })

  return { skus, isLoading }
}

export const useStockHistory = (params) => {
  const queryString = typeof params === 'string' ? params : new URLSearchParams({
    page: String(params.page ?? 1),
    size: String(params.limit ?? LIMIT.INVENTORY),
    ...(params.type && params.type !== 'all' ? { changeType: params.type } : {})
  }).toString()
  const { data, isLoading } = useQuery({
    queryKey: ['history_inventory', queryString],
    queryFn: () => getHistoryInventoryAPI(queryString),
    staleTime: 20 * 1000
  })

  return {
    histories: data?.items || [],
    totalPages: data?.totalPages || 1,
    totalItems: data?.totalItems || 0,
    isLoading
  }
}

export const useReservedStock = () => {
  const { data: reserveds = [], isLoading } = useQuery({
    queryKey: ['reserved_stock'],
    queryFn: getReservedStocAPI,
    staleTime: 30 * 1000
  })

  return { reserveds, isLoading }
}

export const useStockOverviewChart = (period = 'today') => {
  const { data = [], isLoading } = useQuery({
    queryKey: ['chart_inventory', period],
    queryFn: () => getChartArtInventoryAPI(period),
    staleTime: 30 * 1000
  })

  return { data, isLoading }
}

export const useStockAdjustment = () => {
  const queryClient = useQueryClient()

  const mutation = useMutation({
    mutationFn: addInventoryAPI,
    onSuccess: () => {
      toast.success('Điều chỉnh tồn kho thành công!')
      queryClient.invalidateQueries({ queryKey: ['skus'] })
      queryClient.invalidateQueries({ queryKey: ['inventory_adjustment_options'] })
      queryClient.invalidateQueries({ queryKey: ['chart_inventory'] })
      queryClient.invalidateQueries({ queryKey: ['history_inventory'] })
      queryClient.invalidateQueries({ queryKey: ['reserved_stock'] })
    },
    onError: (error) => {
      const message = error.response?.data?.detail || error.response?.data?.message || error.message || 'Lỗi khi cập nhật kho'
      toast.error(message)
    }
  })

  return {
    adjustStock: mutation.mutateAsync,
    isSubmitting: mutation.isPending
  }
}
