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
import VNPayReturnPage from '~/features/Order/pages/VNPayReturnPage'
import ConsumerLayout from '~/common/components/layout/ConsumerLayout/ConsumerLayout'
import ProfilePage from '~/features/Profile/pages/ProfilePage'

const AppRoutes = () => {
  return (
    <Routes>
      {/* Consumer routes — có AppBar + BottomActionBar */}
      <Route element={<ConsumerLayout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/product/:productId" element={<ProductDetailPage />} />
        <Route path="/products" element={<ProductsListPage />} />
        <Route path="/cart" element={<CartPage />} />
        <Route path="/order" element={<OrderPage />} />
        <Route path="/orders" element={<OrderHistoryPage />} />
        <Route path="/profile" element={<ProfilePage />} />
      </Route>

      {/* AUTH */}
      <Route path='/login' element={<Auth />} />
      <Route path='/register' element={<Auth />} />
      <Route path='/verify' element={<Auth />} />

      {/* LIVE feed (standalone, không cần consumer layout) */}
      <Route path='/live' element={<LiveFeedPage />} />
      <Route path='/shop/live/:liveId' element={<ShopLive />} />

      {/* ORDER / PAYMENT */}
      <Route path="/vnpay-return" element={<VNPayReturnPage />} />


      {/* PRODUCT forms */}
      <Route path="/products/form" element={<ProductFormPage />} />
      <Route path="/products/form/:id" element={<ProductFormPage />} />
      <Route path='/manager/inventory' element={<InventoryPage />} />
      <Route path='/manager/discount' element={<DiscountPage />} />

      {/* SHOP DASHBOARD */}
      <Route path='/dashboard/shop' element={<DashboardShop />}>
        <Route index element={<Box>dashboard</Box>} />
        <Route path="products" element={<ProductsPage />} />
        <Route path="orders" element={<Box>Orders</Box>} />
        <Route path="customers" element={<Box>customers</Box>} />
        <Route path="live" element={<LiveManagerPage />} />
        <Route path="analytics" element={<Box>analytics</Box>} />
        <Route path="finance" element={<Box>finance</Box>} />
      </Route>
    </Routes>
  )
}

export default AppRoutes
