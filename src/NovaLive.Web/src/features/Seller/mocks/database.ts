import { store } from '~/store/store'
import type { Campaign, Category, Discount, FlashRegistration, InventoryHistory, InventoryItem, OrderDetail, OrderReturn, PagedResult, PageRequest, ProductDetail, Shop, Wallet, WalletTransaction, Warehouse } from '../types/contracts'
import type { SellerDashboard, SellerLive, SellerNotification, SellerReview, SellerRootState } from '../types/viewModels'
import { MOCK_SCENARIO, ORDER_STATUSES, RETURN_STATUSES, WALLET_TYPES } from '../constants'

export interface MockDatabase {
  shop: Shop; warehouses: Warehouse[]; categories: Category[]; products: ProductDetail[]
  inventory: InventoryItem[]; histories: InventoryHistory[]; orders: OrderDetail[]; returns: OrderReturn[]
  discounts: Discount[]; campaigns: Campaign[]; registrations: FlashRegistration[]; reviews: SellerReview[]
  wallet: Wallet; transactions: WalletTransaction[]; notifications: SellerNotification[]; lives: SellerLive[]
}
const iso = (hours = 0) => new Date(Date.now() + hours * 3_600_000).toISOString()
const id = (n: number) => `10000000-0000-4000-8000-${String(n).padStart(12, '0')}`
const names = ['Áo thun cotton Nova', 'Túi đeo chéo vải canvas', 'Bình giữ nhiệt 500ml', 'Tai nghe không dây', 'Giày thể thao nhẹ', 'Áo khoác chống nắng', 'Quần jeans ống đứng', 'Đèn bàn học LED', 'Balo đi học chống nước', 'Cốc sứ thủ công', 'Bộ chăn ga cotton', 'Mũ lưỡi trai']
function seed(): MockDatabase {
  const warehouses: Warehouse[] = [
    { id: id(1), warehouseName: 'Kho Thủ Đức', contactName: 'Nguyễn Quốc Đạt', contactPhone: '0901234567', provinceName: 'Hồ Chí Minh', districtName: 'Thủ Đức', wardName: 'Linh Trung', detailAddress: '12 Đường số 5', isPrimary: true, isReturn: true, provinceId: 202, districtId: 3695, wardCode: '90768' },
    { id: id(2), warehouseName: 'Kho Hà Nội', contactName: 'Trần Minh Anh', contactPhone: '0912345678', provinceName: 'Hà Nội', districtName: 'Cầu Giấy', wardName: 'Dịch Vọng', detailAddress: '35 Trần Thái Tông', isPrimary: false, isReturn: false, provinceId: 201, districtId: 1485, wardCode: '1A0601' }
  ]
  const categories: Category[] = [
    { id: id(10), name: 'Thời trang', slug: 'thoi-trang', parentId: null, children: [{ id: id(11), name: 'Áo & phụ kiện', slug: 'ao-phu-kien', parentId: id(10), children: [] }] },
    { id: id(12), name: 'Nhà cửa & đời sống', slug: 'nha-cua', parentId: null, children: [] },
    { id: id(13), name: 'Điện tử', slug: 'dien-tu', parentId: null, children: [] }
  ]
  const products: ProductDetail[] = names.map((name, i) => {
    const productId = id(100 + i)
    const price = 129_000 + i * 35_000
    const skus = ['Trắng', 'Đen'].map((color, j) => ({
      id: id(200 + i * 2 + j), spuId: productId, skuCode: `NOVA-${String(i + 1).padStart(3, '0')}-${j + 1}`, attributesJson: JSON.stringify({ Màu: color }), originalPrice: price + 50_000, sellPrice: price,
      weightGram: 300 + i * 50, qtyOnHand: i % 4 === 0 ? 5 + j : 45 + i, reservedQty: i % 4 === 0 ? 3 : 4, availableQty: i % 4 === 0 ? 2 + j : 41 + i, isActive: i !== 10, images: []
    }))
    return { id: productId, shopId: id(5), shopName: 'Nova Lifestyle', categoryId: id(11), categoryName: 'Áo & phụ kiện', name, slug: `san-pham-${i + 1}`, description: `${name} được chọn lọc bởi Nova Lifestyle. Chất liệu bền, dễ sử dụng và bảo quản. Bảo hành theo chính sách cửa hàng.`, brand: 'Nova', thumbnailUrl: '', minPrice: price, maxPrice: price, rating: 4.7, soldCount: 34 + i * 12, status: i === 9 ? 'Draft' : i === 10 ? 'Inactive' : i === 11 ? 'Banned' : 'Active', createdAt: iso(-24 * (i + 1)), attributesConfigJson: JSON.stringify([{ name: 'Màu', values: ['Trắng', 'Đen'] }]), skus, attributes: [{ name: 'Xuất xứ', value: 'Việt Nam' }] }
  })
  const inventory: InventoryItem[] = products.flatMap(product => product.skus.map(sku => ({ skuId: sku.id, skuCode: sku.skuCode, spuName: product.name, qtyOnHand: sku.qtyOnHand, reservedQty: sku.reservedQty, availableQty: sku.availableQty, minStock: 5, lastUpdated: iso(-2) })))
  const orders: OrderDetail[] = Array.from({ length: 30 }, (_, i) => {
    const product = products[i % products.length]
    const status = ORDER_STATUSES[i % ORDER_STATUSES.length]
    const createdAt = iso(-i * 8 - 1)
    return { id: id(400 + i), parentOrderId: id(500 + i), code: `NL2610${String(i + 1).padStart(4, '0')}`, shopId: id(5), subTotal: product.minPrice * 2 + 25_000, status, createdAt,
      warehouseAddressId: i % 2 ? warehouses[0].id : null, orderSource: ['Online', 'Livestream', 'FlashSale'][i % 3], recipient: { recipientName: ['Nguyễn Minh Anh', 'Trần Hoàng Nam', 'Lê Thu Hà'][i % 3], phone: '0902345678', provinceId: 202, provinceName: 'Hồ Chí Minh', districtId: 3695, districtName: 'Thủ Đức', wardCode: '90768', wardName: 'Linh Trung', detailAddress: `${25 + i} Đường số 8` },
      itemAmount: product.minPrice * 2, shippingFee: 30_000, shopDiscountAmount: 5_000, platformDiscountAmount: 0, shippingDiscountAmount: 0, cancelReason: status === 'Cancelled' ? 'Khách yêu cầu hủy' : null, paymentStatus: status === 'PendingPayment' ? 'Pending' : 'Paid', paymentMethod: status === 'PendingPayment' ? 'MoMo' : ['COD', 'MoMo', 'VietQR'][i % 3],
      items: [{ id: id(600 + i), skuId: product.skus[0].id, skuSnapshotJson: JSON.stringify({ spuName: product.name, skuCode: product.skus[0].skuCode, attributesJson: product.skus[0].attributesJson, thumbnailUrl: product.thumbnailUrl }), quantity: 2, unitPrice: product.minPrice, originalPrice: product.minPrice + 50_000, discountAmount: 0, lineTotal: product.minPrice * 2 }],
      timeline: [{ fromStatus: null, toStatus: 'PendingPayment', changedByRole: 'Buyer', note: 'Đơn hàng được tạo', changedAt: createdAt }, ...(status === 'PendingPayment' ? [] : [{ fromStatus: 'PendingPayment', toStatus: status, changedByRole: 'System', note: 'Cập nhật trạng thái đơn hàng', changedAt: iso(-i * 8) }])] }
  })
  const returns: OrderReturn[] = RETURN_STATUSES.map((status, i) => ({ id: id(700 + i), subOrderId: orders[i].id, subOrderCode: orders[i].code, buyerId: id(800 + i), reason: (['WrongItem', 'Defective', 'DamagedInShipping', 'NotAsDescribed', 'ChangeOfMind'] as const)[i % 5], status, requestedRefundAmount: orders[i].itemAmount, approvedRefundAmount: status === 'Completed' ? orders[i].itemAmount : null, createdAt: iso(i === 0 ? -54 : -i * 12), items: [{ orderItemId: orders[i].items[0].id, spuName: products[i].name, skuCode: products[i].skus[0].skuCode, quantity: 2, refundPrice: products[i].minPrice }], evidenceUrls: ['/favicon.svg'], returnAddress: ['SellerApproved', 'AdminApproved', 'Completed'].includes(status) ? warehouses[0] : null, sellerResponse: status === 'SellerRejected' ? 'Sản phẩm đã được sử dụng' : null, sellerEvidenceUrls: status === 'SellerRejected' ? ['/favicon.svg'] : [] }))
  const discounts: Discount[] = Array.from({ length: 12 }, (_, i) => ({ id: id(900 + i), shopId: id(5), code: `NOVA${i + 1}0`, name: ['Chào bạn mới', 'Ưu đãi cuối tuần', 'Mừng phiên live'][i % 3], discountType: i % 2 ? 'FixedCart' : 'PercentCart', discountValue: i % 2 ? 30_000 : 10, minOrderAmount: 150_000, maxDiscountAmount: i % 2 ? null : 50_000, maxUses: 100, usedCount: i * 3, perUserLimit: 1, appliesTo: 'AllProducts', targetIds: null, validFrom: iso(i % 4 === 0 ? 48 : -48), validTo: iso(i % 5 === 0 ? -12 : 168), isPublic: true, isActive: i !== 3, createdAt: iso(-96) }))
  const campaigns: Campaign[] = [{ id: id(1000), name: 'Đại tiệc cuối tuần', startAt: iso(-2), endAt: iso(22), bannerUrl: null, status: 'Active' }, { id: id(1001), name: 'Ngày hội mua sắm tháng 11', startAt: iso(72), endAt: iso(96), bannerUrl: null, status: 'Scheduled' }]
  const registrations: FlashRegistration[] = Array.from({ length: 4 }, (_, i) => ({ id: id(1100 + i), campaignId: campaigns[0].id, skuId: products[i].skus[0].id, spuId: products[i].id, spuName: products[i].name, skuCode: products[i].skus[0].skuCode, thumbnailUrl: '', originalPrice: products[i].minPrice, flashPrice: Math.floor(products[i].minPrice * 0.8), quantity: 20, soldQty: i === 1 ? 8 : 0, reservedQty: 0, perUserLimit: 2, status: (['Pending', 'Approved', 'Rejected', 'Ended'] as const)[i], rejectionReason: i === 2 ? 'Số lượng đăng ký chưa đáp ứng chiến dịch' : null }))
  const reviews: SellerReview[] = Array.from({ length: 15 }, (_, i) => ({ id: id(1200 + i), buyerId: id(800 + i), buyerName: ['Minh Anh', 'Hoàng Nam', 'Thu Hà'][i % 3], buyerAvatarUrl: null, spuId: products[i % 12].id, skuId: products[i % 12].skus[0].id, skuAttributesJson: products[i % 12].skus[0].attributesJson, rating: i % 5 + 1, title: null, content: ['Sản phẩm đẹp, đóng gói cẩn thận. Giao hàng rất nhanh!', 'Chất lượng phù hợp với giá. Shop hỗ trợ nhiệt tình.', 'Màu sắc hơi khác ảnh, mong shop kiểm tra giúp.'][i % 3], sellerReply: i % 3 === 0 ? 'Cảm ơn bạn đã tin tưởng Nova Lifestyle!' : null, repliedAt: i % 3 === 0 ? iso(-5) : null, createdAt: iso(-12 - i * 6), mediaUrls: i % 4 === 0 ? ['/favicon.svg'] : [], productName: products[i % 12].name, isHidden: i === 4 }))
  const transactions: WalletTransaction[] = Array.from({ length: 24 }, (_, i) => ({ id: id(1300 + i), walletId: id(6), amount: i % 8 === 0 || i % 8 === 5 ? 250_000 : -50_000, type: WALLET_TYPES[i % 8], description: i % 8 >= 3 && i % 8 <= 5 ? 'Lệnh rút tiền về tài khoản KYC đã xác minh' : `Giao dịch đơn NL2610${String(i + 1).padStart(4, '0')}`, balanceAfter: 0, createdAt: iso(-i * 6) }))
  let precedingBalance = 4_850_000
  transactions.forEach(transaction => {
    transaction.balanceAfter = precedingBalance
    if (transaction.type !== 'PayoutWithdrawal') precedingBalance -= transaction.amount
  })
  const lives: SellerLive[] = Array.from({ length: 6 }, (_, i) => ({ id: id(1400 + i), title: ['Ưu đãi thời trang cuối tuần', 'Góc nhà xinh', 'Khui hộp sản phẩm mới'][i % 3], status: i === 0 ? 'Live' : 'Ended', peakViewers: 125 + i * 27, totalRevenue: 1_250_000 + i * 300_000, totalOrdersCount: 8 + i, startedAt: iso(-i * 24 - 1), endedAt: i === 0 ? null : iso(-i * 24) }))
  const notifications: SellerNotification[] = Array.from({ length: 14 }, (_, i) => ({ id: id(1500 + i), title: ['Bạn có đơn hàng mới', 'Yêu cầu hoàn hàng cần phản hồi', 'Sản phẩm sắp hết hàng'][i % 3], content: ['Vui lòng chọn kho lấy hàng và chuẩn bị đơn.', 'Bạn còn dưới 24 giờ để xử lý yêu cầu hoàn hàng.', 'Kiểm tra tồn khả dụng và bổ sung hàng.'][i % 3], isRead: i > 4, createdAt: iso(-i), link: ['/seller/orders', '/seller/returns', '/seller/inventory?lowStock=true'][i % 3] }))
  const db: MockDatabase = { shop: { id: id(5), ownerId: id(7), shopName: 'Nova Lifestyle', slug: 'nova-lifestyle', description: 'Sản phẩm thiết thực cho cuộc sống hiện đại.', logoUrl: null, bannerUrl: null, phone: '0901234567', email: 'shop@example.vn', status: 'Active', rating: 4.8, followersCount: 1250, createdAt: iso(-720), verificationStatus: 'Approved', bankAccount: '0123456789' }, warehouses, categories, products, inventory, histories: inventory.map((item, i) => ({ id: id(1600 + i), skuId: item.skuId, skuCode: item.skuCode, spuName: item.spuName, changeType: 'Import', qtyBefore: 0, qtyChange: item.qtyOnHand, reservedBefore: 0, reservedChange: 0, qtyAfter: item.qtyOnHand, refType: null, refId: null, note: 'Nhập hàng đầu kỳ', createdBy: id(7), createdAt: iso(-48) })), orders, returns, discounts, campaigns, registrations, reviews, wallet: { shopId: id(5), balance: 4_850_000, lockedBalance: 1_000_000, updatedAt: iso() }, transactions, notifications, lives }
  if (MOCK_SCENARIO === 'empty') {
    db.products = []; db.inventory = []; db.histories = []; db.orders = []; db.returns = []; db.discounts = []
    db.campaigns = []; db.registrations = []; db.reviews = []; db.transactions = []; db.notifications = []; db.lives = []; db.warehouses = []
    db.wallet.balance = 0; db.wallet.lockedBalance = 0
  }
  return db
}
let cached: MockDatabase | null = null
let cachedKey = ''
function storageKey() {
  const user = (store.getState() as unknown as SellerRootState).user.currentUser
  return `novalive:seller:mock:v1:${user?.id || user?._id || 'anonymous'}:${MOCK_SCENARIO}`
}
export function db() {
  const key = storageKey()
  if (cached && cachedKey === key) return cached
  cachedKey = key
  try {
    const raw = sessionStorage.getItem(key)
    cached = raw ? JSON.parse(raw) as MockDatabase : seed()
  } catch { cached = seed() }
  return cached
}
export function saveMock() { try { sessionStorage.setItem(cachedKey, JSON.stringify(db())) } catch { /* Session storage can be unavailable in private mode. */ } }
export function paginate<T>(items: T[], request: PageRequest): PagedResult<T> {
  const page = Math.max(1, request.page)
  const size = Math.min(100, Math.max(1, request.size))
  return structuredClone({ items: items.slice((page - 1) * size, page * size), page, size, total: items.length, totalPages: Math.ceil(items.length / size) })
}
export function required<T>(value: T | undefined, label: string): T { if (!value) throw new Error(`${label} không tồn tại.`); return value }
export function mockDashboard(from?: string, to?: string): SellerDashboard {
  const data = db()
  const orders = data.orders.filter(order => (!from || order.createdAt >= from) && (!to || order.createdAt <= to))
  const paid = orders.filter(order => ['Delivered', 'Completed'].includes(order.status))
  const daily = new Map<string, { date: string; revenue: number; ordersCount: number }>()
  for (const order of paid) {
    const date = new Date(new Date(order.createdAt).getTime() + 7 * 3_600_000).toISOString().slice(0, 10)
    const entry = daily.get(date) || { date, revenue: 0, ordersCount: 0 }
    entry.revenue += order.itemAmount - order.shopDiscountAmount; entry.ordersCount++; daily.set(date, entry)
  }
  const totals = new Map<string, { skuId: string; spuName: string; skuCode: string; soldQuantity: number; totalRevenue: number }>()
  for (const order of paid) for (const item of order.items) {
    const stock = data.inventory.find(sku => sku.skuId === item.skuId)
    const entry = totals.get(item.skuId) || { skuId: item.skuId, spuName: stock?.spuName || 'Sản phẩm', skuCode: stock?.skuCode || '', soldQuantity: 0, totalRevenue: 0 }
    entry.soldQuantity += item.quantity; entry.totalRevenue += item.lineTotal; totals.set(item.skuId, entry)
  }
  const pending = data.orders.filter(order => order.status === 'Confirmed' && !order.warehouseAddressId).length
  return { totalSalesRevenue: paid.reduce((sum, order) => sum + order.itemAmount - order.shopDiscountAmount, 0), totalOrdersCount: orders.length, walletAvailableBalance: data.wallet.balance, pendingOrdersCount: pending, dailyRevenue: [...daily.values()].sort((a, b) => a.date.localeCompare(b.date)), orderStatuses: ORDER_STATUSES.map(status => ({ status, count: orders.filter(order => order.status === status).length })), topSkus: [...totals.values()].sort((a, b) => b.totalRevenue - a.totalRevenue).slice(0, 10), recentLives: data.lives.filter(live => live.startedAt && (!from || live.startedAt >= from) && (!to || live.startedAt <= to)).map(live => ({ sessionId: live.id, title: live.title, peakViewers: live.peakViewers, totalOrdersCount: live.totalOrdersCount, totalRevenue: live.totalRevenue, date: live.startedAt || iso() })), actions: { pendingOrders: pending, expiringReturns: data.returns.filter(item => item.status === 'Pending' && Date.now() - new Date(item.createdAt).getTime() >= 48 * 3_600_000).length, lowStockSkus: data.inventory.filter(item => item.qtyOnHand - item.reservedQty <= item.minStock).length } }
}
