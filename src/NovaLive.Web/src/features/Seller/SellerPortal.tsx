import { lazy, Suspense } from 'react'
import { Alert, Button } from '@mui/material'
import { Link, Navigate, Route, Routes, useLocation, useParams } from 'react-router-dom'
import { SellerGuard } from './components/layout/SellerGuard'
import { SellerLayout } from './components/layout/SellerLayout'
import { QueryState } from './components/shared/QueryState'
import { legacySellerPaths } from './config/navigation'
import { SellerErrorBoundary } from './components/layout/SellerErrorBoundary'

const DashboardPage = lazy(() => import('./pages/DashboardPage'))
const ProfilePage = lazy(() => import('./pages/ProfilePage'))
const NotificationsPage = lazy(() => import('./pages/NotificationsPage'))
// Keep the seller portal routes while reusing the richer pages from the original seller UI.
const ProductsPage = lazy(() => import('~/features/Product/pages/ProductPage'))
const ProductFormPage = lazy(() => import('./pages/ProductFormPage'))
const InventoryPage = lazy(() => import('~/features/Inventory/pages/InventoryPage'))
const OrdersPage = lazy(() => import('./pages/OrdersPage'))
const OrderDetailPage = lazy(() => import('./pages/OrderDetailPage'))
const ReturnsPage = lazy(() => import('./pages/ReturnsPage'))
const CustomersPage = lazy(() => import('./pages/CustomersPage'))
const DiscountsPage = lazy(() => import('~/features/Discount/pages/DiscountPage').then(module => ({ default: module.DiscountPage })))
const FlashSalesPage = lazy(() => import('./pages/FlashSalesPage'))
const ReviewsPage = lazy(() => import('./pages/ReviewsPage'))
const WalletPage = lazy(() => import('./pages/WalletPage'))
const LivePage = lazy(() => import('~/features/LiveSession/pages/LiveManagerPage'))
const MarketingPage = lazy(() => import('./pages/MarketingPage'))

export default function SellerPortal() {
  const location = useLocation()
  return <SellerGuard><SellerLayout><SellerErrorBoundary key={location.pathname}><Suspense fallback={<QueryState loading error={null} onRetry={() => window.location.reload()}>{null}</QueryState>}><Routes>
    <Route index element={<DashboardPage />} />
    <Route path='analytics' element={<DashboardPage />} />
    <Route path='profile' element={<ProfilePage />} />
    <Route path='notifications' element={<NotificationsPage />} />
    <Route path='products' element={<ProductsPage />} />
    <Route path='products/new' element={<ProductFormPage />} />
    <Route path='products/:id/edit' element={<ProductFormPage />} />
    <Route path='inventory' element={<InventoryPage />} />
    <Route path='orders' element={<OrdersPage />} />
    <Route path='orders/:id' element={<OrderDetailPage />} />
    <Route path='shipping' element={<OrdersPage shipping />} />
    <Route path='returns' element={<ReturnsPage />} />
    <Route path='customers' element={<CustomersPage />} />
    <Route path='discounts' element={<DiscountsPage />} />
    <Route path='flash-sales' element={<FlashSalesPage />} />
    <Route path='reviews' element={<ReviewsPage />} />
    <Route path='wallet' element={<WalletPage />} />
    <Route path='live' element={<LivePage />} />
    <Route path='marketing' element={<MarketingPage />} />
    {Object.entries(legacySellerPaths).map(([oldPath, newPath]) => <Route key={oldPath} path={oldPath} element={<Navigate to={`/seller/${newPath}`} replace />} />)}
    <Route path='*' element={<Alert severity='info' action={<Button component={Link} to='/seller'>Về tổng quan</Button>}>Không tìm thấy trang quản lý cửa hàng.</Alert>} />
  </Routes></Suspense></SellerErrorBoundary></SellerLayout></SellerGuard>
}
export function LegacySellerRedirect({ target }: { target?: string }) {
  const location = useLocation(); const { id } = useParams()
  const suffix = location.pathname.replace(/^\/dashboard\/shop\/?/, '')
  const [first, ...rest] = suffix.split('/')
  const mapped = first ? [legacySellerPaths[first] || first, ...rest].join('/') : ''
  const path = target === 'product-form' ? id ? `products/${id}/edit` : 'products/new' : target ?? mapped
  return <Navigate to={`/seller${path ? `/${path}` : ''}${location.search}${location.hash}`} replace />
}
