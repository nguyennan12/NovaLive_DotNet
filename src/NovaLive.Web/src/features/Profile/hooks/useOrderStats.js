import { useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useSelector } from 'react-redux'
import { getMyOrdersAPI } from '~/common/apis/services/orderService'
import { ORDER_STATUS } from '~/features/Order/constants/orderStatus'
import { selectCurrentUser } from '~/store/user/userSlice'

const PAID_STATUSES = new Set([ORDER_STATUS.CONFIRMED, ORDER_STATUS.SHIPPED, ORDER_STATUS.DELIVERED])

export const useOrderStats = () => {
  const currentUser = useSelector(selectCurrentUser)

  const { data: orders = [], isLoading } = useQuery({
    queryKey: ['order_stats_summary'],
    queryFn: () => getMyOrdersAPI({ limit: 500 }),
    staleTime: 2 * 60 * 1000,
    enabled: !!currentUser,
    retry: false
  })

  const stats = useMemo(() => ({
    totalOrders: orders.length,
    totalSpent: orders
      .filter(o => PAID_STATUSES.has(o.order_status))
      .reduce((sum, o) => sum + (o.order_total || 0), 0),
    pendingOrders: orders.filter(o => o.order_status === ORDER_STATUS.PENDING).length
  }), [orders])

  return { ...stats, isLoading }
}
