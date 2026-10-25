import { useSelector } from 'react-redux'
import { Navigate, useLocation } from 'react-router-dom'
import type { ReactNode } from 'react'
import { Alert, Button, Box } from '@mui/material'
import type { SellerRootState } from '../../types/viewModels'
import { hasSellerRole } from '../../utils/business'

export function SellerGuard({ children }: { children: ReactNode }) {
  const currentUser = useSelector((state: SellerRootState) => state.user.currentUser)
  const location = useLocation()
  if (!currentUser) return <Navigate to={`/login?redirect=${encodeURIComponent(location.pathname + location.search)}`} replace />
  if (!hasSellerRole(currentUser.roles, currentUser.role)) return <Box sx={{ p: 4 }}><Alert severity='warning'>Khu vực này yêu cầu tài khoản có vai trò Seller.</Alert><Button href='/'>Về trang chủ</Button></Box>
  return children
}
