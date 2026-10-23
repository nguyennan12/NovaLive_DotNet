import { Box } from '@mui/material'
import { Routes, Route } from 'react-router-dom'
import Auth from '~/features/Auth/pages/Auth'
import LiveFeedPage from '~/features/LiveSession/pages/LiveFeedPage'
import ShopLive from '~/features/Shop/pages/ShopLive'
import ProductFormPage from '~/features/Product/pages/ProductFormPage'
import DashboardShop from '~/common/components/layout/DashBoard/DashBoardShop'
import ProductsPage from '~/features/Product/pages/ProductPage'
import InventoryPage from '~/features/Inventory/pages/InventoryPage'
import { DiscountPage } from '~/features/Discount/pages/DiscountPage'
import LiveManagerPage from '~/features/LiveSession/pages/LiveManagerPage'
import { HomePage } from '~/features/Home/pages/HomePage'
import ProductDetailPage from '~/features/Product/pages/ProductDetailPage'
import ProductsListPage from '~/features/Product/pages/ProductsListPage'
import CartPage from '~/features/Cart/pages/CartPage'
import OrderPage from '~/features/Order/pages/OrderPage'
import OrderHistoryPage from '~/features/Order/pages/OrderHistoryPage'
import ConsumerLayout from '~/common/components/layout/ConsumerLayout/ConsumerLayout'
import ProfilePage from '~/features/Profile/pages/ProfilePage'
import AdminLayout from '~/features/Admin/components/AdminLayout'
import AdminDashboardPage from '~/features/Admin/pages/AdminDashboardPage'
import PrivateRoute from './PrivateRoute'
import { Navigate } from 'react-router-dom'

const AppRoutes = () => {
  return (
    <Routes>
      {/* Consumer routes — có AppBar + BottomActionBar */}
      <Route element={<ConsumerLayout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/product/:productId" element={<ProductDetailPage />} />
        <Route path="/products" element={<ProductsListPage />} />
        <Route path="/cart" element={<CartPage />} />

        {/* Protected User routes */}
        <Route element={<PrivateRoute />}>
          <Route path="/order" element={<OrderPage />} />
          <Route path="/orders" element={<OrderHistoryPage />} />
          <Route path="/profile" element={<ProfilePage />} />
        </Route>
      </Route>

      {/* AUTH */}
      <Route path="/login" element={<Auth />} />
      <Route path="/register" element={<Auth />} />
      <Route path="/verify" element={<Auth />} />

      {/* LIVE feed (public viewer) */}
      <Route path="/live" element={<LiveFeedPage />} />

      {/* SELLER ONLY PROTECTED ROUTES */}
      <Route element={<PrivateRoute allowedRoles={['Seller', 'Admin']} />}>
        {/* Host Live session */}
        <Route path="/shop/live/:liveId" element={<ShopLive />} />

        {/* Product & Warehouse manager forms */}
        <Route path="/products/form" element={<ProductFormPage />} />
        <Route path="/products/form/:id" element={<ProductFormPage />} />
        <Route path="/manager/inventory" element={<InventoryPage />} />
        <Route path="/manager/discount" element={<DiscountPage />} />

        {/* SHOP DASHBOARD */}
        <Route path="/dashboard/shop" element={<DashboardShop />}>
          <Route index element={<Box sx={{ p: 2 }}>Dashboard Overview</Box>} />
          <Route path="products" element={<ProductsPage />} />
          <Route path="orders" element={<Box sx={{ p: 2 }}>Orders Management</Box>} />
          <Route path="customers" element={<Box sx={{ p: 2 }}>Customers</Box>} />
          <Route path="live" element={<LiveManagerPage />} />
          <Route path="analytics" element={<Box sx={{ p: 2 }}>Analytics</Box>} />
          <Route path="finance" element={<Box sx={{ p: 2 }}>Finance</Box>} />
        </Route>
      </Route>

      {/* PLATFORM ADMIN DASHBOARD */}
      <Route path="/admin" element={<AdminLayout />}>
        <Route index element={<Navigate to="/admin/dashboard" replace />} />
        <Route path="dashboard" element={<AdminDashboardPage />} />
        <Route path="livestreams" element={<AdminDashboardPage />} />
        <Route path="disputes" element={<AdminDashboardPage />} />
        <Route path="shops" element={<AdminDashboardPage />} />
        <Route path="reports" element={<AdminDashboardPage />} />
        <Route path="categories" element={<AdminDashboardPage />} />
        <Route path="flash-sales" element={<AdminDashboardPage />} />
        <Route path="payouts" element={<AdminDashboardPage />} />
        <Route path="reviews" element={<AdminDashboardPage />} />
        <Route path="users" element={<AdminDashboardPage />} />
      </Route>
    </Routes>
  )
}

export default AppRoutes
