import BarChartIcon from '@mui/icons-material/BarChart'
import CloseIcon from '@mui/icons-material/Close'
import LocalOfferIcon from '@mui/icons-material/LocalOffer'
import PaymentsIcon from '@mui/icons-material/Payments'
import PendingActionsIcon from '@mui/icons-material/PendingActions'
import ShoppingBagIcon from '@mui/icons-material/ShoppingBag'
import { Box, Button, Chip, Dialog, DialogContent, DialogTitle, Divider, Grid, IconButton, Paper, Skeleton, Typography } from '@mui/material'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { DiscountCardMini } from '~/features/Discount/components/DIscountSelect/DiscountCardMini'
import { useDiscounts } from '~/features/Discount/hook/useDiscounts'
import { formatCompactVND } from '~/common/utils/formatters'
import { glassSx } from '~/theme'
import SectionTitle from '../../shared/SectionTitle'

const StatBox = ({ icon: Icon, label, value, color = 'secondary.main', isLoading }) => (
  <Box sx={{
    display: 'flex', flexDirection: 'column', alignItems: 'center',
    p: 1.5, borderRadius: 2,
    background: 'rgba(83,155,255,0.05)',
    border: '1px solid', borderColor: 'divider',
    gap: 0.5, textAlign: 'center', minWidth: '100px'
  }}>
    <Icon sx={{ color, fontSize: 26 }} />
    {isLoading
      ? <Skeleton width={44} height={24} />
      : <Typography sx={{ fontWeight: 700, fontSize: '1.05rem', color: 'primary.contrastText', lineHeight: 1.2 }}>{value}</Typography>
    }
    <Typography sx={{ fontSize: '0.7rem', color: 'text.secondary', lineHeight: 1.2 }}>{label}</Typography>
  </Box>
)

const VoucherDialog = ({ open, onClose }) => {
  const { filtered: discounts, isLoading } = useDiscounts({ scope: 'global', status: 'active' })

  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth PaperProps={{ sx: { borderRadius: 3 } }}>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', pb: 1 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <LocalOfferIcon sx={{ fontSize: 20, color: '#8b5cf6' }} />
          <Typography sx={{ fontWeight: 700, fontSize: '1rem' }}>Mã giảm giá của bạn</Typography>
          {!isLoading && discounts.length > 0 && (
            <Chip
              label={discounts.length}
              size="small"
              sx={{ height: 18, fontSize: '0.65rem', fontWeight: 700, bgcolor: '#8b5cf6', color: '#fff', '& .MuiChip-label': { px: 0.75 } }}
            />
          )}
        </Box>
        <IconButton size="small" onClick={onClose}><CloseIcon fontSize="small" /></IconButton>
      </DialogTitle>

      <DialogContent sx={{ pt: 0 }}>
        {isLoading ? (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
            <Skeleton variant="rounded" height={64} />
            <Skeleton variant="rounded" height={64} />
            <Skeleton variant="rounded" height={64} />
          </Box>
        ) : discounts.length === 0 ? (
          <Box sx={{ textAlign: 'center', py: 4 }}>
            <LocalOfferIcon sx={{ fontSize: 40, color: 'text.disabled', mb: 1 }} />
            <Typography sx={{ fontSize: '0.85rem', color: 'text.secondary' }}>
              Chưa có mã giảm giá khả dụng
            </Typography>
          </Box>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.25 }}>
            {discounts.map(d => (
              <DiscountCardMini key={d.code ?? d._id} discount={d} selected={false} onSelect={() => {}} />
            ))}
          </Box>
        )}
      </DialogContent>
    </Dialog>
  )
}

const UserVoucherButton = () => {
  const [open, setOpen] = useState(false)

  return (
    <>
      <Button
        fullWidth size="small" variant="outlined"
        startIcon={<LocalOfferIcon sx={{ fontSize: '16px !important' }} />}
        onClick={() => setOpen(true)}
        sx={{ borderColor: '#8b5cf6', color: '#8b5cf6', '&:hover': { background: 'rgba(139,92,246,0.08)' } }}
      >
        Xem mã giảm giá
      </Button>
      <VoucherDialog open={open} onClose={() => setOpen(false)} />
    </>
  )
}

const UserActivityStatsCard = ({ totalOrders, totalSpent, pendingOrders, isLoading }) => {
  const navigate = useNavigate()

  return (
    <Paper elevation={0} sx={{ borderRadius: 3, p: { xs: 2, sm: 2.5 }, bgcolor: 'primary.main', border: '1px solid', borderColor: 'divider', minWidth: '360px', ...glassSx }}>
      <SectionTitle title="Hoạt động" icon={BarChartIcon} accentColor="#f59e0b" />

      <Box sx={{ display: 'flex', justifyContent: 'space-around', mb: 2 }}>
        <Grid xs={4}>
          <StatBox icon={ShoppingBagIcon} label="Tổng đơn" value={totalOrders} isLoading={isLoading} />
        </Grid>
        <Grid xs={4}>
          <StatBox icon={PaymentsIcon} label="Đã chi" value={formatCompactVND(totalSpent)} color="#14c3eb" isLoading={isLoading} />
        </Grid>
        <Grid xs={4}>
          <StatBox icon={PendingActionsIcon} label="Chờ xử lý" value={pendingOrders} color="#f59e0b" isLoading={isLoading} />
        </Grid>
      </Box>

      <Button
        fullWidth size="small" variant="outlined"
        onClick={() => navigate('/orders')}
        sx={{ borderColor: 'secondary.main', color: 'secondary.main', mb: 1.5, '&:hover': { background: 'rgba(83,155,255,0.08)' } }}
      >
        Xem lịch sử đơn hàng
      </Button>

      <Divider sx={{ mb: 1.5, opacity: 0.5 }} />

      <UserVoucherButton />
    </Paper>
  )
}

export default UserActivityStatsCard
