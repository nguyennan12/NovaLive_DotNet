import { useSelector } from 'react-redux'
import { Link, Navigate, useLocation } from 'react-router-dom'
import type { ReactNode } from 'react'
import { Alert, Button, Box, Stack } from '@mui/material'
import type { SellerRootState } from '../../types/viewModels'
import { hasSellerRole } from '../../utils/business'

export function SellerGuard({ children }: { children: ReactNode }) {
  const currentUser = useSelector((state: SellerRootState) => state.user.currentUser)
  const location = useLocation()
  if (!currentUser) return <Navigate to={`/login?redirect=${encodeURIComponent(location.pathname + location.search)}`} replace />
  if (!hasSellerRole(currentUser.roles, currentUser.role)) {
    return (
      <Box sx={{ p: 4, maxWidth: 640, mx: 'auto', mt: 6 }}>
        <Alert severity='warning' sx={{ mb: 2 }}>
          Khu vực này yêu cầu tài khoản có vai trò Seller. Nếu bạn chưa mở shop, vui lòng đăng ký để bắt đầu bán hàng.
        </Alert>
        <Stack direction='row' spacing={2}>
          <Button variant='contained' component={Link} to='/profile'>
            Đăng ký mở shop
          </Button>
          <Button variant='outlined' component={Link} to='/'>
            Về trang chủ
          </Button>
        </Stack>
      </Box>
    )
  }
  return children
}
