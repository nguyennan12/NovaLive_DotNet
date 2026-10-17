import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'react-toastify'
import {
  addInventoryAPI,
  getChartArtInventoryAPI,
  getHistoryInventoryAPI,
  getReservedStocAPI
} from '~/common/apis/services/inventoryService'
import { getAllProductWithStockAPI } from '~/common/apis/services/productService'

export const useInventoryStock = (params) => {
  const queryString = typeof params === 'string' ? params : new URLSearchParams(params).toString()
  const { data, isLoading, isPlaceholderData } = useQuery({
    queryKey: ['skus', queryString],
    queryFn: () => getAllProductWithStockAPI(queryString),
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

export const useStockHistory = (params) => {
  const queryString = typeof params === 'string' ? params : new URLSearchParams(params).toString()
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
