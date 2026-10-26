export const SELLER_BASE = '/seller'
export const PAGE_SIZE = 10
export const MIN_PAYOUT = 50_000
export const SELLER_TIMEZONE = 'Asia/Ho_Chi_Minh'
export const RETURN_RESPONSE_MS = 3 * 24 * 60 * 60 * 1000
export const FLASH_MAX_PRICE_RATIO = 0.9
export const REVIEW_PAGE_SIZE = 50
export const REVIEW_REPLY_MAX = 1000
export const MOCK_ONLY = import.meta.env.VITE_SELLER_API_MODE === 'mock'
export const MOCK_FALLBACK = import.meta.env.VITE_USE_MOCK_SELLER_API === 'true'
export const MOCK_SCENARIO = import.meta.env.VITE_SELLER_MOCK_SCENARIO || 'success'
export const SELLER_QUERY_KEY = ['seller'] as const
export const ORDER_STATUSES = ['PendingPayment', 'Confirmed', 'Processing', 'Shipping', 'Delivered', 'Completed', 'ReturnRequested', 'Returned', 'Refunded', 'Cancelled'] as const
export const RETURN_STATUSES = ['Pending', 'SellerApproved', 'SellerRejected', 'AdminDispute', 'AdminApproved', 'AdminRejected', 'Completed'] as const
export const WALLET_TYPES = ['OrderRevenue', 'OrderRefund', 'CodCommissionDeduct', 'PayoutLock', 'PayoutWithdrawal', 'PayoutFailedUnlock', 'PenaltyDeduct', 'ManualAdjustment'] as const
