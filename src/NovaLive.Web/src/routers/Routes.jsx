import { Routes, Route, Navigate } from 'react-router-dom'
import Auth from '~/features/Auth/pages/Auth'
import LiveFeedPage from '~/features/LiveSession/pages/LiveFeedPage'
import ShopLive from '~/features/Shop/pages/ShopLive'
import SellerPortal, { LegacySellerRedirect } from '~/features/Seller/SellerPortal'
import { HomePage } from '~/features/Home/pages/HomePage'
import ProductDetailPage from '~/features/Product/pages/ProductDetailPage'
import ProductsListPage from '~/features/Product/pages/ProductsListPage'
import CartPage from '~/features/Cart/pages/CartPage'
import OrderPage from '~/features/Order/pages/OrderPage'
import OrderHistoryPage from '~/features/Order/pages/OrderHistoryPage'
import ConsumerLayout from '~/common/components/layout/ConsumerLayout/ConsumerLayout'
import ProfilePage from '~/features/Profile/pages/ProfilePage'
import AdminLayout from '~/features/Admin/components/AdminLayout'
import AdminOverviewPage from '~/features/Admin/pages/AdminOverviewPage'
import AdminShopsPage from '~/features/Admin/pages/AdminShopsPage'
import AdminPayoutsPage from '~/features/Admin/pages/AdminPayoutsPage'
import AdminCategoriesPage from '~/features/Admin/pages/AdminCategoriesPage'
import AdminFlashSalesPage from '~/features/Admin/pages/AdminFlashSalesPage'
import AdminDiscountsPage from '~/features/Admin/pages/AdminDiscountsPage'
import AdminLivePage from '~/features/Admin/pages/AdminLivePage'
import AdminReviewsPage from '~/features/Admin/pages/AdminReviewsPage'
import PrivateRoute from './PrivateRoute'

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
          <Route path="/user/returns" element={<Navigate to="/orders?tab=returns" replace />} />
          <Route path="/returns" element={<Navigate to="/user/returns" replace />} />
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
      </Route>

      <Route path="/seller/*" element={<SellerPortal />} />
      <Route path="/dashboard/shop/*" element={<LegacySellerRedirect />} />
      <Route path="/products/form" element={<LegacySellerRedirect target="product-form" />} />
      <Route path="/products/form/:id" element={<LegacySellerRedirect target="product-form" />} />

      {/* ADMIN PLATFORM PROTECTED ROUTES */}
      <Route element={<PrivateRoute allowedRoles={['Admin']} />}>
        <Route element={<AdminLayout />}>
          <Route path="/admin" element={<Navigate to="/admin/overview" replace />} />
          <Route path="/admin/overview" element={<AdminOverviewPage />} />
          <Route path="/admin/shops" element={<AdminShopsPage />} />
          <Route path="/admin/payouts" element={<AdminPayoutsPage />} />
          <Route path="/admin/categories" element={<AdminCategoriesPage />} />
          <Route path="/admin/flash-sales" element={<AdminFlashSalesPage />} />
          <Route path="/admin/discounts" element={<AdminDiscountsPage />} />
          <Route path="/admin/live" element={<AdminLivePage />} />
          <Route path="/admin/reviews" element={<AdminReviewsPage />} />
        </Route>
      </Route>

      {/* 404 fallback */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default AppRoutes
