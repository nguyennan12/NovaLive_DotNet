import ShoppingBagRoundedIcon from '@mui/icons-material/ShoppingBagRounded'
import { Box, IconButton } from '@mui/material'
import { useState } from 'react'
import { useSelector } from 'react-redux'
import { selectCurrentUser } from '~/store/user/userSlice'
import { useLiveSocket } from '../../hooks/useLiveSocket'
import LiveComments from './LiveComments'
import LiveHeaderInfo from './LiveHeaderInfo'
import LiveProductListSheet from './LiveProductListSheet'

const LiveOverlay = ({ live }) => {
  const currentUser = useSelector(selectCurrentUser)
  const [listOpen, setListOpen] = useState(false)

  const { viewers, likes, comments, pinnedProduct, sendLike, sendComment, commentError } = useLiveSocket({
    liveCode: live?.live_code,
    userId: currentUser?._id,
    userName: currentUser?.user_name ?? currentUser?.user_email ?? 'Khách',
    avatar: currentUser?.user_avatar,
    liveProducts: live?.live_products ?? [],
    initialViewers: live?.live_metrics?.viewer_count ?? 0,
    initialLikes: live?.live_metrics?.total_likes ?? 0
  })

  const liveProducts = live?.live_products ?? []
  const liveShop = live?.live_shopId
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

        {/* Pinned product — above comment area */}
        {/* <PinnedProductCard
          product={pinnedProduct}
          onClick={() => pinnedProduct && (onSelectProduct ? onSelectProduct(pinnedProduct) : setListOpen(true))}
        /> */}

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
        open={listOpen}
        onClose={() => setListOpen(false)}
        products={liveProducts}
        liveShop={liveShop}
      />
    </>
  )
}

export default LiveOverlay
