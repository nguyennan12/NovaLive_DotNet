import { Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from '@mui/material'

const ConfirmDialog = ({
  open,
  title = 'Xác nhận',
  content = 'Bạn có chắc chắn muốn thực hiện hành động này?',
  confirmText = 'Xác nhận',
  cancelText = 'Hủy',
  confirmColor = '#f72626',
  isLoading = false,
  onConfirm,
  onClose
}) => {
  return (
    <Dialog
      open={open}
      onClose={!isLoading ? onClose : undefined}
      maxWidth="xs"
      fullWidth
    >
      <DialogTitle sx={{ fontWeight: 'bold', fontSize: '1.25rem' }}>
        {title}
      </DialogTitle>

      <DialogContent>
        <DialogContentText>
          {content}
        </DialogContentText>
      </DialogContent>

      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button
          onClick={onClose}
          disabled={isLoading}
          color="inherit"
          variant="text"
        >
          {cancelText}
        </Button>
        <Button
          onClick={onConfirm}
          color={confirmColor}
          variant="contained"
          disabled={isLoading}
          sx={{ color: '#ff2828ff', ':hover': { bgcolor: '#ff2828ff', color: 'white' } }}
          startIcon={isLoading ? <CircularProgress size={20} color="inherit" /> : null}
          disableElevation
        >
          {confirmText}
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default ConfirmDialog