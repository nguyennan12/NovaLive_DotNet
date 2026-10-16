import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'react-toastify'
import {
  addProductToLiveSessionAPI,
  getLiveSessionByIdAPI,
  removeProductFromLiveSessionAPI
} from '~/common/apis/services/liveService'

const QUERY_KEY = (liveId) => ['live-session-products', liveId]

export const useLiveProducts = (liveId) => {
  const queryClient = useQueryClient()
  const qKey = QUERY_KEY(liveId)

  // Get live từ server
  const { data: sessionData, isLoading: isLoadingProducts } = useQuery({
    queryKey: qKey,
    queryFn: () => getLiveSessionByIdAPI(liveId),
    enabled: !!liveId,
    staleTime: 0
  })

  const liveProducts = sessionData?.live_products ?? []

  // Thêm sản phẩm
  const { mutate: addProduct, isPending: isAdding } = useMutation({
    mutationFn: (products) => addProductToLiveSessionAPI(liveId, products),
    onSuccess: (data) => {
      queryClient.setQueryData(qKey, data)
      toast.success('Đã thêm sản phẩm vào phiên live')
    },
    onError: () => {
      toast.error('Không thể thêm sản phẩm')
    }
  })

  // Xoá sản phẩm
  const { mutate: removeProduct, isPending: isRemoving } = useMutation({
    mutationFn: (productId) => removeProductFromLiveSessionAPI(liveId, productId),
    onMutate: async (productId) => {
      await queryClient.cancelQueries({ queryKey: qKey })
      const snapshot = queryClient.getQueryData(qKey)
      queryClient.setQueryData(qKey, (old) => ({
        ...old,
        live_products: (old?.live_products ?? []).filter(
          (p) => p.productId?.toString() !== productId?.toString()
        )
      }))
      return { snapshot }
    },
    onSuccess: (data) => {
      queryClient.setQueryData(qKey, data)
      toast.success('Đã xoá sản phẩm khỏi phiên live')
    },
    onError: (_err, _productId, context) => {
      if (context?.snapshot) queryClient.setQueryData(qKey, context.snapshot)
      toast.error('Không thể xoá sản phẩm')
    }
  })

  return { liveProducts, addProduct, removeProduct, isAdding, isRemoving, isLoadingProducts }
}
