import { createContext, useContext, useEffect, useState, useMemo } from 'react'
import { useSelector } from 'react-redux'
import { selectCurrentUser } from '~/store/user/userSlice'
import { useOrderHub } from '../hooks/useSignalR'
import { toast } from 'react-toastify'
import { useLocation } from 'react-router-dom'

const SignalRContext = createContext(null)

export const SignalRProvider = ({ children, excludedOrderPaths = [] }) => {
  const { pathname } = useLocation()
  const orderHubExcluded = excludedOrderPaths.some(path => pathname === path || pathname.startsWith(`${path}/`))
  const currentUser = useSelector(selectCurrentUser)
  const userId = currentUser?._id || currentUser?.id
  const shopId = currentUser?.shopId || currentUser?.ShopId

  const [lastOrderEvent, setLastOrderEvent] = useState(null)

  const handleOrderPlaced = (order) => {
    setLastOrderEvent({ type: 'OrderPlaced', order, timestamp: new Date() })
    toast.info(`🔔 Bạn có đơn hàng mới #${order?.orderCode || order?.id || ''}!`, {
      position: 'top-right',
      autoClose: 5000
    })
  }

  const handleOrderStatusChanged = (payload) => {
    setLastOrderEvent({ type: 'OrderStatusChanged', payload, timestamp: new Date() })
    toast.info(`📦 Đơn hàng #${payload?.orderCode || ''} đã chuyển sang trạng thái: ${payload?.status || ''}`, {
      position: 'top-right'
    })
  }

  useOrderHub({
    shopId,
    customerId: userId,
    onOrderPlaced: handleOrderPlaced,
    onOrderStatusChanged: handleOrderStatusChanged,
    enabled: !orderHubExcluded && !!(userId || shopId)
  })

  const contextValue = useMemo(() => ({
    lastOrderEvent
  }), [lastOrderEvent])

  return (
    <SignalRContext.Provider value={contextValue}>
      {children}
    </SignalRContext.Provider>
  )
}

export const useSignalRContext = () => {
  const context = useContext(SignalRContext)
  return context || {}
}
