import { useSelector } from 'react-redux'
import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { selectCurrentUser } from '~/store/user/userSlice'

export const PrivateRoute = ({ allowedRoles = null }) => {
  const currentUser = useSelector(selectCurrentUser)
  const location = useLocation()

  if (!currentUser) {
    return <Navigate to={`/login?redirect=${encodeURIComponent(location.pathname)}`} replace />
  }

  if (allowedRoles && allowedRoles.length > 0) {
    const userRoles = Array.isArray(currentUser.roles)
      ? currentUser.roles
      : currentUser.role
      ? [currentUser.role]
      : []

    const normalizedRoles = userRoles.map((r) => String(r).toLowerCase())
    const hasRole = allowedRoles.some((r) => normalizedRoles.includes(r.toLowerCase()))
    const isSeller = Boolean(currentUser.shopId || currentUser.user_shop || normalizedRoles.includes('seller'))

    if (!hasRole && !(allowedRoles.includes('Seller') && isSeller)) {
      return <Navigate to="/" replace />
    }
  }

  return <Outlet />
}

export default PrivateRoute
