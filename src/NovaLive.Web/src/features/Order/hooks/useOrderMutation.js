import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { toast } from 'react-toastify'
import { OrderByUserAPI, cancelOrderAPI, confirmCodOTPAPI, createVNPayUrlAPI, resendCodOTPAPI } from '~/common/apis/services/orderService'

export const useOrderMutation = ({ setCreatedOrder, setShowCodOtp, onSuccessConfirm } = {}) => {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const invalidateOrders = () => queryClient.invalidateQueries({ queryKey: ['my_orders'] })

  //  Mutation Đặt hàng chính
  const orderMutation = useMutation({
    mutationFn: OrderByUserAPI,
    onSuccess: async (data, variables) => {
      if (variables.userPayment === 'vnpay') {
        try {
          const result = await createVNPayUrlAPI({
            orderId: data.order_trackingNumber,
            amount: data.order_checkout?.finalCheckout
          })
          window.location.href = result.paymentUrl
        } catch {
          toast.error('Không thể tạo link thanh toán VNPay. Vui lòng liên hệ hỗ trợ.')
          navigate('/')
        }
      } else if (variables.userPayment === 'cod') {
        setCreatedOrder(data)
        setShowCodOtp(true)
      } else {
        toast.success('Đặt hàng thành công!')
        navigate('/')
      }
    },
    onError: (err) => {
      toast.error(err?.response?.data?.message || 'Đặt hàng thất bại. Vui lòng thử lại.')
    }
  })

  //  Mutation Xác nhận OTP
  const confirmMutation = useMutation({
    mutationFn: confirmCodOTPAPI,
    onSuccess: () => {
      toast.success('Xác nhận thành công! Đơn hàng đang được xử lý.')
      onSuccessConfirm?.()
    },
    onError: (err) => {
      toast.error(err?.response?.data?.message || 'Mã OTP không đúng. Vui lòng thử lại.')
    }
  })

  // Mutation Gửi lại OTP
  const resendMutation = useMutation({
    mutationFn: resendCodOTPAPI,
    onSuccess: () => {
      toast.success('Đã gửi lại mã OTP tới email của bạn!')
    },
    onError: () => {
      toast.error('Gửi lại mã thất bại. Vui lòng thử lại.')
    }
  })

  //Mutation thanh toán lại vnpay
  const retryVNPayMutation = useMutation({
    mutationFn: ({ trackingNumber, amount }) =>
      createVNPayUrlAPI({ orderId: trackingNumber, amount }),
    onSuccess: (data) => {
      if (data?.paymentUrl) window.location.href = data.paymentUrl
    },
    onError: (err) => {
      toast.error(err?.response?.data?.message || 'Không thể tạo link thanh toán. Vui lòng thử lại.')
    }
  })

  //Mutation cancelOrder
  const cancelMutation = useMutation({
    mutationFn: cancelOrderAPI,
    onSuccess: () => {
      invalidateOrders()
      toast.success('Đơn hàng đã được hủy thành công!')
    },
    onError: (err) => {
      toast.error(err?.response?.data?.message || 'Hủy đơn hàng thất bại. Vui lòng thử lại.')
    }
  })

  return { orderMutation, confirmMutation, resendMutation, cancelMutation, retryVNPayMutation, invalidateOrders }
}