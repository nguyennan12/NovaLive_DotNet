import WarningAmberRoundedIcon from '@mui/icons-material/WarningAmberRounded'
import { Box, Button, Dialog, DialogActions, DialogContent, Typography } from '@mui/material'

export const DeleteDialog = ({ open, discount, onConfirm, onCancel }) => (
  <Dialog
    open={open}
    onClose={onCancel}
    maxWidth='xs'
    fullWidth
    PaperProps={{ sx: { borderRadius: '14px', boxShadow: '0 20px 60px rgba(0,0,0,0.12)' } }}
  >
    <DialogContent sx={{ px: 3, pt: 3, pb: 1 }}>
      <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', textAlign: 'center', gap: 1.5 }}>
        <Box sx={{
          width: 52, height: 52, borderRadius: '14px', bgcolor: '#fef2f2',
          display: 'flex', alignItems: 'center', justifyContent: 'center'
        }}>
          <WarningAmberRoundedIcon sx={{ fontSize: 26, color: '#ef4444' }} />
        </Box>
        <Box>
          <Typography sx={{ fontSize: '0.95rem', fontWeight: 700, color: '#1a1a1a', mb: 0.5 }}>
            Xoá discount này?
          </Typography>
          <Typography sx={{ fontSize: '0.8rem', color: '#6b7280', lineHeight: 1.5 }}>
            Bạn sắp xoá <strong style={{ color: '#2d2d2d' }}>{discount?.name}</strong>. Hành động này không thể hoàn tác.
          </Typography>
        </Box>
      </Box>
    </DialogContent>
    <DialogActions sx={{ px: 3, pb: 3, pt: 1.5, gap: 1 }}>
      <Button
        fullWidth
        onClick={onCancel}
        sx={{
          fontSize: '0.85rem', fontWeight: 600, borderRadius: '8px',
          border: '1px solid #eeeeee', color: '#6b7280',
          '&:hover': { bgcolor: '#f9fafb' }
        }}
      >
        Huỷ
      </Button>
      <Button
        fullWidth
        onClick={onConfirm}
        variant='contained'
        sx={{
          bgcolor: '#ef4444', color: '#fff', fontWeight: 700,
          fontSize: '0.85rem', borderRadius: '8px',
          boxShadow: '0 2px 8px rgba(239,68,68,0.3)',
          '&:hover': { bgcolor: '#dc2626' }
        }}
      >
        Xoá
      </Button>
    </DialogActions>
  </Dialog>
)

