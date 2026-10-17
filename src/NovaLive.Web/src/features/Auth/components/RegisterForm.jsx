import { Card as MuiCard } from '@mui/material'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import CardActions from '@mui/material/CardActions'
import TextField from '@mui/material/TextField'
import Typography from '@mui/material/Typography'
import Zoom from '@mui/material/Zoom'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { toast } from 'react-toastify'
import { registerUserAPI } from '~/common/apis/services/userService'
import FieldErrorAlert from '~/common/components/common/form/FieldErrorAlert'
import {
  EMAIL_RULE,
  EMAIL_RULE_MESSAGE,
  FIELD_REQUIRED_MESSAGE,
  PASSWORD_RULE,
  PASSWORD_RULE_MESSAGE
} from '~/common/utils/validators'

function RegisterForm() {
  const { register, handleSubmit, getValues, formState: { errors } } = useForm()
  const navigate = useNavigate()

  const onSubmitRegister = (data) => {
    const { email, password } = data
    toast.promise(registerUserAPI({ email, password }),
      { pending: 'Registration is in progress...' }
    ).then((response) => {
      const emailTarget = response?.email || response?.user?.user_email || email
      const userId = response?.userId || response?.id || ''
      navigate(`/verify?registeredEmail=${encodeURIComponent(emailTarget)}&userId=${encodeURIComponent(userId)}`)
    })
  }

  return (
    <form onSubmit={handleSubmit(onSubmitRegister)}>
      <Zoom in={true} style={{ transitionDelay: '200ms' }}>
        <MuiCard sx={{
          minWidth: '380px',
          maxWidth: '380px',
          borderRadius: '12px',
          boxShadow: '0 0 3px 0.5px rgba(0,0,0,0.15)'
        }}>
          <Box sx={{ margin: '1em', display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
            <Typography variant='h6' sx={{ fontWeight: '600' }}>Welcome Back!</Typography>
            <Typography variant='b2' sx={{ color: '#838383' }}>Enter your credentials to access your account</Typography>
          </Box>


          <Box sx={{ padding: '0 1em 1em 1em' }}>
            <Box sx={{ marginTop: '1em' }}>
              <TextField
                autoFocus
                fullWidth
                label="Enter Email..."
                type="text"
                variant="outlined"
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
            <Box sx={{ marginTop: '1em' }}>
              <TextField
                autoFocus
                fullWidth
                label="Enter Password..."
                type="text"
                variant="outlined"
                error={!!errors['password']}
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
            <Box sx={{ marginTop: '1em' }}>
              <TextField
                autoFocus
                fullWidth
                label="Enter Password Cofirmation..."
                type="text"
                variant="outlined"
                error={!!errors['password_cofimation']}
                {...register('password_cofimation', {
                  validate: (value) => {
                    if (value === getValues('password')) return true
                    return 'Password Confirmation does not match!'
                  }
                })}
              />
              <FieldErrorAlert errors={errors} fieldName={'password_cofimation'} />
            </Box>
          </Box>
          <CardActions sx={{ padding: '0 1em 1em 1em' }}>
            <Button
              type="submit"
              variant="contained"
              size="large"
              fullWidth
              sx={{
                background:
                  'linear-gradient(90deg, #3465c8, #69aedc, #8acdde)',
                color: '#ffffff'
              }}

            >
              Register
            </Button>
          </CardActions>
          <Box sx={{ padding: '0 1em 1em 1em', textAlign: 'center', display: 'flex', gap: 1, justifyContent: 'center' }}>
            <Typography>Already have an account?</Typography>
            <Link to="/login" style={{ textDecoration: 'none' }}>
              <Typography sx={{ color: 'secondary.main' }}>Sign in</Typography>
            </Link>
          </Box>
        </MuiCard>
      </Zoom>
    </form >
  )
}

export default RegisterForm