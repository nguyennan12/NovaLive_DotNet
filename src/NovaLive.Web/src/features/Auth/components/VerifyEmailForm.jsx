import { useRef, useState, useEffect } from 'react'
import { Card as MuiCard } from '@mui/material'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Typography from '@mui/material/Typography'
import Zoom from '@mui/material/Zoom'
import CircularProgress from '@mui/material/CircularProgress'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { toast } from 'react-toastify'
import { verifyUserAPI } from '~/common/apis/services/userService'
import { resendMailAPI } from '~/common/apis/services/userService'

const OTP_LENGTH = 6
const RESEND_COOLDOWN = 60

function VerifyEmailForm() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const registeredEmail = searchParams.get('registeredEmail') || ''
  const userId = searchParams.get('userId') || ''

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
    setLoading(true)
    try {
      await toast.promise(
        verifyUserAPI({ userId, email: registeredEmail, otpCode: code }),
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
      resendMailAPI({ email: registeredEmail }),
      { pending: 'Sending a new code...', success: 'New code sent to your email!' }
    )
  }

  const gradientBg = 'linear-gradient(90deg, #3465c8, #69aedc, #8acdde)'

  return (
    <Zoom in={true} style={{ transitionDelay: '200ms' }}>
      <MuiCard sx={{
        minWidth: '380px',
        maxWidth: '380px',
        borderRadius: '12px',
        boxShadow: '0 0 3px 0.5px rgba(0,0,0,0.15)'
      }}>
        <Box sx={{ margin: '1.5em 1em 0.5em', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 0.5 }}>
          <Box sx={{
            width: 56,
            height: 56,
            borderRadius: '50%',
            background: 'rgba(52, 101, 200, 0.1)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            mb: 1
          }}>
            <svg width="28" height="28" viewBox="0 0 24 24" fill="none">
              <path
                d="M20 4H4C2.9 4 2 4.9 2 6v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2z"
                stroke="#3465c8"
                strokeWidth="1.8"
                fill="none"
              />
              <path
                d="M2 6l10 7 10-7"
                stroke="#3465c8"
                strokeWidth="1.8"
                strokeLinecap="round"
              />
            </svg>
          </Box>
          <Typography variant="h6" sx={{ fontWeight: '600' }}>Check your email</Typography>
          <Typography variant="body2" sx={{ color: '#838383', textAlign: 'center', lineHeight: 1.5 }}>
            We sent a 6-digit verification code to <br />
            <strong style={{ color: '#444' }}>{registeredEmail}</strong>
          </Typography>
        </Box>

        <Box sx={{ padding: '1em 1.5em', display: 'flex', gap: '10px', justifyContent: 'center' }}>
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
                width: '44px',
                height: '52px',
                textAlign: 'center',
                fontSize: '20px',
                fontWeight: '600',
                border: '2px solid',
                borderColor: digit
                  ? '#3465c8'
                  : 'rgba(0,0,0,0.2)',
                borderRadius: '8px',
                outline: 'none',
                background: '#fff',
                color: '#111',
                transition: 'border-color 0.2s, box-shadow 0.2s',
                caretColor: 'transparent',
                '&:focus': {
                  borderColor: 'secondary.main',
                  boxShadow: '0 0 0 3px rgba(52,101,200,0.15)'
                }
              }}
            />
          ))}
        </Box>

        <Box sx={{ padding: '0 1em 1em' }}>
          <Button
            onClick={handleSubmit}
            variant="contained"
            size="large"
            fullWidth
            disabled={loading || otp.join('').length < OTP_LENGTH}
            sx={{
              background: gradientBg,
              color: '#ffffff',
              fontWeight: 600,
              letterSpacing: '0.5px',
              '&:disabled': {
                opacity: 0.6,
                color: '#fff'
              }
            }}
          >
            {loading ? <CircularProgress size={22} sx={{ color: '#fff' }} /> : 'Verify Email'}
          </Button>
        </Box>

        <Box sx={{ padding: '0 1em 1.5em', textAlign: 'center', display: 'flex', gap: 0.5, justifyContent: 'center', flexWrap: 'wrap' }}>
          <Typography variant="body2" sx={{ color: '#838383' }}>Didn&apost receive the code?</Typography>
          <Typography
            variant="body2"
            onClick={handleResend}
            sx={{
              color: canResend
                ? 'secondary.main'
                : '#838383',
              cursor: canResend ? 'pointer' : 'default',
              fontWeight: canResend ? 600 : 400,
              transition: 'color 0.2s',
              '&:hover': canResend ? { textDecoration: 'underline' } : {}
            }}
          >
            {canResend ? 'Resend code' : `Resend in ${resendCooldown}s`}
          </Typography>
        </Box>
      </MuiCard>
    </Zoom>
  )
}

export default VerifyEmailForm