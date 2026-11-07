import ShoppingBagRoundedIcon from '@mui/icons-material/ShoppingBagRounded'
import { Box, IconButton } from '@mui/material'
import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { getLiveSessionByIdAPI } from '~/common/apis/services/liveService'
import { useLiveSocket } from '../../hooks/useLiveSocket'
import LiveComments from './LiveComments'
import LiveHeaderInfo from './LiveHeaderInfo'
import LiveProductListSheet from './LiveProductListSheet'

const LiveOverlay = ({ live }) => {
  const [listOpen, setListOpen] = useState(false)
  const liveId = live?.id || live?._id
  const { data: detail } = useQuery({
    queryKey: ['live-session-products', liveId],
    queryFn: () => getLiveSessionByIdAPI(liveId),
    enabled: !!liveId,
    staleTime: 15 * 1000
  })
  const initialProducts = detail?.live_products ?? live?.live_products ?? []

  const { viewers, likes, comments, products, sendLike, sendComment, commentError } = useLiveSocket({
    liveCode: liveId,
    liveProducts: initialProducts,
    initialViewers: live?.live_metrics?.viewer_count ?? 0,
    initialLikes: live?.live_metrics?.total_likes ?? 0
  })

  const liveProducts = products
  return (
    <>
      <Box sx={{
        position: 'absolute', inset: 0, zIndex: 10,
        pointerEvents: 'none', borderRadius: 'inherit'
      }}>
        {/* Top header */}
        <LiveHeaderInfo live={live} viewers={viewers} />

        {/* Products FAB*/}
        {liveProducts.length > 0 && (
          <IconButton
            onClick={() => setListOpen(true)}
            sx={{
              position: 'absolute', top: 72, right: 10,
              width: 40, height: 40,
              bgcolor: 'rgba(255,255,255,0.18)',
              backdropFilter: 'blur(8px)',
              WebkitBackdropFilter: 'blur(8px)',
              border: '1px solid rgba(255,255,255,0.2)',
              pointerEvents: 'auto',
              '&:hover': { bgcolor: 'rgba(74,127,255,0.4)' }
            }}
          >
            <ShoppingBagRoundedIcon sx={{ color: 'secondary.main', fontSize: 20 }} />
          </IconButton>
        )}

        {/* Bottom: comments + like/share */}
        <LiveComments
          comments={comments}
          onSend={sendComment}
          commentError={commentError}
          likes={likes}
          onLike={sendLike}
        />
      </Box>

      {/* Product list bottom sheet — outside overlay (not affected by pointer-events: none) */}
      <LiveProductListSheet
        liveId={liveId}
        open={listOpen}
        onClose={() => setListOpen(false)}
        products={liveProducts}
      />
    </>
  )
}

export default LiveOverlay
