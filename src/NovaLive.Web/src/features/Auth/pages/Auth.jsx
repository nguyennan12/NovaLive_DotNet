import { useLocation } from 'react-router-dom'
import Box from '@mui/material/Box'
import LoginForm from '../components/LoginForm'
import RegisterForm from '../components/RegisterForm'
import { useSelector } from 'react-redux'
import { selectCurrentUser } from '~/store/user/userSlice'
import VerifyEmailForm from '../components/VerifyEmailForm'

function Auth() {
  const location = useLocation()
  const isLogin = location.pathname === '/login'
  const isRegister = location.pathname === '/register'
  const isverify = location.pathname === '/verify'

  useSelector(selectCurrentUser)

  // if (currentUser) {
  //   return <Navigate to='/' replace={true} />
  // }


  return (
    <Box sx={{
      minHeight: '100vh',
      width: '100%',
      bgcolor: '#ffffff',
      position: 'relative',
      overflow: 'hidden'
    }}>
      <Box sx={{
        display: 'flex',
        flexDirection: 'column',
        height: '100%',
        alignItems: 'center',
        justifyContent: 'center',
        position: 'absolute',
        zIndex: 0,
        inset: 0,
        backgroundImage: `
          radial-gradient(circle 600px at 0% 200px, #bfdbfe, transparent),
          radial-gradient(circle 600px at 100% 500px, #bfdbfe, transparent) `
      }}>
        {isLogin && <LoginForm />}
        {isRegister && <RegisterForm />}
        {isverify && <VerifyEmailForm />}
      </Box>
    </Box>

  )
}

export default Auth

