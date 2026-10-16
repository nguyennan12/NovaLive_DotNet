import Button from '@mui/material/Button'
import Dialog from '@mui/material/Dialog'
import DialogActions from '@mui/material/DialogActions'
import DialogContent from '@mui/material/DialogContent'
import DialogTitle from '@mui/material/DialogTitle'
import Typography from '@mui/material/Typography'
import { useNavigate } from 'react-router-dom'

const EndCofirmDialog = ({ confirmEnd, setConfirmEnd, endLive }) => {
  const navigate = useNavigate()
  const handleEndConfirm = async () => {
    setConfirmEnd(false)
    await endLive()
    navigate('/')
  }
  return (
    <Dialog
      open={confirmEnd}
      onClose={() => setConfirmEnd(false)}
      PaperProps={{
        sx: { borderRadius: 3, minWidth: 300 }
      }}
    >
      <DialogTitle fontWeight={600}>Kết thúc buổi live?</DialogTitle>
      <DialogContent>
        <Typography color="text.secondary">
          Người xem sẽ bị ngắt kết nối. Bạn có chắc muốn kết thúc không?
        </Typography>
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2, gap: 1 }}>
        <Button
          onClick={() => setConfirmEnd(false)}
          variant="outlined"
          sx={{ borderRadius: 2 }}
        >
          Tiếp tục live
        </Button>
        <Button
          onClick={handleEndConfirm}
          variant="contained"
          color="error"
          sx={{ borderRadius: 2 }}
        >
          Kết thúc
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default EndCofirmDialog