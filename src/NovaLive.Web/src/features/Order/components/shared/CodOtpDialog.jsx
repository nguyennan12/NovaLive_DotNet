import MarkEmailReadRoundedIcon from '@mui/icons-material/MarkEmailReadRounded'
import {
  Box, Button, CircularProgress,
  Dialog, DialogContent, DialogTitle,
  Divider, Typography
} from '@mui/material'
import { useEffect, useRef, useState } from 'react'
import { useOtpTimer } from '~/common/hooks/useOtpTimer'
import { glassSx } from '~/theme'
import { useOrderMutation } from '../../hooks/useOrderMutation'

const OTP_LENGTH = 6
const RESEND_COOLDOWN = 60

function CodOtpDialog({ open, onClose, orderId, email, onSuccess }) {
  const [otp, setOtp] = useState(Array(OTP_LENGTH).fill(''))
  const [prevOpen, setPrevOpen] = useState(open)
  const inputRefs = useRef([])

  if (prevOpen !== open) {
    setPrevOpen(open)
    if (!open) setOtp(Array(OTP_LENGTH).fill(''))
  }

  const { resendCooldown, setResendCooldown, canResend } = useOtpTimer(RESEND_COOLDOWN, open)
  const { confirmMutation, resendMutation } = useOrderMutation({
    onSuccessConfirm: onSuccess
  })


  useEffect(() => {
    if (open) setTimeout(() => inputRefs.current[0]?.focus(), 100)
  }, [open])

  const handleChange = (index, value) => {
    if (!/^\d*$/.test(value)) return
    const newOtp = [...otp]
    if (value.length > 1) {
      const digits = value.replace(/\D/g, '').slice(0, OTP_LENGTH)
      digits.split('').forEach((digit, i) => {
        if (index + i < OTP_LENGTH) newOtp[index + i] = digit
      })
      setOtp(newOtp)
      inputRefs.current[Math.min(index + digits.length, OTP_LENGTH - 1)]?.focus()
      return
    }
    newOtp[index] = value
    setOtp(newOtp)
    if (value && index < OTP_LENGTH - 1) inputRefs.current[index + 1]?.focus()
  }

  const handleKeyDown = (index, e) => {
    if (e.key === 'Backspace' && !otp[index] && index > 0) inputRefs.current[index - 1]?.focus()
    if (e.key === 'ArrowLeft' && index > 0) inputRefs.current[index - 1]?.focus()
    if (e.key === 'ArrowRight' && index < OTP_LENGTH - 1) inputRefs.current[index + 1]?.focus()
  }

  const handleSubmit = () => {
    const otpToken = otp.join('')
    if (otpToken.length < OTP_LENGTH) return
    confirmMutation.mutate({ orderId, email, otpToken })
  }

  const handleResend = () => {
    if (!canResend || resendMutation.isPending) return
    resendMutation.mutate({ email, title: 'Xác thực đơn hàng' }, {
      onSuccess: () => {
        setResendCooldown(RESEND_COOLDOWN)
        setOtp(Array(OTP_LENGTH).fill(''))
        inputRefs.current[0]?.focus()
      }
    })
  }

  return (
    <Dialog
      open={open}
      onClose={onClose}
      fullWidth
      maxWidth="xs"
      PaperProps={{
        sx: {
          ...glassSx,
          bgcolor: 'primary.main',
          borderRadius: 3,
          border: '1px solid',
          borderColor: 'divider',
          m: { xs: 1.5, sm: 3 }
        }
      }}
    >
      <DialogTitle sx={{ pb: 1.5, textAlign: 'center' }}>
        <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 1 }}>
          <Box sx={{
            width: 54, height: 54, borderRadius: '50%',
            bgcolor: 'rgba(52,133,247,0.12)',
            display: 'flex', alignItems: 'center', justifyContent: 'center'
          }}>
            <MarkEmailReadRoundedIcon sx={{ fontSize: 28, color: 'secondary.main' }} />
          </Box>
          <Typography sx={{ fontWeight: 700, fontSize: '1rem', color: 'primary.contrastText' }}>
            Xác nhận đơn hàng COD
          </Typography>
          <Typography sx={{
            fontSize: '0.78rem', color: 'rgba(45,45,45,0.55)',
            textAlign: 'center', lineHeight: 1.55, fontWeight: 400
          }}>
            Mã xác nhận 6 chữ số đã được gửi tới<br />
            <strong>{email}</strong>
          </Typography>
        </Box>
      </DialogTitle>

      <Divider sx={{ borderColor: 'divider' }} />

      <DialogContent sx={{
        pt: 2.5, pb: 3,
        display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2.5
      }}>
        {/* OTP boxes */}
        <Box sx={{ display: 'flex', gap: 1 }}>
          {otp.map((digit, index) => (
            <Box
              key={index}
              component="input"
              ref={el => inputRefs.current[index] = el}
              type="text"
              inputMode="numeric"
              maxLength={6}
              value={digit}
              onChange={e => handleChange(index, e.target.value)}
              onKeyDown={e => handleKeyDown(index, e)}
              onFocus={e => e.target.select()}
              sx={{
                width: '44px', height: '52px',
                textAlign: 'center', fontSize: '20px', fontWeight: 700,
                border: '2px solid',
                borderColor: digit ? 'secondary.main' : 'divider',
                borderRadius: '10px', outline: 'none',
                bgcolor: 'transparent',
                color: 'primary.contrastText',
                transition: 'border-color 0.2s, box-shadow 0.2s',
                caretColor: 'transparent',
                '&:focus': {
                  borderColor: 'secondary.main',
                  boxShadow: '0 0 0 3px rgba(52,133,247,0.18)'
                }
              }}
            />
          ))}
        </Box>

        {/* Confirm button */}
        <Button
          variant="contained"
          fullWidth
          disabled={otp.join('').length < OTP_LENGTH || confirmMutation.isPending}
          onClick={handleSubmit}
          sx={{
            fontWeight: 700, borderRadius: 2, py: 1.25,
            background: 'linear-gradient(90deg, #568dfb, #69aedc)',
            color: '#fff',
            '&:hover': { boxShadow: '0 4px 16px rgba(52,133,247,0.35)' },
            '&:disabled': { opacity: 0.6, color: '#fff' }
          }}
        >
          {confirmMutation.isPending
            ? <CircularProgress size={20} sx={{ color: '#fff' }} />
            : 'Xác nhận đặt hàng'}
        </Button>

        {/* Resend */}
        <Box sx={{ display: 'flex', gap: 0.75, alignItems: 'center', flexWrap: 'wrap', justifyContent: 'center' }}>
          <Typography sx={{ fontSize: '0.78rem', color: 'rgba(45,45,45,0.5)' }}>
            Chưa nhận được mã?
          </Typography>
          <Typography
            onClick={handleResend}
            sx={{
              fontSize: '0.78rem',
              fontWeight: canResend ? 700 : 400,
              color: canResend ? 'secondary.main' : 'rgba(45,45,45,0.4)',
              cursor: canResend && !resendMutation.isPending ? 'pointer' : 'default',
              transition: 'color 0.2s',
              '&:hover': canResend ? { textDecoration: 'underline' } : {}
            }}
          >
            {resendMutation.isPending
              ? 'Đang gửi...'
              : canResend
                ? 'Gửi lại mã'
                : `Gửi lại sau ${resendCooldown}s`}
          </Typography>
        </Box>
      </DialogContent>
    </Dialog>
  )
}

export default CodOtpDialog
