import type { Review } from './contracts'

// Provisional contracts for endpoints absent from the checked-in backend.
// Keep separate from existing V1 DTOs; coordinate these fields with backend T50/T46.
export interface SellerDashboard {
  totalSalesRevenue: number; totalOrdersCount: number; walletAvailableBalance: number; pendingOrdersCount: number
  dailyRevenue: { date: string; revenue: number; ordersCount: number }[]
  orderStatuses: { status: string; count: number }[]
  topSkus: { skuId: string; spuName: string; skuCode: string; soldQuantity: number; totalRevenue: number }[]
  recentLives: { sessionId: string; title: string; peakViewers: number; totalOrdersCount: number; totalRevenue: number; date: string }[]
  actions: { pendingOrders: number; expiringReturns: number; lowStockSkus: number }
}
export interface SellerNotification { id: string; title: string; content: string; isRead: boolean; createdAt: string; link: string | null }
export interface SellerLive { id: string; title: string; status: string; peakViewers: number; totalRevenue: number; totalOrdersCount: number; startedAt: string | null; endedAt: string | null }
export interface SellerReview extends Review { productName: string; isHidden?: boolean }
export interface BankDetails { bankAccount: string | null; bankName: string | null; bankHolder: string | null; bankBranch: string | null }
export interface SellerUser { id?: string; _id?: string; roles?: string[]; role?: string; accessToken?: string; fullName?: string; user_name?: string }
export interface SellerRootState { user: { currentUser: SellerUser | null } }
export interface OrderPlacedEvent { subOrderId: string; itemCount: number; totalAmount: number }
export interface Province { ProvinceID: number; ProvinceName: string }
export interface District { DistrictID: number; DistrictName: string }
export interface Ward { WardCode: string; WardName: string }
