import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { toast } from 'react-toastify'
import { checkoutAPI, cancelOrderAPI } from '~/common/apis/services/orderService'

export const useOrderMutation = ({
  onPaymentRequired,
  onOrderSuccess
} = {}) => {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const invalidateOrders = () => {
    queryClient.invalidateQueries({ queryKey: ['orders'] })
    queryClient.invalidateQueries({ queryKey: ['my_orders'] })
    queryClient.invalidateQueries({ queryKey: ['cart'] })
  }

  // Mutation Đặt hàng chính
  const orderMutation = useMutation({
    mutationFn: (payload) => checkoutAPI(payload),
    onSuccess: (data, variables) => {
      invalidateOrders()
      const method = (variables.paymentMethod || '').toLowerCase()

      if (method === 'vietqr' || method === 'momo') {
        // Mở Modal thanh toán QR động kết nối SignalR
        onPaymentRequired?.({
          paymentId: data.paymentId,
          parentOrderId: data.parentOrderId,
          orderCode: data.orderCode,
          amount: data.grandTotal,
          paymentMethod: method,
          expiredAt: data.expiredAt
        })
      } else {
        // COD - Thành công trực tiếp
        toast.success('🎉 Đặt hàng thành công! Cảm ơn bạn đã mua sắm.')
        onOrderSuccess?.(data)
        navigate('/orders')
      }
    },
    onError: (err) => {
      const msg = err?.response?.data?.message || err?.response?.data?.detail || err?.message || 'Đặt hàng thất bại. Vui lòng kiểm tra lại.'
      toast.error(msg)
    }
  })

  // Mutation Hủy đơn hàng
  const cancelMutation = useMutation({
    mutationFn: ({ orderId, reason }) => cancelOrderAPI(orderId, reason),
    onSuccess: () => {
      invalidateOrders()
      toast.success('Đơn hàng đã được hủy thành công!')
    },
    onError: (err) => {
      toast.error(err?.response?.data?.message || 'Hủy đơn hàng thất bại. Vui lòng thử lại.')
    }
  })

  return {
    orderMutation,
    cancelMutation,
    invalidateOrders
  }
}