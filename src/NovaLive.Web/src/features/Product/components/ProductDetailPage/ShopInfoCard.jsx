import ChatBubbleOutlineRoundedIcon from '@mui/icons-material/ChatBubbleOutlineRounded'
import StarRoundedIcon from '@mui/icons-material/StarRounded'
import StoreRoundedIcon from '@mui/icons-material/StoreRounded'
import { Avatar, Box, Button, Divider, Typography } from '@mui/material'
import { glassSx } from '~/theme'
import { useQuery } from '@tanstack/react-query'
import { getInfoShopAPI } from '~/common/apis/services/shopService'

const ShopInfoCard = ({ shopId }) => {

  const { data: shop } = useQuery({
    queryKey: ['shop', shopId],
    queryFn: () => getInfoShopAPI(shopId),
    enabled: !!shopId
  })

  if (!shop) return null

  return (
    <Box sx={{
      display: 'flex',
      alignItems: { xs: 'flex-start', sm: 'center' },
      flexDirection: { xs: 'column', sm: 'row' },
      gap: { xs: 2, sm: 3 },
      bgcolor: '#fafafa',
      ...glassSx,
      borderRadius: 3,
      p: { xs: 2, sm: 2.5 }
    }}>
      {/* Avatar + name */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, flexShrink: 0 }}>
        <Avatar
          src={shop.shop_logo}
          sx={{ width: 56, height: 56, bgcolor: 'secondary.main', fontSize: '1.4rem', fontWeight: 800 }}
        >
          {shop.shop_name?.charAt(0)}
        </Avatar>
        <Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <StoreRoundedIcon sx={{ fontSize: 14, color: 'secondary.main' }} />
            <Typography sx={{ fontSize: '0.875rem', fontWeight: 800, color: 'primary.contrastText' }}>
              {shop.shop_name}
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.4, mt: 0.3 }}>
            <StarRoundedIcon sx={{ fontSize: 13, color: '#f59e0b' }} />
            <Typography sx={{ fontSize: '0.78rem', color: '#f59e0b', fontWeight: 700 }}>
              {shop.shop_metrics?.rating_avg?.toFixed(1) ?? '0.0'}
            </Typography>
            <Typography sx={{ fontSize: '0.72rem', color: '#aaa' }}>
              ({shop.shop_metrics?.rating_count ?? 0} đánh giá)
            </Typography>
          </Box>
        </Box>
      </Box>

      <Divider orientation="vertical" flexItem sx={{ display: { xs: 'none', sm: 'block' }, borderColor: '#eee' }} />

      {/* Stats */}
      <Box sx={{ display: 'flex', gap: { xs: 2, sm: 3 }, flexWrap: 'wrap', flex: 1 }}>
        <Box>
          <Typography sx={{ fontSize: '0.68rem', color: '#6f97ddff', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.06em' }}>
            Sản phẩm
          </Typography>
          <Typography sx={{ fontSize: '0.85rem', fontWeight: 700, color: 'primary.contrastText', mt: 0.25 }}>
            {shop.shop_metrics?.total_products ?? 0}
          </Typography>
        </Box>
        <Box>
          <Typography sx={{ fontSize: '0.68rem', color: '#6f97ddff', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.06em' }}>
            Thời gian phản hồi
          </Typography>
          <Typography sx={{ fontSize: '0.85rem', fontWeight: 700, color: 'primary.contrastText', mt: 0.25 }}>
            {shop.shop_metrics?.response_time ?? '—'}
          </Typography>
        </Box>
        <Box>
          <Typography sx={{ fontSize: '0.68rem', color: '#6f97ddff', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.06em' }}>
            Đã bán
          </Typography>
          <Typography sx={{ fontSize: '0.85rem', fontWeight: 700, color: 'primary.contrastText', mt: 0.25 }}>
            {shop.shop_metrics?.total_sold ?? 0}
          </Typography>
        </Box>
      </Box>

      {/* Actions */}
      <Box sx={{ display: 'flex', gap: 1, flexShrink: 0 }}>
        <Button
          variant="outlined"
          size="small"
          startIcon={<ChatBubbleOutlineRoundedIcon sx={{ fontSize: 14 }} />}
          sx={{
            textTransform: 'none', borderRadius: '9px', fontWeight: 600, fontSize: '0.8rem',
            borderColor: 'secondary.main', color: 'secondary.main',
            '&:hover': { bgcolor: 'rgba(52,133,247,0.06)' }
          }}
        >
          Chat
        </Button>
        <Button
          variant="outlined"
          size="small"
          startIcon={<StoreRoundedIcon sx={{ fontSize: 14 }} />}
          sx={{
            textTransform: 'none', borderRadius: '9px', fontWeight: 600, fontSize: '0.8rem',
            borderColor: '#e0e0e0', color: '#555',
            '&:hover': { bgcolor: '#f5f5f5', borderColor: '#bbb' }
          }}
        >
          Xem shop
        </Button>
      </Box>
    </Box>
  )
}

export default ShopInfoCard
