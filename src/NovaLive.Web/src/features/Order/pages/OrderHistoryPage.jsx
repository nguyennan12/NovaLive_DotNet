import ShoppingBagOutlinedIcon from '@mui/icons-material/ShoppingBagOutlined'
import { Box, CircularProgress, Container, Tab, Tabs, Typography } from '@mui/material'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useInfiniteScroll } from '~/common/hooks/useScroll'
import OrderDetailDrawer from '../components/OrderHistoryPage/OrderDetailDrawer'
import OrderHistoryCard, { OrderHistoryCardSkeleton } from '../components/OrderHistoryPage/OrderHistoryCard'
import { ORDER_TABS } from '../constants/orderStatus'
import { useInfiniteOrders } from '../hooks/useMyOrders'
import { useOrderMutation } from '../hooks/useOrderMutation'


function EmptyOrders() {
  return (
    <Box sx={{
      display: 'flex', flexDirection: 'column', alignItems: 'center',
      py: 8, gap: 2, color: 'rgba(45,45,45,0.4)'
    }}>
      <ShoppingBagOutlinedIcon sx={{ fontSize: 64, opacity: 0.3 }} />
      <Typography sx={{ fontSize: '0.95rem', fontWeight: 600, color: 'rgba(45,45,45,0.5)' }}>
        Chưa có đơn hàng nào
      </Typography>
      <Typography sx={{ fontSize: '0.82rem', color: 'rgba(45,45,45,0.35)', textAlign: 'center', maxWidth: 260 }}>
        Hãy mua sắm và đặt hàng để theo dõi đơn ở đây
      </Typography>
    </Box>
  )
}

function OrderHistoryPage() {
  const navigate = useNavigate()
  const [tabIdx, setTabIdx] = useState(0)
  const [detailOrder, setDetailOrder] = useState(null)
  const [status, setStatus] = useState('pending')

  const { orders, isLoading, isFetchingNextPage, hasNextPage, fetchNextPage } = useInfiniteOrders(status)
  const lastOrderRef = useInfiniteScroll({ isFetchingNextPage, fetchNextPage, hasNextPage })
  const { cancelMutation, retryVNPayMutation, invalidateOrders } = useOrderMutation()

  const handleTabChange = (_, newValue) => {
    setTabIdx(newValue)
    const tab = ORDER_TABS[newValue]
    const currentStatus = tab.status ?? 'all'
    setStatus(currentStatus)
  }

  return (
    <Container maxWidth="md" sx={{ pt: { xs: 2, sm: 3 }, pb: 6 }}>

      {/* Tabs */}
      <Box sx={{
        bgcolor: 'primary.main',
        borderRadius: 3,
        border: '1px solid', borderColor: 'divider',
        mb: 2, overflow: 'hidden'
      }}>
        <Tabs
          value={tabIdx}
          onChange={handleTabChange}
          variant="scrollable"
          scrollButtons="auto"
          allowScrollButtonsMobile
          sx={{
            minHeight: 44,
            '& .MuiTabs-scrollButtons': { width: 28 },
            '& .MuiTab-root': { minHeight: 44, py: 0 }
          }}
        >
          {ORDER_TABS.map((tab) => (
            <Tab
              key={tab.label}
              label={
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
                  <Typography sx={{ fontSize: '0.8rem', fontWeight: 'inherit' }}>
                    {tab.label}
                  </Typography>

                </Box>
              }
            />
          ))}
        </Tabs>
      </Box>

      {/* Order list */}
      <Box>
        {isLoading ? (
          Array.from({ length: 3 }).map((_, i) => <OrderHistoryCardSkeleton key={i} />)
        ) : orders.length === 0 ? (
          <Box sx={{
            bgcolor: 'primary.main', borderRadius: 3,
            border: '1px solid', borderColor: 'divider'
          }}>
            <EmptyOrders onShop={() => navigate('/')} />
          </Box>
        ) : (
          orders.map((order, idx) => {
            const isLast = idx === orders.length - 1
            return (
              <OrderHistoryCard
                key={order._id}
                order={order}
                onViewDetail={setDetailOrder}
                cancelMutation={cancelMutation}
                retryVNPayMutation={retryVNPayMutation}
                onOrderUpdated={invalidateOrders}
                ref={isLast ? lastOrderRef : null}
              />
            )
          })
        )}
        {isFetchingNextPage && (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
            <CircularProgress size={30} />
          </Box>
        )}
      </Box>

      {/* Order detail dialog */}
      <OrderDetailDrawer
        open={!!detailOrder}
        onClose={() => setDetailOrder(null)}
        order={detailOrder}
        onOrderUpdated={invalidateOrders}
      />
    </Container>
  )
}

export default OrderHistoryPage
