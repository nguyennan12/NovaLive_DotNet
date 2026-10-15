import { useQuery } from '@tanstack/react-query'
import { getOrderDetailAPI } from '~/common/apis/services/orderService'

export const useOrderDetail = (orderId) => {
  return useQuery({
    queryKey: ['order-detail', orderId],
    queryFn: () => getOrderDetailAPI(orderId),
    enabled: !!orderId,
    staleTime: 30_000
  })
}
