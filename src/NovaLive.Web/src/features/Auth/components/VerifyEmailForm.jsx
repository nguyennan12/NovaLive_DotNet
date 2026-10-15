import { useRef, useState, useEffect } from 'react'
import { Card as MuiCard } from '@mui/material'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Typography from '@mui/material/Typography'
import Zoom from '@mui/material/Zoom'
import CircularProgress from '@mui/material/CircularProgress'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { toast } from 'react-toastify'
import { verifyUserAPI, resendMailAPI } from '~/common/apis/services/userService'
import { authCardSx, authButtonSx } from '../authStyles'

const OTP_LENGTH = 6
const RESEND_COOLDOWN = 60

function VerifyEmailForm() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const userId = searchParams.get('userId') || ''
  const registeredEmail = searchParams.get('registeredEmail') || ''

  const [otp, setOtp] = useState(Array(OTP_LENGTH).fill(''))
  const [loading, setLoading] = useState(false)
  const [resendCooldown, setResendCooldown] = useState(RESEND_COOLDOWN)
  let canResend = resendCooldown <= 0

  const inputRefs = useRef([])

  useEffect(() => {
    if (resendCooldown > 0) {
      const timer = setTimeout(() => {
        setResendCooldown(prev => prev - 1)
      }, 1000)
      return () => clearTimeout(timer)
    }
  }, [resendCooldown])

  const handleChange = (index, value) => {
    if (!/^\d*$/.test(value)) return
    const newOtp = [...otp]
    if (value.length > 1) {
      const digits = value.replace(/\D/g, '').slice(0, OTP_LENGTH)
      digits.split('').forEach((digit, i) => {
        if (index + i < OTP_LENGTH) newOtp[index + i] = digit
      })
      setOtp(newOtp)
      const nextIndex = Math.min(index + digits.length, OTP_LENGTH - 1)
      inputRefs.current[nextIndex]?.focus()
      return
    }

    newOtp[index] = value
    setOtp(newOtp)

    if (value && index < OTP_LENGTH - 1) {
      inputRefs.current[index + 1]?.focus()
    }
  }

  const handleKeyDown = (index, e) => {
    if (e.key === 'Backspace') {
      if (!otp[index] && index > 0) {
        inputRefs.current[index - 1]?.focus()
      }
    }
    if (e.key === 'ArrowLeft' && index > 0) {
      inputRefs.current[index - 1]?.focus()
    }
    if (e.key === 'ArrowRight' && index < OTP_LENGTH - 1) {
      inputRefs.current[index + 1]?.focus()
    }
  }

  const handleSubmit = async () => {
    const code = otp.join('')
    if (!userId) {
      toast.error('Missing User ID. Please register again.')
      navigate('/register')
      return
    }
    setLoading(true)
    try {
      await toast.promise(
        verifyUserAPI({ userId, otpCode: code }),
        { pending: 'Verifying your code...' }
      )
      navigate(`/login?verifiedEmail=${encodeURIComponent(registeredEmail)}`)
    } catch {
      // error handled by API Interceptor
    } finally {
      setLoading(false)
    }
  }

  const handleResend = async () => {
    if (!canResend) return
    setResendCooldown(RESEND_COOLDOWN)
    setOtp(Array(OTP_LENGTH).fill(''))
    inputRefs.current[0]?.focus()

    toast.promise(
      resendMailAPI({ emailOrPhone: registeredEmail }),
      { pending: 'Sending a new code...', success: 'New code sent to your email!' }
    )
  }


  const gradientBg = 'linear-gradient(90deg, #3465c8, #69aedc, #8acdde)'

  return (
    <Box sx={{ width: '100%', display: 'flex', justifyContent: 'center' }}>
      <Zoom in={true} style={{ transitionDelay: '150ms' }}>
        <MuiCard sx={authCardSx}>
          <Box sx={{ mb: 3, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
            <Box sx={{
              width: 56,
              height: 56,
              borderRadius: '16px',
              background: 'rgba(37, 99, 235, 0.08)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              mb: 1.5
            }}>
              <svg width="28" height="28" viewBox="0 0 24 24" fill="none">
                <path
                  d="M20 4H4C2.9 4 2 4.9 2 6v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2z"
                  stroke="#2563eb"
                  strokeWidth="1.8"
                  fill="none"
                />
                <path
                  d="M2 6l10 7 10-7"
                  stroke="#2563eb"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                />
              </svg>
            </Box>
            <Typography variant="h5" sx={{ fontWeight: 800, color: '#0f172a', letterSpacing: '-0.02em' }}>
              Check your email
            </Typography>
            <Typography variant="body2" sx={{ color: '#64748b', textAlign: 'center', mt: 0.5, lineHeight: 1.5 }}>
              We sent a 6-digit verification code to <br />
              <strong style={{ color: '#0f172a' }}>{registeredEmail}</strong>
            </Typography>
          </Box>

          <Box sx={{ py: 1.5, display: 'flex', gap: '8px', justifyContent: 'center' }}>
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
                  width: '46px',
                  height: '54px',
                  textAlign: 'center',
                  fontSize: '22px',
                  fontWeight: '700',
                  border: '2px solid',
                  borderColor: digit ? '#2563eb' : '#e2e8f0',
                  borderRadius: '12px',
                  outline: 'none',
                  background: digit ? '#ffffff' : '#f8fafc',
                  color: '#0f172a',
                  transition: 'all 0.2s ease',
                  caretColor: '#2563eb',
                  '&:focus': {
                    borderColor: '#2563eb',
                    backgroundColor: '#ffffff',
                    boxShadow: '0 0 0 3px rgba(37, 99, 235, 0.15)'
                  }
                }}
              />
            ))}
          </Box>

          <Box sx={{ mt: 3 }}>
            <Button
              onClick={handleSubmit}
              variant="contained"
              size="large"
              fullWidth
              disabled={loading || otp.join('').length < OTP_LENGTH}
              sx={authButtonSx}
            >
              {loading ? <CircularProgress size={22} sx={{ color: '#fff' }} /> : 'Verify Code'}
            </Button>
          </Box>


        <Box sx={{ mt: 3, textAlign: 'center', display: 'flex', gap: 0.5, justifyContent: 'center', flexWrap: 'wrap' }}>
          <Typography variant="body2" sx={{ color: '#64748b' }}>{"Didn't receive the code?"}</Typography>
          <Typography
            variant="body2"
            onClick={handleResend}
            sx={{
              color: canResend
                ? '#2563eb'
                : '#94a3b8',
              cursor: canResend ? 'pointer' : 'default',
              fontWeight: canResend ? 700 : 400,
              transition: 'color 0.2s',
              '&:hover': canResend ? { textDecoration: 'underline' } : {}
            }}
          >
            {canResend ? 'Resend code' : `Resend in ${resendCooldown}s`}
          </Typography>
        </Box>
      </MuiCard>
    </Zoom>
  </Box>
  )
}

export default VerifyEmailForm