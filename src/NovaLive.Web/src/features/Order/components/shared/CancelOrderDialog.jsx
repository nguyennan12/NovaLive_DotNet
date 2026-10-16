import {
  Button, Dialog, DialogActions, DialogContent,
  DialogTitle, Typography, CircularProgress
} from '@mui/material'
import WarningAmberRoundedIcon from '@mui/icons-material/WarningAmberRounded'

function CancelOrderDialog({ open, onClose, onConfirm, loading, trackingNumber }) {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="xs"
      fullWidth
      PaperProps={{
        sx: {
          borderRadius: 3,
          border: '1px solid',
          borderColor: 'divider',
          bgcolor: 'primary.main'
        }
      }}
    >
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1.5, pb: 1 }}>
        <WarningAmberRoundedIcon sx={{ color: '#f59e0b', fontSize: 26 }} />
        <Typography sx={{ fontWeight: 700, fontSize: '1rem', color: 'primary.contrastText' }}>
          Xác nhận hủy đơn hàng
        </Typography>
      </DialogTitle>

      <DialogContent sx={{ pt: 0 }}>
        <Typography sx={{ fontSize: '0.875rem', color: 'rgba(45,45,45,0.7)', lineHeight: 1.6 }}>
          Bạn có chắc muốn hủy đơn hàng{' '}
          <strong style={{ color: '#2d2d2d' }}>{trackingNumber}</strong>?
        </Typography>
        <Typography sx={{ fontSize: '0.8rem', color: 'rgba(45,45,45,0.5)', mt: 1 }}>
          Kho hàng sẽ được nhả lại sau khi hủy. Thao tác này không thể hoàn tác.
        </Typography>
      </DialogContent>

      <DialogActions sx={{ px: 3, pb: 2.5, gap: 1 }}>
        <Button
          onClick={onClose}
          disabled={loading}
          sx={{
            flex: 1, fontWeight: 600, fontSize: '0.875rem',
            border: '1px solid', borderColor: 'divider',
            color: 'primary.contrastText',
            '&:hover': { bgcolor: 'rgba(0,0,0,0.04)' }
          }}
        >
          Quay lại
        </Button>
        <Button
          onClick={onConfirm}
          disabled={loading}
          startIcon={loading ? <CircularProgress size={15} sx={{ color: '#fff' }} /> : null}
          sx={{
            flex: 1, fontWeight: 700, fontSize: '0.875rem',
            bgcolor: '#dc2626', color: '#fff',
            '&:hover': { bgcolor: '#b91c1c' },
            '&.Mui-disabled': { bgcolor: 'rgba(220,38,38,0.5)', color: '#fff' }
          }}
        >
          {loading ? 'Đang hủy...' : 'Hủy đơn hàng'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default CancelOrderDialog
