// Transport DTOs mirror NovaLive.Contracts.V1. View-only extensions live in viewModels.ts.
export interface PagedResult<T> {
  items: T[]
  page: number
  size: number
  total: number
  totalPages: number
  hasPreviousPage?: boolean
  hasNextPage?: boolean
}
export interface PageRequest { page: number; size: number }
export interface DateRange { from?: string; to?: string }
export type ShopStatus = 'Pending' | 'Active' | 'Suspended' | 'Banned' | 'Closed'
export interface Shop {
  id: string; ownerId: string; shopName: string; slug: string; description: string
  logoUrl: string | null; bannerUrl: string | null; phone: string; email: string
  status: ShopStatus; rating: number; followersCount: number; createdAt: string
  verificationStatus?: string | null; reviewerNote?: string | null; bankAccount?: string | null
}
export type ShopProfileRequest = Pick<Shop, 'shopName' | 'description' | 'logoUrl' | 'bannerUrl' | 'phone' | 'email'>
export interface Warehouse {
  id: string; warehouseName: string; contactName: string; contactPhone: string
  provinceName: string; districtName: string; wardName: string; detailAddress: string
  isPrimary: boolean; isReturn: boolean; provinceId: number | null; districtId: number | null; wardCode: string | null
}
export type WarehouseRequest = Omit<Warehouse, 'id'>
export interface Category { id: string; name: string; slug: string; parentId: string | null; children: Category[] }
export type ProductStatus = 'Draft' | 'Active' | 'Inactive' | 'Banned'
export interface ProductAttribute { name: string; value: string }
export interface Sku {
  id: string; spuId: string; skuCode: string; attributesJson: string; originalPrice: number
  sellPrice: number; weightGram: number; qtyOnHand: number; reservedQty: number
  availableQty: number; isActive: boolean; images: string[]
}
export interface Product {
  id: string; shopId: string; shopName: string; categoryId: string; categoryName: string
  name: string; slug: string; description: string; brand: string | null; thumbnailUrl: string
  minPrice: number; maxPrice: number; rating: number; soldCount: number; status: ProductStatus; createdAt: string
}
export interface ProductDetail extends Product { attributesConfigJson: string | null; skus: Sku[]; attributes: ProductAttribute[] }
export interface CreateSkuRequest {
  skuCode: string; attributesJson: string; originalPrice: number; sellPrice: number
  weightGram: number; initialStock: number; images: string[]
}
export interface CreateProductRequest {
  name: string; description: string; categoryId: string; brand: string | null; thumbnailUrl: string
  attributesConfigJson: string | null; skus: CreateSkuRequest[]; attributes: ProductAttribute[]
}
export type UpdateProductRequest = Omit<CreateProductRequest, 'skus'>
export type UpdateSkuRequest = Pick<Sku, 'sellPrice' | 'originalPrice' | 'weightGram' | 'isActive'>
export interface ProductFilters extends PageRequest { keyword?: string; categoryId?: string }
export interface InventoryItem {
  skuId: string; skuCode: string; spuName: string; qtyOnHand: number; reservedQty: number
  availableQty: number; minStock: number; lastUpdated: string
}
export interface InventoryHistory {
  id: string; skuId: string; skuCode: string; spuName: string; changeType: string
  qtyBefore: number; qtyChange: number; reservedBefore: number; reservedChange: number; qtyAfter: number
  refType: string | null; refId: string | null; note: string | null; createdBy: string | null; createdAt: string
}
export interface AdjustInventoryRequest { skuId: string; qtyChange: number; changeType: 'Import' | 'ManualAdjust'; note: string; operationId: string }
export type OrderStatus = 'PendingPayment' | 'Confirmed' | 'Processing' | 'Shipping' | 'Delivered' | 'Completed' | 'Cancelled' | 'ReturnRequested' | 'Returned' | 'Refunded'
export interface OrderSummary { id: string; parentOrderId: string; code: string; shopId: string; subTotal: number; status: OrderStatus; createdAt: string }
export interface OrderItem { id: string; skuId: string; skuSnapshotJson: string; quantity: number; unitPrice: number; originalPrice: number; discountAmount: number; lineTotal: number }
export interface OrderTimeline { fromStatus: string | null; toStatus: string; changedByRole: string | null; note: string | null; changedAt: string }
export interface OrderRecipient {
  recipientName: string; phone: string; provinceId: number | null; provinceName: string
  districtId: number | null; districtName: string; wardCode: string | null; wardName: string; detailAddress: string
}
export interface OrderDetail extends OrderSummary {
  warehouseAddressId: string | null; orderSource: string; recipient: OrderRecipient
  itemAmount: number; shippingFee: number; shopDiscountAmount: number; platformDiscountAmount: number
  shippingDiscountAmount: number; cancelReason: string | null; paymentStatus: string; paymentMethod: string
  items: OrderItem[]; timeline: OrderTimeline[]
}
export interface OrderFilters extends PageRequest, DateRange { status?: string }
export type ShippingProvider = 'GHN' | 'GHTK' | 'ViettelPost'
export interface CreateShippingRequest { subOrderId: string; provider: ShippingProvider; serviceCode: string; pickupAddressId: string }
export interface ShippingOrder { id: string; subOrderId: string; provider: string; trackingCode: string; shippingFee: number; status: string; createdAt: string }
export interface TrackingEvent { status: string; description: string | null; occurredAt: string }
export interface Tracking { subOrderId: string; provider: string; trackingCode: string; status: string; events: TrackingEvent[]; isStale: boolean }
export type ReturnStatus = 'Pending' | 'SellerApproved' | 'SellerRejected' | 'AdminDispute' | 'AdminApproved' | 'AdminRejected' | 'Completed'
export type ReturnReason = 'WrongItem' | 'Defective' | 'DamagedInShipping' | 'NotAsDescribed' | 'ChangeOfMind'
export interface ReturnItem { orderItemId: string; spuName: string; skuCode: string; quantity: number; refundPrice: number }
export interface OrderReturn {
  id: string; subOrderId: string; subOrderCode: string; buyerId: string; reason: ReturnReason
  status: ReturnStatus; requestedRefundAmount: number; approvedRefundAmount: number | null
  createdAt: string; items: ReturnItem[]; evidenceUrls: string[]; returnAddress: Warehouse | null
  sellerResponse: string | null; sellerEvidenceUrls: string[] | null
}
export type DiscountType = 'PercentCart' | 'FixedCart'
export interface Discount {
  id: string; shopId: string | null; code: string; name: string; discountType: DiscountType
  discountValue: number; minOrderAmount: number; maxDiscountAmount: number | null
  maxUses: number | null; usedCount: number; perUserLimit: number; appliesTo: string; targetIds: string[] | null
  validFrom: string; validTo: string; isPublic: boolean; isActive: boolean; createdAt: string
}
export type CreateDiscountRequest = Omit<Discount, 'id' | 'shopId' | 'usedCount' | 'isActive' | 'createdAt'>
export type UpdateDiscountRequest = Pick<Discount, 'name' | 'maxUses' | 'validTo' | 'isActive'>
export interface Campaign { id: string; name: string; startAt: string; endAt: string; bannerUrl: string | null; status: string }
export interface FlashRegistration {
  id: string; campaignId: string; skuId: string; spuId: string; spuName: string; skuCode: string
  thumbnailUrl: string; originalPrice: number; flashPrice: number; quantity: number
  soldQty: number; reservedQty: number; perUserLimit: number; status: 'Pending' | 'Approved' | 'Rejected' | 'Ended'; rejectionReason: string | null
}
export interface FlashRegistrationRequest { skuId: string; flashPrice: number; quantity: number; perUserLimit: number }
export interface Review {
  id: string; buyerId: string; buyerName: string; buyerAvatarUrl: string | null; spuId: string; skuId: string
  skuAttributesJson: string; rating: number; title: string | null; content: string | null
  sellerReply: string | null; repliedAt: string | null; createdAt: string; mediaUrls: string[]
}
export interface Wallet { shopId: string; balance: number; lockedBalance: number; updatedAt: string }
export type WalletTransactionType = 'OrderRevenue' | 'OrderRefund' | 'CodCommissionDeduct' | 'PayoutLock' | 'PayoutWithdrawal' | 'PayoutFailedUnlock' | 'PenaltyDeduct' | 'ManualAdjustment'
export interface WalletTransaction { id: string; walletId: string; amount: number; type: WalletTransactionType; description: string; balanceAfter: number; createdAt: string }
export interface Payout { id: string; shopId: string; amount: number; bankName: string; bankAccount: string; bankHolder: string; status: string; transferRef: string | null; reason: string | null; createdAt: string }
export interface UploadResponse { url: string; fileName?: string; bucket?: string; size?: number }
