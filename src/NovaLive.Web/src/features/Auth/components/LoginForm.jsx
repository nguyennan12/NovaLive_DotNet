import { useState } from 'react'
import { Card as MuiCard } from '@mui/material'
import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import Zoom from '@mui/material/Zoom'
import InputAdornment from '@mui/material/InputAdornment'
import IconButton from '@mui/material/IconButton'
import VisibilityOutlinedIcon from '@mui/icons-material/VisibilityOutlined'
import VisibilityOffOutlinedIcon from '@mui/icons-material/VisibilityOffOutlined'
import { useForm } from 'react-hook-form'
import { useQueryClient } from '@tanstack/react-query'
import { useDispatch, useSelector } from 'react-redux'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { toast } from 'react-toastify'
import FieldErrorAlert from '~/common/components/common/form/FieldErrorAlert'
import { loginUserAPI } from '~/store/user/userSlice'
import { clearGuestCart, selectGuestItems, setSelectedIds } from '~/store/cart/cartSlice'
import { addToCartAPI } from '~/common/apis/services/cartService'
import AuthHeader from './AuthHeader'
import { authCardSx, authInputSx, authButtonSx } from '../authStyles'
import {
  FIELD_REQUIRED_MESSAGE
} from '~/common/utils/validators'

function LoginForm() {
  const { register, handleSubmit, formState: { errors } } = useForm()
  const [searchParams] = useSearchParams()
  const [showPassword, setShowPassword] = useState(false)

  const dispatch = useDispatch()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const guestItems = useSelector(selectGuestItems)

  const registeredEmail = searchParams.get('registeredEmail')
  const verifiedEmail = searchParams.get('verifiedEmail')
  const isCheckoutRedirect = searchParams.get('redirect') === '/order'

  const onSubmitLogin = async (data) => {
    const { email, password } = data
    const loginPromise = dispatch(loginUserAPI({ email, password })).unwrap()
    try {
      await toast.promise(loginPromise, {
        pending: 'Đang đăng nhập...',
        success: 'Đăng nhập thành công!',
        error: { render: ({ data: message }) => message || 'Đăng nhập thất bại. Vui lòng thử lại.' }
      })

      try {
        const syncedIds = []
        for (const item of guestItems) {
          const saved = await addToCartAPI({
            skuId: item.skuId,
            quantity: item.quantity,
            livestreamSessionId: item.livestreamSessionId
          })
          if (saved?.cartItemId) syncedIds.push(String(saved.cartItemId))
        }
        if (guestItems.length > 0) {
          dispatch(clearGuestCart())
          dispatch(setSelectedIds(syncedIds))
          await queryClient.invalidateQueries({ queryKey: ['cart'] })
        }
      } catch (error) {
        toast.error(error?.response?.data?.detail || 'Đăng nhập thành công nhưng chưa đồng bộ được giỏ hàng. Vui lòng thử lại.')
        navigate('/cart')
        return
      }

      const requestedRedirect = searchParams.get('redirect')
      const redirect = requestedRedirect?.startsWith('/') && !requestedRedirect.startsWith('//')
        ? requestedRedirect : '/'
      navigate(redirect)
    } catch {
      // toast.promise already shows the login error.
    }
  }

  return (
    <form onSubmit={handleSubmit(onSubmitLogin)} style={{ width: '100%', display: 'flex', justifyContent: 'center' }}>
      <Zoom in={true} style={{ transitionDelay: '150ms' }}>
        <MuiCard sx={authCardSx}>
          {/* Header */}
          <AuthHeader
            title="Welcome Back!"
            subtitle="Enter your credentials to access your account"
          />

          {/* Alerts */}
          {verifiedEmail &&
            <Alert severity="success" sx={{ mb: 2, borderRadius: '8px' }}>
              Email <b>{verifiedEmail}</b> has been verified. Now you can sign in!
            </Alert>
          }
          {registeredEmail &&
            <Alert severity="info" sx={{ mb: 2, borderRadius: '8px' }}>
              An activation code has been sent to <b>{registeredEmail}</b>.
            </Alert>
          }
          {isCheckoutRedirect &&
            <Alert severity="info" sx={{ mb: 2, borderRadius: '8px' }}>
              Vui lòng đăng nhập để tiếp tục thanh toán. Sản phẩm trong giỏ vẫn được giữ lại.
            </Alert>
          }

          {/* Form Fields */}
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {/* Email or Phone */}
            <Box>
              <TextField
                autoFocus
                fullWidth
                size="small"
                label="Email"
                placeholder="example@gmail.com or 09..."
                variant="outlined"
                sx={authInputSx}
                error={!!errors['email']}
                {...register('email', {
                  required: FIELD_REQUIRED_MESSAGE,
                  validate: (value) => {
                    if (!value) return FIELD_REQUIRED_MESSAGE
                    const isEmail = /^\S+@\S+\.\S+$/.test(value)
                    const isPhone = /^(0|\+84)[35789][0-9]{8}$/.test(value)
                    if (!isEmail && !isPhone) {
                      return 'Please enter a valid email or phone number.'
                    }
                    return true
                  }
                })}
              />
              <FieldErrorAlert errors={errors} fieldName={'email'} />
            </Box>

            {/* Password */}
            <Box>
              <TextField
                fullWidth
                size="small"
                label="Password"
                placeholder="••••••••"
                type={showPassword ? 'text' : 'password'}
                variant="outlined"
                sx={authInputSx}
                error={!!errors['password']}
                slotProps={{
                  input: {
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton
                          size="small"
                          onClick={() => setShowPassword(prev => !prev)}
                          edge="end"
                          tabIndex={-1}
                          sx={{ color: '#64748b' }}
                        >
                          {showPassword ? <VisibilityOffOutlinedIcon fontSize="small" /> : <VisibilityOutlinedIcon fontSize="small" />}
                        </IconButton>
                      </InputAdornment>
                    )
                  }
                }}
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        size="small"
                        onClick={() => setShowPassword(prev => !prev)}
                        edge="end"
                        tabIndex={-1}
                        sx={{ color: '#64748b' }}
                      >
                        {showPassword ? <VisibilityOffOutlinedIcon fontSize="small" /> : <VisibilityOutlinedIcon fontSize="small" />}
                      </IconButton>
                    </InputAdornment>
                  )
                }}
                {...register('password', {
                  required: FIELD_REQUIRED_MESSAGE
                })}
              />
              <FieldErrorAlert errors={errors} fieldName={'password'} />
            </Box>
          </Box>

          {/* Submit Button */}
          <Box sx={{ mt: 2.5 }}>
            <Button
              type="submit"
              variant="contained"
              size="large"
              fullWidth
              sx={authButtonSx}
            >
              Sign In
            </Button>
          </Box>

          {/* Footer */}
          <Box sx={{ mt: 2.5, textAlign: 'center', display: 'flex', gap: 0.5, justifyContent: 'center', alignItems: 'center' }}>
            <Typography variant="body2" sx={{ color: '#64748b', fontSize: '0.875rem' }}>
              Don&apos;t have an account?
            </Typography>
            <Link to="/register" style={{ textDecoration: 'none' }}>
              <Typography variant="body2" sx={{ color: 'secondary.main', fontWeight: 700, fontSize: '0.875rem', '&:hover': { textDecoration: 'underline' } }}>
                Create account
              </Typography>
            </Link>
          </Box>
        </MuiCard>
      </Zoom>
    </form>
  )
}

export default LoginForm
