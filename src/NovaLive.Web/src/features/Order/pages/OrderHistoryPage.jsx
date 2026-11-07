import ShoppingBagOutlinedIcon from '@mui/icons-material/ShoppingBagOutlined'
import AssignmentReturnRoundedIcon from '@mui/icons-material/AssignmentReturnRounded'
import { Alert, Box, Button, CircularProgress, Container, Tab, Tabs, Typography } from '@mui/material'
import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { useInfiniteScroll } from '~/common/hooks/useScroll'
import { getMyReturnsAPI } from '~/common/apis/services/returnService'
import OrderDetailDrawer from '../components/OrderHistoryPage/OrderDetailDrawer'
import OrderHistoryCard, { OrderHistoryCardSkeleton } from '../components/OrderHistoryPage/OrderHistoryCard'
import ReturnHistoryCard from '../components/OrderHistoryPage/ReturnHistoryCard'
import ReturnDetailModal from '../components/OrderHistoryPage/ReturnDetailModal'
import { ORDER_TABS } from '../constants/orderStatus'
import { useInfiniteOrders } from '../hooks/useMyOrders'
import { useOrderMutation } from '../hooks/useOrderMutation'

function EmptyOrders({
  icon: Icon = ShoppingBagOutlinedIcon,
  title = 'Chưa có đơn hàng nào',
  description = 'Hãy mua sắm và đặt hàng để theo dõi đơn ở đây'
}) {
  return (
    <Box sx={{
      display: 'flex', flexDirection: 'column', alignItems: 'center',
      py: 8, gap: 2, color: 'rgba(45,45,45,0.4)'
    }}>
      <Icon sx={{ fontSize: 64, opacity: 0.3 }} />
      <Typography sx={{ fontSize: '0.95rem', fontWeight: 600, color: 'rgba(45,45,45,0.5)' }}>
        {title}
      </Typography>
      <Typography sx={{ fontSize: '0.82rem', color: 'rgba(45,45,45,0.35)', textAlign: 'center', maxWidth: 300 }}>
        {description}
      </Typography>
    </Box>
  )
}

function OrderHistoryPage() {
  const navigate = useNavigate()
  const [searchParams, setSearchParams] = useSearchParams()
  const tabParam = searchParams.get('tab')

  // Find initial tab index based on url query param or default to 0
  const initialTabIdx = useMemo(() => {
    if (!tabParam) return 0
    const idx = ORDER_TABS.findIndex((t) => t.status.toLowerCase() === tabParam.toLowerCase())
    return idx >= 0 ? idx : 0
  }, [tabParam])

  const [tabIdx, setTabIdx] = useState(initialTabIdx)
  const [detailOrder, setDetailOrder] = useState(null)
  const [detailReturn, setDetailReturn] = useState(null)

  const activeTab = ORDER_TABS[tabIdx] || ORDER_TABS[0]
  const status = activeTab.status ?? 'all'

  // Sync tab state when URL search param changes
  useEffect(() => {
    if (tabParam) {
      const idx = ORDER_TABS.findIndex((t) => t.status.toLowerCase() === tabParam.toLowerCase())
      if (idx >= 0 && idx !== tabIdx) {
        setTabIdx(idx)
      }
    }
  }, [tabParam])

  // Normal order queries (only active when not on 'returns' tab)
  const isReturnsTab = status === 'returns'
  const { orders, isLoading, isFetchingNextPage, hasNextPage, fetchNextPage, isError: isOrdersError, refetch: refetchOrders } = useInfiniteOrders(
    isReturnsTab ? 'all' : status,
    !isReturnsTab
  )
  const lastOrderRef = useInfiniteScroll({ isFetchingNextPage, fetchNextPage, hasNextPage })
  const { cancelMutation, invalidateOrders } = useOrderMutation()

  // Returns query (only active when on 'returns' tab)
  const { data: returnsData = [], isLoading: isLoadingReturns, isError: isReturnsError, refetch: refetchReturns } = useQuery({
    queryKey: ['my_returns'],
    queryFn: async () => {
      const res = await getMyReturnsAPI()
      const items = res?.items || (Array.isArray(res) ? res : [])
      return items
    },
    enabled: isReturnsTab,
    staleTime: 30_000
  })

  const handleTabChange = (_, newValue) => {
    setTabIdx(newValue)
    const tab = ORDER_TABS[newValue]
    const currentStatus = tab?.status ?? 'all'
    setSearchParams(currentStatus === 'pending' ? {} : { tab: currentStatus })
  }

  return (
    <Container maxWidth="md" sx={{ pt: { xs: 2, sm: 3 }, pb: 6 }}>
      {/* Header bar */}
      <Box sx={{ mb: 2 }}>
        <Typography variant="h6" sx={{ fontWeight: 800, color: 'primary.contrastText' }}>
          Đơn Hàng Của Tôi
        </Typography>
      </Box>

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
            '& .MuiTab-root': {
              minHeight: 44,
              py: 0,
              px: { xs: 1.5, sm: 2 },
              textTransform: 'uppercase',
              fontSize: '0.8rem',
              fontWeight: 700,
              letterSpacing: '0.02em',
              color: 'rgba(45,45,45,0.6)',
              '&.Mui-selected': {
                color: 'secondary.main',
                fontWeight: 800
              }
            }
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

      {/* Content Section */}
      {isReturnsTab ? (
        /* Returns list */
        <Box>
          {isReturnsError ? (
            <Alert severity="error" action={<Button color="inherit" size="small" onClick={() => refetchReturns()}>Thử lại</Button>} sx={{ mb: 2 }}>
              Không tải được yêu cầu trả hàng. Vui lòng thử lại.
            </Alert>
          ) : isLoadingReturns ? (
            Array.from({ length: 3 }).map((_, i) => <OrderHistoryCardSkeleton key={i} />)
          ) : returnsData.length === 0 ? (
            <Box sx={{
              bgcolor: 'primary.main', borderRadius: 3,
              border: '1px solid', borderColor: 'divider'
            }}>
              <EmptyOrders
                icon={AssignmentReturnRoundedIcon}
                title="Chưa có yêu cầu trả hàng nào"
                description="Các yêu cầu trả hàng và hoàn tiền sau khi gửi sẽ được cập nhật tiến trình tại đây"
              />
            </Box>
          ) : (
            returnsData.map((item) => (
              <ReturnHistoryCard
                key={item.id}
                item={item}
                onViewDetail={setDetailReturn}
              />
            ))
          )}
        </Box>
      ) : (
        /* Standard Orders list */
        <Box>
          {isOrdersError ? (
            <Alert severity="error" action={<Button color="inherit" size="small" onClick={() => refetchOrders()}>Thử lại</Button>} sx={{ mb: 2 }}>
              Không tải được đơn hàng. Vui lòng thử lại.
            </Alert>
          ) : isLoading ? (
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
      )}

      {/* Order detail dialog */}
      <OrderDetailDrawer
        open={!!detailOrder}
        onClose={() => setDetailOrder(null)}
        order={detailOrder}
        onOrderUpdated={invalidateOrders}
      />

      {/* Return detail modal */}
      <ReturnDetailModal
        open={!!detailReturn}
        onClose={() => setDetailReturn(null)}
        returnItem={detailReturn}
      />
    </Container>
  )
}

export default OrderHistoryPage
