import CloseRoundedIcon from '@mui/icons-material/CloseRounded'
import LocalOfferRoundedIcon from '@mui/icons-material/LocalOfferRounded'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import {
  Box, Button, CircularProgress, Dialog, DialogContent,
  DialogTitle, Divider, IconButton, InputAdornment,
  TextField, Typography
} from '@mui/material'
import { useState } from 'react'
import { toast } from 'react-toastify'
import { useDispatch, useSelector } from 'react-redux'
import { checkAvailableAPI } from '~/common/apis/services/discountService'
import { selectAppliedShopDiscounts, clearShopDiscount, setShopDiscount } from '~/store/discount/discountSlice'
import { useDiscounts } from '../../hook/useDiscounts'
import { DiscountCardMini } from './DiscountCardMini'

function ShopDiscountPopup({ open, onClose, shopId, shopName, subtotal = 0 }) {
  const dispatch = useDispatch()
  const shopDiscounts = useSelector(selectAppliedShopDiscounts)
  const appliedDiscount = shopDiscounts?.[String(shopId)]
  const [codeInput, setCodeInput] = useState('')
  const [checkingId, setCheckingId] = useState(null)

  const filters = { shopId, search: codeInput, status: 'active' }
  const { filtered, isLoading } = useDiscounts(filters)

  const handleSelect = async (discount) => {
    if (appliedDiscount?.id === discount.id) {
      dispatch(clearShopDiscount(shopId))
      onClose()
      return
    }

    setCheckingId(discount.id)
    try {
      await toast.promise(
        checkAvailableAPI(discount.code, subtotal),
        {
          pending: 'Đang kiểm tra voucher...',
          success: 'Áp dụng voucher thành công!',
          error: { render: ({ data }) => data?.response?.data?.message || data?.message || 'Voucher không khả dụng' }
        }
      )
      dispatch(setShopDiscount({ shopId: String(shopId), discount }))
      onClose()
    } catch {
      // toast.promise đã hiển thị lỗi
    } finally {
      setCheckingId(null)
    }
  }

  const handleApplyCode = async () => {
    const trimmed = codeInput.trim().toUpperCase()
    if (!trimmed) return
    const found = filtered.find(d => d.code?.toUpperCase() === trimmed)
    if (found) {
      await handleSelect(found)
      setCodeInput('')
    }
  }

  const handleClose = () => {
    setCodeInput('')
    onClose()
  }

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      maxWidth="sm"
      fullWidth
      PaperProps={{
        sx: { borderRadius: 3, maxHeight: '82vh', mx: 2 }
      }}
    >
      {/* Header */}
      < DialogTitle sx={{ px: 2.5, py: 2, pb: 1.5 }
      }>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Box sx={{
            width: 32, height: 32, borderRadius: '9px',
            bgcolor: 'rgba(245,158,11,0.1)',
            display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0
          }}>
            <LocalOfferRoundedIcon sx={{ fontSize: 16, color: 'fourth.main' }} />
          </Box>
          <Box sx={{ flex: 1, minWidth: 0 }}>
            <Typography sx={{ fontSize: '0.93rem', fontWeight: 700, color: 'primary.contrastText' }}>
              Mã giảm giá của shop
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.15 }}>
              <StorefrontRoundedIcon sx={{ fontSize: 11, color: 'secondary.main' }} />
              <Typography noWrap sx={{ fontSize: '0.7rem', color: 'rgba(45,45,45,0.5)' }}>
                {shopName}
              </Typography>
            </Box>
          </Box>
          <IconButton size="small" onClick={handleClose} sx={{ color: 'rgba(45,45,45,0.38)', ml: 1 }}>
            <CloseRoundedIcon sx={{ fontSize: 18 }} />
          </IconButton>
        </Box>
      </DialogTitle >

      <Divider />

      <DialogContent sx={{ px: 2.5, py: 2, display: 'flex', flexDirection: 'column', gap: 1.75 }}>
        {/* Applied discount banner */}
        {appliedDiscount && (
          <Box sx={{
            display: 'flex', alignItems: 'center', gap: 1, px: 1.5, py: 1,
            borderRadius: 2, bgcolor: 'rgba(52,133,247,0.06)',
            border: '1px solid rgba(52,133,247,0.18)'
          }}>
            <Typography sx={{ flex: 1, fontSize: '0.78rem', color: 'secondary.main', fontWeight: 600 }}>
              Đang dùng: {appliedDiscount.code || appliedDiscount.name}
            </Typography>
            <Button
              size="small" color="error"
              onClick={() => { dispatch(clearShopDiscount(shopId)); onClose() }}
              sx={{ textTransform: 'none', fontSize: '0.7rem', py: 0.2, minWidth: 'unset' }}
            >
              Bỏ chọn
            </Button>
          </Box>
        )}

        {/* Code input */}
        <TextField
          size="small"
          placeholder="Nhập mã giảm giá..."
          value={codeInput}
          onChange={(e) => setCodeInput(e.target.value.toUpperCase())}
          onKeyDown={(e) => e.key === 'Enter' && handleApplyCode()}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <SearchRoundedIcon sx={{ fontSize: 15, color: 'rgba(45,45,45,0.35)' }} />
                </InputAdornment>
              ),
              endAdornment: (
                <InputAdornment position="end">
                  <Button
                    size="small"
                    disabled={!codeInput.trim()}
                    onClick={handleApplyCode}
                    sx={{
                      textTransform: 'none', fontWeight: 600, fontSize: '0.73rem',
                      borderRadius: '7px', py: 0.35, px: 1.2,
                      bgcolor: 'secondary.main', color: '#fff',
                      '&:hover': { bgcolor: '#4e96f6' },
                      '&.Mui-disabled': { bgcolor: 'rgba(0,0,0,0.1)' }
                    }}
                  >
                    Áp dụng
                  </Button>
                </InputAdornment>
              )
            }
          }}
          sx={{
            '& .MuiOutlinedInput-root': {
              borderRadius: '10px', fontSize: '0.82rem', bgcolor: '#f5f7ff', pr: 0.5
            }
          }}
        />

        {isLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 5 }}>
            <CircularProgress size={26} sx={{ color: 'secondary.main' }} />
          </Box>
        ) : filtered.length === 0 ? (
          <Box sx={{ py: 5, textAlign: 'center' }}>
            <LocalOfferRoundedIcon sx={{ fontSize: 38, color: 'rgba(45,45,45,0.18)', mb: 1, display: 'block', mx: 'auto' }} />
            <Typography sx={{ fontSize: '0.8rem', color: 'rgba(45,45,45,0.42)' }}>
              Không có mã giảm giá nào
            </Typography>
          </Box>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.2, pb: 0.5 }}>
            {filtered.map(discount => (
              <DiscountCardMini
                key={discount.id}
                discount={discount}
                selected={appliedDiscount?.id === discount.id}
                onSelect={() => handleSelect(discount)}
                subtotal={subtotal}
                loading={checkingId === discount.id}
              />
            ))}
          </Box>
        )}
      </DialogContent>
    </Dialog >
  )
}

export default ShopDiscountPopup
