import type { ReactNode } from 'react'
import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from '@mui/material'

export function ConfirmDialog({ open, title, description, children, pending, disabled, onClose, onConfirm, confirmText = 'Xác nhận', danger = false }: { open: boolean; title: string; description: string; children?: ReactNode; pending: boolean; disabled?: boolean; onClose: () => void; onConfirm: () => void; confirmText?: string; danger?: boolean }) {
  return <Dialog open={open} onClose={pending ? undefined : onClose} maxWidth='sm' fullWidth aria-labelledby='seller-confirm-title' aria-describedby='seller-confirm-description'>
    <DialogTitle id='seller-confirm-title'>{title}</DialogTitle><DialogContent><DialogContentText id='seller-confirm-description' sx={{ mb: 2 }}>{description}</DialogContentText>{children}</DialogContent>
    <DialogActions><Button disabled={pending} onClick={onClose}>Quay lại</Button><Button color={danger ? 'error' : 'secondary'} variant='contained' disabled={pending || disabled} onClick={onConfirm}>{pending ? 'Đang xử lý…' : confirmText}</Button></DialogActions>
  </Dialog>
}
