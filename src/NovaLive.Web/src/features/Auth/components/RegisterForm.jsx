import { useState } from 'react'
import { Card as MuiCard } from '@mui/material'
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
import { Link, useNavigate } from 'react-router-dom'
import { toast } from 'react-toastify'
import { registerUserAPI } from '~/common/apis/services/userService'
import FieldErrorAlert from '~/common/components/common/form/FieldErrorAlert'
import AuthHeader from './AuthHeader'
import { authCardSx, authInputSx, authButtonSx } from '../authStyles'
import {
  EMAIL_RULE,
  EMAIL_RULE_MESSAGE,
  PHONE_RULE,
  PHONE_RULE_MESSAGE,
  FIELD_REQUIRED_MESSAGE,
  PASSWORD_RULE,
  PASSWORD_RULE_MESSAGE
} from '~/common/utils/validators'

function RegisterForm() {
  const { register, handleSubmit, getValues, formState: { errors } } = useForm()
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)
  const navigate = useNavigate()

  const onSubmitRegister = (data) => {
    const { fullName, phone, email, password } = data
    toast.promise(registerUserAPI({ fullName, phone, email, password }),
      { pending: 'Registration is in progress...' }
    ).then((response) => {
      const emailTarget = response?.email || response?.user?.user_email || email
      const userId = response?.userId || response?.id || ''
      navigate(`/verify?registeredEmail=${encodeURIComponent(emailTarget)}&userId=${encodeURIComponent(userId)}`)
    })
  }

  return (
    <form onSubmit={handleSubmit(onSubmitRegister)} style={{ width: '100%', display: 'flex', justifyContent: 'center' }}>
      <Zoom in={true} style={{ transitionDelay: '150ms' }}>
        <MuiCard sx={authCardSx}>
          {/* Header */}
          <AuthHeader
            title="Create an Account"
            subtitle="Sign up to start shopping & streaming on NovaLive"
          />

          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {/* Full Name */}
            <Box>
              <TextField
                autoFocus
                fullWidth
                size="small"
                label="Full Name"
                placeholder="Nguyen Van A"
                variant="outlined"
                sx={authInputSx}
                error={!!errors['fullName']}
                {...register('fullName', {
                  required: FIELD_REQUIRED_MESSAGE,
                  minLength: { value: 2, message: 'Name must be at least 2 characters.' },
                  maxLength: { value: 200, message: 'Name cannot exceed 200 characters.' }
                })}
              />
              <FieldErrorAlert errors={errors} fieldName={'fullName'} />
            </Box>

            {/* Phone Number */}
            <Box>
              <TextField
                fullWidth
                size="small"
                label="Phone Number"
                placeholder="0912345678"
                variant="outlined"
                sx={authInputSx}
                error={!!errors['phone']}
                {...register('phone', {
                  required: FIELD_REQUIRED_MESSAGE,
                  pattern: {
                    value: PHONE_RULE,
                    message: PHONE_RULE_MESSAGE
                  }
                })}
              />
              <FieldErrorAlert errors={errors} fieldName={'phone'} />
            </Box>

            {/* Email */}
            <Box>
              <TextField
                fullWidth
                size="small"
                label="Email"
                placeholder="example@gmail.com"
                type="text"
                variant="outlined"
                sx={authInputSx}
                error={!!errors['email']}
                {...register('email', {
                  required: FIELD_REQUIRED_MESSAGE,
                  pattern: {
                    value: EMAIL_RULE,
                    message: EMAIL_RULE_MESSAGE
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
                {...register('password', {
                  required: FIELD_REQUIRED_MESSAGE,
                  pattern: {
                    value: PASSWORD_RULE,
                    message: PASSWORD_RULE_MESSAGE
                  }
                })}
              />
              <FieldErrorAlert errors={errors} fieldName={'password'} />
            </Box>

            {/* Password Confirmation */}
            <Box>
              <TextField
                fullWidth
                size="small"
                label="Confirm Password"
                placeholder="••••••••"
                type={showConfirmPassword ? 'text' : 'password'}
                variant="outlined"
                sx={authInputSx}
                error={!!errors['password_confirmation']}
                slotProps={{
                  input: {
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton
                          size="small"
                          onClick={() => setShowConfirmPassword(prev => !prev)}
                          edge="end"
                          tabIndex={-1}
                          sx={{ color: '#64748b' }}
                        >
                          {showConfirmPassword ? <VisibilityOffOutlinedIcon fontSize="small" /> : <VisibilityOutlinedIcon fontSize="small" />}
                        </IconButton>
                      </InputAdornment>
                    )
                  }
                }}
                {...register('password_confirmation', {
                  required: FIELD_REQUIRED_MESSAGE,
                  validate: (value) => {
                    if (value === getValues('password')) return true
                    return 'Password Confirmation does not match!'
                  }
                })}
              />
              <FieldErrorAlert errors={errors} fieldName={'password_confirmation'} />
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
              Create Account
            </Button>
          </Box>

          {/* Footer */}
          <Box sx={{ mt: 2.5, textAlign: 'center', display: 'flex', gap: 0.5, justifyContent: 'center', alignItems: 'center' }}>
            <Typography variant="body2" sx={{ color: '#64748b', fontSize: '0.875rem' }}>
              Already have an account?
            </Typography>
            <Link to="/login" style={{ textDecoration: 'none' }}>
              <Typography variant="body2" sx={{ color: 'secondary.main', fontWeight: 700, fontSize: '0.875rem', '&:hover': { textDecoration: 'underline' } }}>
                Sign in
              </Typography>
            </Link>
          </Box>
        </MuiCard>
      </Zoom>
    </form>
  )
}

export default RegisterForm