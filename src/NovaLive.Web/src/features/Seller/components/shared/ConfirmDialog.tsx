import type { ReactNode } from 'react'
import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from '@mui/material'
import { sellerBtnStyles } from '../../theme/sellerBtnStyles'

export function ConfirmDialog({
  open,
  title,
  description,
  children,
  pending,
  disabled,
  onClose,
  onConfirm,
  confirmText = 'Xác nhận',
  danger = false
}: {
  open: boolean
  title: string
  description: string
  children?: ReactNode
  pending: boolean
  disabled?: boolean
  onClose: () => void
  onConfirm: () => void
  confirmText?: string
  danger?: boolean
}) {
  return (
    <Dialog
      open={open}
      onClose={pending ? undefined : onClose}
      maxWidth="sm"
      fullWidth
      slotProps={{ paper: { sx: { borderRadius: '12px' } } }}
      aria-labelledby="seller-confirm-title"
      aria-describedby="seller-confirm-description"
    >
      <DialogTitle id="seller-confirm-title" sx={{ fontWeight: 700, fontSize: '1.1rem', pt: 2.5, px: 3 }}>
        {title}
      </DialogTitle>
      <DialogContent sx={{ px: 3 }}>
        <DialogContentText id="seller-confirm-description" sx={{ mb: 2, color: '#475569', fontSize: '0.9rem' }}>
          {description}
        </DialogContentText>
        {children}
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2.5, pt: 1, gap: 1 }}>
        <Button
          disabled={pending}
          onClick={onClose}
          sx={{ textTransform: 'none', fontWeight: 600, color: '#64748b', borderRadius: '8px' }}
        >
          Quay lại
        </Button>
        <Button
          variant="contained"
          disabled={pending || disabled}
          onClick={onConfirm}
          sx={{
            ...(danger ? sellerBtnStyles.transDanger : sellerBtnStyles.primary),
            px: 2.5,
            py: 0.8,
            fontSize: '0.875rem'
          }}
        >
          {pending ? 'Đang xử lý…' : confirmText}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
