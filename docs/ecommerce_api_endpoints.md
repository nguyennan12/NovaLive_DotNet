# 🔌 TÀI LIỆU TOÀN DIỆN REST API & REALTIME ENDPOINTS — NOVALIVE

> **Hệ thống**: NovaLive E-Commerce & Livestream Commerce Platform  
> **Phiên bản tài liệu**: 2.0 (Mapping 100% nghiệp vụ SRS v1.0 & Business Logic Rules)  
> **Base URL**: `https://api.novalive.vn/api/v1`  
> **Realtime WSS**: `wss://api.novalive.vn/hubs`  
> **Chuẩn Header**: `Authorization: Bearer <JWT_ACCESS_TOKEN>` | `Content-Type: application/json`

---

## 🏛️ QUY ƯỚC ĐẶT ROUTE & TIÊU CHUẨN PHÂN QUYỀN

Toàn bộ API của hệ thống được chuẩn hóa theo tiền tố (Route Prefix) phân tách 3 cổng làm việc độc lập (Clean Separation of Portals):

1. **`[Public/Buyer]` (`/api/v1/...`)**: Cổng công khai cho Khách vãng lai và Người mua hàng cá nhân.
2. **`[Seller]` (`/api/v1/seller/...`)**: Cổng Seller Center cho Nhà bán hàng quản lý Shop, Sản phẩm, Tồn kho, Đơn hàng, Vận chuyển, Ví và Livestream. **Backend tự động áp dụng `WHERE shop_id = _currentUser.ShopId`**.
3. **`[Admin]` (`/api/v1/admin/...`)**: Cổng Admin Portal cho Quản trị viên điều hành toàn sàn, kiểm duyệt KYC, phân xử tranh chấp khiếu nại, duyệt Payout và xem báo cáo tài chính.
4. **`[Webhook]` (`/api/v1/.../webhook/...`)**: Inbound Webhooks nhận callback từ bên ngoài (MoMo, VietQR, GHN, GHTK, ViettelPost) xác thực bằng Checksum / HMAC Signature.
5. **`[Realtime]` (`wss://api.novalive.vn/hubs/...`)**: WebSocket Hubs (SignalR + Redis Backplane) phục vụ thông báo đơn hàng, thanh toán và phòng Livestream.

---

# PHẦN 1 — CỔNG PUBLIC & BUYER PORTAL (`/api/v1/...`)

### 1.1 🔐 Module Auth & Identity (Xác thực & Bảo mật)
| Method | Endpoint | Request Body / Query Params | Mô tả nghiệp vụ | SRS Mapping | Quyền |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `POST` | `/auth/register` | `{ email, password, phone, fullName }` | Đăng ký tài khoản mới (`status = Unverified`), sinh OTP 6 số gửi email | `FR-AUTH-001`, `002`, `003` | `[Public]` |
| `POST` | `/auth/verify-otp` | `{ email, otp }` | Xác thực OTP 6 số kích hoạt tài khoản (`status = Active`), gán Role `Buyer` | `FR-AUTH-002`, `003` | `[Public]` |
| `POST` | `/auth/resend-otp` | `{ email }` | Gửi lại mã OTP (Giới hạn tối đa 3 lần / 15 phút qua Redis key lock) | `FR-AUTH-004`, `005` | `[Public]` |
| `POST` | `/auth/login` | `{ email, password }` | Đăng nhập (Kiểm tra sai 5 lần liên tiếp khóa backoff, trả về cặp Token) | `FR-AUTH-006`, `011` | `[Public]` |
| `POST` | `/auth/login/google` | `{ idToken }` | Đăng nhập / Đăng ký nhanh qua Google OAuth OpenID Connect — **hoãn, chưa làm** | `FR-AUTH-006` | `[Public]` |
| `POST` | `/auth/refresh` | `{ refreshToken }` | Cấp Access Token mới (Token Rotation; phát hiện token reuse sẽ revoke toàn bộ) | `FR-AUTH-007`, `008`, `009` | `[Public]` |
| `POST` | `/auth/logout` | `{ refreshToken }` | Đăng xuất (Đưa `jti` Access Token vào Redis Blacklist, revoke Refresh Token) | `FR-AUTH-010` | `[Authorize]` |
| `POST` | `/auth/forgot-password`| `{ email }` | Gửi mã OTP 6 số đặt lại mật khẩu qua email | `FR-AUTH-003` | `[Public]` |
| `POST` | `/auth/reset-password` | `{ email, otp, newPassword }` | Xác thực OTP và cập nhật mật khẩu mới | `FR-AUTH-003` | `[Public]` |
| `POST` | `/auth/change-password`| `{ oldPassword, newPassword }` | Đổi mật khẩu tài khoản và thu hồi toàn bộ phiên đăng nhập cũ | `FR-AUTH-010` | `[Authorize]` |
| `GET`  | `/auth/me` | — | Lấy thông tin tài khoản hiện tại kèm Roles, ShopId và Permissions | `FR-AUTH-006` | `[Authorize]` |

---

### 1.2 👤 Module User Profile, Sổ địa chỉ & Yêu thích (Buyer)
| Method | Endpoint | Request Body / Query Params | Mô tả nghiệp vụ | SRS Mapping | Quyền |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `GET`  | `/users/me` | — | Xem hồ sơ cá nhân của chính mình | `FR-USER-001` | `[Buyer]` |
| `PUT`  | `/users/me` | `{ fullName, phone, birthday, gender, avatarUrl }` | Cập nhật hồ sơ cá nhân | `FR-USER-001` | `[Buyer]` |
| `GET`  | `/users/me/addresses` | — | Xem danh sách sổ địa chỉ nhận hàng cá nhân | `FR-USER-002` | `[Buyer]` |
| `POST` | `/users/me/addresses` | `{ recipientName, phone, provinceId, provinceName, districtId, districtName, wardCode, wardName, detailAddress, isDefault }` | Thêm địa chỉ nhận hàng mới (Tự động unset default cũ nếu chọn `isDefault`) | `FR-USER-002`, `003` | `[Buyer]` |
| `PUT`  | `/users/me/addresses/{id}` | `{ recipientName, phone, provinceId, provinceName, districtId, districtName, wardCode, wardName, detailAddress, isDefault }` | Cập nhật địa chỉ nhận hàng chính chủ | `FR-USER-002` | `[Buyer]` |
| `DELETE`| `/users/me/addresses/{id}` | — | Xóa địa chỉ (Chặn xóa nếu là địa chỉ mặc định duy nhất còn lại) | `FR-USER-004` | `[Buyer]` |
| `PUT`  | `/users/me/addresses/{id}/default` | — | Đặt địa chỉ làm mặc định giao hàng | `FR-USER-003` | `[Buyer]` |
| `GET`  | `/users/me/wishlist` | `?page=&size=` | Danh sách sản phẩm Buyer đã bấm yêu thích | `FR-USER-001` | `[Buyer]` |
| `POST` | `/users/me/wishlist/{spuId}` | — | Thêm sản phẩm vào danh sách yêu thích | `FR-USER-001` | `[Buyer]` |
| `DELETE`| `/users/me/wishlist/{spuId}`| — | Xóa sản phẩm khỏi danh sách yêu thích | `FR-USER-001` | `[Buyer]` |

---

### 1.3 🏬 Module Public Shop & Gian hàng (Marketplace)
| Method | Endpoint | Request Body / Query Params | Mô tả nghiệp vụ | SRS Mapping | Quyền |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `POST` | `/shops/register` | `{ shopName, slug, description, phone, email, taxCode?, idCardFront, idCardBack, bankAccount, bankName, bankBranch }` | Nộp hồ sơ đăng ký mở gian hàng mới (`status = Pending`, tạo KYC) | `FR-SHOP-001`, `002`, `003` | `[Buyer]` |
| `GET`  | `/shops/{shopId}` | — | Xem trang thông tin công khai của Shop (Đánh giá sao, số lượng SP) | `FR-SHOP-007` | `[Public]` |
| `GET`  | `/shops/{shopId}/products` | `?categoryId=&keyword=&sort=&page=&size=` | Danh sách sản phẩm mở bán của một Shop cụ thể | `FR-CAT-005` | `[Public]` |
| `POST` | `/shops/{shopId}/follow` | — | Theo dõi gian hàng (Follow Shop) | `FR-SHOP-009` | `[Buyer]` |
| `DELETE`| `/shops/{shopId}/follow` | — | Bỏ theo dõi gian hàng (Unfollow Shop) | `FR-SHOP-009` | `[Buyer]` |

---

### 1.4 📦 Module Danh mục & Tìm kiếm Sản phẩm (Catalog & Search Engine)
| Method | Endpoint | Request Body / Query Params | Mô tả nghiệp vụ | SRS Mapping | Quyền |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `GET`  | `/categories` | — | Xem toàn bộ cây danh mục sản phẩm đa cấp (Tree JSON) | `FR-CAT-001` | `[Public]` |
| `GET`  | `/categories/{id}` | — | Chi tiết danh mục kèm Breadcrumb cấp cha-con | `FR-CAT-001` | `[Public]` |
| `GET`  | `/products` | `?shopId=&categoryId=&keyword=&minPrice=&maxPrice=&rating=&sort=&page=&size=` | Tìm kiếm & lọc sản phẩm công khai bằng PostgreSQL `tsvector` + `pg_trgm` | `FR-SEARCH-001`, `002`, `003` | `[Public]` |
| `GET`  | `/products/{spuId}` | — | Xem chi tiết sản phẩm SPU kèm toàn bộ biến thể SKU, giá & stock realtime | `FR-CAT-002`, `003` | `[Public]` |
| `GET`  | `/products/{spuId}/reviews` | `?rating=&hasMedia=&page=&size=` | Xem danh sách đánh giá sản phẩm (Lọc theo sao, có ảnh/video) | `FR-REVIEW-001`, `004` | `[Public]` |

---

### 1.5 🛒 Module Giỏ hàng & Đặt hàng đa Shop (Cart & Checkout Engine)
| Method | Endpoint | Request Body / Query Params | Mô tả nghiệp vụ | SRS Mapping | Quyền |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `GET`  | `/cart` | — | Xem giỏ hàng cá nhân (Gom nhóm sản phẩm theo từng Shop) | `FR-CART-001`, `002` | `[Buyer]` |
| `POST` | `/cart/items` | `{ skuId, quantity }` | Thêm SKU vào giỏ hàng (Cộng dồn số lượng nếu đã có) | `FR-CART-002` | `[Buyer]` |
| `PUT`  | `/cart/items/{cartItemId}` | `{ quantity }` | Cập nhật số lượng món trong giỏ (`quantity = 0` tự động xóa) | `FR-CART-003` | `[Buyer]` |
| `DELETE`| `/cart/items/{cartItemId}` | — | Xóa 1 món khỏi giỏ hàng | `FR-CART-003` | `[Buyer]` |
| `DELETE`| `/cart/items` | `{ cartItemIds: [] }` | Xóa nhiều món đã chọn cùng lúc | `FR-CART-003` | `[Buyer]` |
| `POST` | `/orders/calculate-checkout` | `{ cartItemIds: [], shippingAddressId, shopVouchers: [{ shopId, code }], platformProductVoucherCode?, platformFreeshipVoucherCode? }` | **Tính nháp Checkout Realtime** (Preview cước ship ĐVVC, phân rã voucher 3 cấp) | `FR-CHECKOUT-001`, `002`, `003` | `[Buyer]` |
| `POST` | `/orders/checkout` | `{ cartItemIds: [], shippingAddressId, shippingProviders: [{ shopId, provider, serviceCode }], shopVouchers: [{ shopId, code }], platformProductVoucherCode?, platformFreeshipVoucherCode?, paymentMethod: MoMo\|VietQR\|COD, note? }` | **Đặt hàng chính thức (8-Step Transaction)**: Tách Parent/Sub-orders, reserve tồn kho, tạo Payment `Pending` | `FR-CHECKOUT-004`, `005`, `006`, `007` | `[Buyer]` |
| `GET`  | `/orders` | `?status=&from=&to=&page=&size=` | Xem lịch sử đơn hàng đã mua của Buyer | `FR-ORD-001` | `[Buyer]` |
| `GET`  | `/orders/{orderId}` | — | Xem chi tiết đơn hàng (Snapshot SKU, Sub-orders, trạng thái, timeline) | `FR-CHECKOUT-005` | `[Buyer]` |
| `POST` | `/orders/{orderId}/cancel` | `{ reason }` | Buyer hủy đơn (Chỉ được hủy khi SubOrders chưa chuyển sang `Confirmed`) | `FR-CAT-011` | `[Buyer]` |
| `GET`  | `/orders/{orderId}/tracking` | — | Tra cứu hành trình vận đơn từ ĐVVC | `FR-SHIP-005` | `[Buyer]` |

---

### 1.6 💳 Module Thanh toán, Vouchers & Flash Sale (Buyer)
| Method | Endpoint | Request Body / Query Params | Mô tả nghiệp vụ | SRS Mapping | Quyền |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `POST` | `/payments/initiate` | `{ orderId, method: MoMo\|VietQR }` | Khởi tạo thanh toán online (Sinh URL MoMo / Mã VietQR động, TTL 15 phút) | `FR-PAY-001`, `002` | `[Buyer]` |
| `GET`  | `/payments/{paymentId}/status` | — | Query trạng thái thanh toán từ DB / Redis Cache | `FR-PAY-004` | `[Buyer]` |
| `GET`  | `/discounts/shop/{shopId}` | — | Xem danh sách Voucher công khai của Shop | `FR-DISCOUNT-001` | `[Public]` |
| `GET`  | `/discounts/platform` | — | Xem danh sách Voucher toàn sàn (Voucher SP & Voucher Freeship) | `FR-DISCOUNT-002`, `003` | `[Public]` |
| `POST` | `/discounts/validate` | `{ code, shopId?, cartItemIds: [] }` | Kiểm tra tính hợp lệ & số tiền giảm giá của mã | `FR-DISCOUNT-004` | `[Buyer]` |
| `GET`  | `/flash-sales/active` | — | Xem khung giờ Flash Sale đang diễn ra | `FR-FS-001` | `[Public]` |
| `GET`  | `/flash-sales/{campaignId}/products` | `?page=&size=` | Danh sách sản phẩm Flash Sale kèm số lượng % đã bán | `FR-FS-001` | `[Public]` |

---

### 1.7 🔄 Module Hoàn hàng, Đánh giá & Xem Livestream (Buyer)
| Method | Endpoint | Request Body / Query Params | Mô tả nghiệp vụ | SRS Mapping | Quyền |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `POST` | `/returns` | `{ subOrderId, reason, items: [{ orderItemId, quantity, reasonDetail? }], evidenceUrls: [] }` | Buyer tạo yêu cầu Trả hàng / Hoàn tiền (≤ 7 ngày từ lúc Delivered) | `FR-RETURN-001`, `002`, `003` | `[Buyer]` |
| `GET`  | `/returns` | `?status=&page=&size=` | Xem danh sách yêu cầu hoàn hàng của mình | `FR-RETURN-001` | `[Buyer]` |
| `GET`  | `/returns/{returnId}` | — | Xem chi tiết tiến trình xử lý khiếu nại trả hàng | `FR-RETURN-002` | `[Buyer]` |
| `POST` | `/reviews` | `{ orderItemId, rating, title?, content?, mediaUrls: [] }` | Viết đánh giá sao cho sản phẩm thuộc đơn đã giao thành công | `FR-REVIEW-001`, `002`, `004` | `[Buyer]` |
| `PUT`  | `/reviews/{reviewId}` | `{ rating, title?, content?, mediaUrls: [] }` | Chỉnh sửa đánh giá (Giới hạn 1 lần duy nhất trong vòng 30 ngày) | `FR-REVIEW-003` | `[Buyer]` |
| `GET`  | `/livestreams/active` | `?page=&size=` | Xem danh sách các phòng Livestream đang phát sóng | `FR-LIVE-003` | `[Public]` |
| `GET`  | `/livestreams/{sessionId}` | — | Chi tiết phòng live + Nhận Agora RTC Token vai trò Subscriber (Xem live) | `FR-LIVE-002`, `003` | `[Public]` |
| `GET`  | `/livestreams/{sessionId}/pinned-products` | — | Xem danh sách sản phẩm đang ghim trong phòng live | `FR-LIVE-005` | `[Public]` |
| `POST` | `/livestreams/{sessionId}/quick-buy` | `{ skuId, quantity, shippingAddressId, paymentMethod: MoMo\|VietQR\|COD }` | **Mua ngay sản phẩm ghim trong Live** (Bỏ qua giỏ hàng, tạo đơn tức thì không gián đoạn video) | `FR-LIVE-007` | `[Buyer]` |
| `GET`  | `/notifications` | `?isRead=&page=&size=` | Xem danh sách thông báo in-app cá nhân | `FR-NOTI-001` | `[Buyer]` |
| `PUT`  | `/notifications/{id}/read` | — | Đánh dấu 1 thông báo đã đọc | `FR-NOTI-001` | `[Buyer]` |
| `PUT`  | `/notifications/read-all` | — | Đánh dấu toàn bộ thông báo đã đọc | `FR-NOTI-001` | `[Buyer]` |
| `POST` | `/upload/media` | `(multipart/form-data: file, type)` | Upload hình ảnh/video lên MinIO S3 bucket | `NFR-SEC-008` | `[Authorize]` |

---

# PHẦN 2 — CỔNG SELLER CENTER PORTAL (`/api/v1/seller/...`)

> **Đặc tả Backend**: Toàn bộ Endpoint `/seller/*` yêu cầu quyền `[Authorize]` có Role `Seller`. Backend tự động ép điều kiện `WHERE shop_id = _currentUser.ShopId`.

### 2.1 🏪 Cấu hình Gian hàng & Kho vận (Shop Settings)
| Method | Endpoint | Request Body / Query Params | Mô tả nghiệp vụ | SRS Mapping | Quyền |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `GET`  | `/seller/shop/me` | — | Xem thông tin chi tiết gian hàng của chính mình | `FR-SHOP-005` | `[Seller]` |
| `PUT`  | `/seller/shop/me` | `{ shopName, description, logoUrl, bannerUrl, phone, email }` | Cập nhật hồ sơ gian hàng | `FR-SHOP-005` | `[Seller]` |
| `GET`  | `/seller/shop/addresses` | — | Xem danh sách kho lấy hàng & kho nhận hàng trả của Shop | `FR-SHOP-008` | `[Seller]` |
| `POST` | `/seller/shop/addresses` | `{ warehouseName, contactName, contactPhone, provinceId, provinceName, districtId, districtName, wardCode, wardName, detailAddress, isPrimary, isReturn }` | Thêm kho hàng mới | `FR-SHOP-008` | `[Seller]` |
| `PUT`  | `/seller/shop/addresses/{id}` | `{ warehouseName, contactName, contactPhone, provinceId, provinceName, districtId, districtName, wardCode, wardName, detailAddress, isPrimary, isReturn }` | Cập nhật thông tin kho hàng chính chủ | `FR-SHOP-008` | `[Seller]` |
| `DELETE`| `/seller/shop/addresses/{id}` | — | Xóa kho hàng | `FR-SHOP-008` | `[Seller]` |

---

### 2.2 📦 Quản lý Sản phẩm SPU / SKU & Sổ cái Kho hàng
| Method | Endpoint | Request Body / Query Params | Mô tả nghiệp vụ | SRS Mapping | Quyền |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `GET`  | `/seller/products` | `?searchTerm=&categoryId=&isActive=&page=&size=` | Danh sách sản phẩm của Shop (Xem cả sản phẩm Ẩn, Hết hàng, Nháp) | `FR-CAT-002` | `[Seller]` |
| `GET`  | `/seller/products/{spuId}` | — | Xem chi tiết SPU & danh sách SKU thuộc Shop mình | `FR-CAT-002` | `[Seller]` |
| `POST` | `/seller/products` | `{ name, description, categoryId, brand, thumbnailUrl, attributesConfig: [], skus: [{ skuCode, attributesJson, originalPrice, sellPrice, weightGram, initialStock, images: [] }], attributes: [] }` | Tạo mới SPU kèm ma trận SKU biến thể | `FR-CAT-002`, `003`, `004` | `[Seller]` |
| `PUT`  | `/seller/products/{spuId}` | `{ name, description, categoryId, brand, thumbnailUrl, attributesConfig, attributes }` | Cập nhật thông tin SPU | `FR-CAT-002` | `[Seller]` |
| `DELETE`| `/seller/products/{spuId}` | — | Soft-delete / Ẩn sản phẩm khỏi gian hàng | `FR-CAT-005` | `[Seller]` |
| `PUT`  | `/seller/skus/{skuId}` | `{ sellPrice, originalPrice, weightGram, isActive }` | Cập nhật giá bán, trọng lượng và trạng thái SKU | `FR-CAT-005` | `[Seller]` |
| `GET`  | `/seller/inventory` | `?lowStock=true&page=&size=` | Bảng theo dõi tồn kho 2 trạng thái (`qty_on_hand`, `reserved_qty`) | `FR-CAT-007`, `008` | `[Seller]` |
| `POST` | `/seller/inventory/adjust` | `{ skuId, qtyChange, changeType: ManualAdjust\|StockIn\|DamageOut, note }` | Điều chỉnh tồn kho thủ công (Ghi nhận sổ cái `InventoryHistories`) | `FR-CAT-006` | `[Seller]` |
| `GET`  | `/seller/inventory/histories` | `?skuId=&from=&to=&page=&size=` | Xem sổ cái lịch sử biến động kho hàng append-only | `FR-CAT-006` | `[Seller]` |

---

### 2.3 🚚 Quản lý Đơn hàng & In phiếu giao (Fulfillment & Shipping)
| Method | Endpoint | Request Body / Query Params | Mô tả nghiệp vụ | SRS Mapping | Quyền |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `GET`  | `/seller/orders` | `?status=&from=&to=&page=&size=` | Xem danh sách Sub-orders thuộc gian hàng mình quản lý | `FR-SHIP-002` | `[Seller]` |
| `GET`  | `/seller/orders/{subOrderId}` | — | Xem chi tiết Sub-order (thông tin người nhận, snapshot giá, cước phí) | `FR-CHECKOUT-005` | `[Seller]` |
| `PUT`  | `/seller/orders/{subOrderId}/confirm` | `{ warehouseAddressId }` | Seller xác nhận đơn & chọn kho lấy hàng (`status = Confirmed`) | `FR-SHIP-002` | `[Seller]` |
| `POST` | `/seller/shipping/create-order` | `{ subOrderId, provider: GHN\|GHTK\|ViettelPost, serviceCode, pickupAddressId }` | Đẩy đơn sang ĐVVC, tạo vận đơn lấy mã `tracking_code` | `FR-SHIP-003`, `004` | `[Seller]` |
| `GET`  | `/seller/shipping/{subOrderId}/label` | — | Tải file PDF phiếu giao hàng A6 chuẩn khổ in ĐVVC | `FR-SHIP-003` | `[Seller]` |
| `GET`  | `/seller/shipping/{subOrderId}/tracking` | — | Tra cứu hành trình vận đơn trực tiếp | `FR-SHIP-005` | `[Seller]` |
| `POST` | `/seller/orders/{subOrderId}/cancel` | `{ reason }` | Seller hủy đơn do hết hàng/sự cố (`Inventories.reserved_qty` hoàn trả) | `FR-CAT-011` | `[Seller]` |

---

### 2.4 🔄 Xử lý Hoàn hàng, Vouchers, Flash Sale & Live Studio
| Method | Endpoint | Request Body / Query Params | Mô tả nghiệp vụ | SRS Mapping | Quyền |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `GET`  | `/seller/returns` | `?status=&page=&size=` | Xem danh sách yêu cầu trả hàng cần Shop xử lý | `FR-RETURN-004` | `[Seller]` |
| `PUT`  | `/seller/returns/{returnId}/approve` | `{ note? }` | Seller chấp thuận hoàn hàng (cung cấp địa chỉ kho nhận trả) | `FR-RETURN-004` | `[Seller]` |
| `PUT`  | `/seller/returns/{returnId}/reject` | `{ reason, evidenceUrls: [] }` | Seller từ chối yêu cầu hoàn hàng (tải chứng cứ đối chứng) | `FR-RETURN-004` | `[Seller]` |
| `PUT`  | `/seller/returns/{returnId}/received` | — | Seller xác nhận đã nhận lại hàng hoàn -> Kích hoạt hoàn tiền từ tài khoản Shop | `FR-RETURN-007` | `[Seller]` |
| `GET`  | `/seller/discounts` | `?status=&page=&size=` | Danh sách Voucher do Shop phát hành | `FR-DISCOUNT-007` | `[Seller]` |
| `POST` | `/seller/discounts` | `{ code, name, discountType: PercentCart\|FixedCart, discountValue, minOrderAmount, maxDiscountAmount?, maxUses?, perUserLimit, validFrom, validTo, isPublic }` | Tạo Voucher giảm giá mới của Shop | `FR-DISCOUNT-001`, `007` | `[Seller]` |
| `PUT`  | `/seller/discounts/{id}` | `{ name, maxUses?, validTo, isActive }` | Chỉnh sửa hạn mức / thời hạn Voucher | `FR-DISCOUNT-007` | `[Seller]` |
| `POST` | `/seller/flash-sales/{campaignId}/register` | `{ skuId, flashPrice, quantity, perUserLimit }` | Đăng ký SKU vào chiến dịch Flash Sale của Sàn (Giá giảm ≥ 10%) | `FR-FS-002`, `004` | `[Seller]` |
| `GET`  | `/seller/flash-sales/registrations` | `?status=&page=&size=` | Danh sách sản phẩm Shop đã gửi duyệt Flash Sale | `FR-FS-002` | `[Seller]` |
| `POST` | `/seller/livestreams/start` | `{ title, thumbnailUrl?, warehouseAddressId }` | Khởi tạo phiên phát Live & nhận Agora RTC Token vai trò Publisher | `FR-LIVE-001`, `002` | `[Seller]` |
| `POST` | `/seller/livestreams/{sessionId}/end` | — | Kết thúc Live & tổng kết doanh số phát sinh | `FR-LIVE-001` | `[Seller]` |
| `POST` | `/seller/livestreams/{sessionId}/pin-product` | `{ skuId, flashPrice?, quantityLimit? }` | Ghim sản phẩm lên màn hình Live (SignalR `ProductPinned`) | `FR-LIVE-005`, `006` | `[Seller]` |
| `DELETE`| `/seller/livestreams/{sessionId}/pin-product/{skuId}` | — | Gỡ ghim sản phẩm (SignalR `ProductUnpinned`) | `FR-LIVE-005` | `[Seller]` |
| `POST` | `/seller/reviews/{reviewId}/reply` | `{ replyText }` | Seller phản hồi công khai đánh giá của khách hàng (1 lần duy nhất) | `FR-REVIEW-005` | `[Seller]` |

---

### 2.5 💰 Ví Shop, Rút tiền & Dashboard Doanh thu
| Method | Endpoint | Request Body / Query Params | Mô tả nghiệp vụ | SRS Mapping | Quyền |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `GET`  | `/seller/wallet` | — | Xem số dư khả dụng (`balance`), số dư khóa rút (`locked_balance`) | `FR-WALLET-001` | `[Seller]` |
| `GET`  | `/seller/wallet/transactions` | `?type=&from=&to=&page=&size=` | Xem sổ cái lịch sử biến động số dư ví Shop append-only | `FR-WALLET-002` | `[Seller]` |
| `POST` | `/seller/wallet/payout` | `{ amount, bankAccount, bankName, bankBranch }` | Tạo yêu cầu rút tiền về tài khoản ngân hàng (`balance -= amount`, `locked += amount`) | `FR-WALLET-003`, `004` | `[Seller]` |
| `GET`  | `/seller/reports/dashboard` | `?from=&to=` | **Dashboard Shop**: Biểu đồ doanh thu thuần, phân rã trạng thái đơn, Top 10 SP bán chạy, hiệu quả Livestream | `FR-REPORT-002` | `[Seller]` |

---

# PHẦN 3 — CỔNG ADMIN OPERATIONS PORTAL (`/api/v1/admin/...`)

> **Đặc tả Backend**: Toàn bộ Endpoint `/admin/*` yêu cầu quyền `[Authorize(Roles = "Admin")]` hoặc `[Authorize(Roles = "SuperAdmin")]`.

### 3.1 🛡️ Quản trị Gian hàng, Người dùng & Danh mục
| Method | Endpoint | Request Body / Query Params | Mô tả nghiệp vụ | SRS Mapping | Quyền |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `GET`  | `/admin/shops` | `?status=&keyword=&page=&size=` | Xem danh sách toàn bộ gian hàng trên sàn | `FR-ADMIN-001` | `[Admin]` |
| `GET`  | `/admin/shops/{id}` | — | Xem chi tiết hồ sơ KYC gian hàng (CCCD, GPKD, STK) | `FR-SHOP-003` | `[Admin]` |
| `PUT`  | `/admin/shops/{id}/approve` | `{ note? }` | Phê duyệt kích hoạt Shop (`status = Active`), cấp Role `Seller` và tạo ví `ShopWallets` | `FR-SHOP-004`, `005` | `[Admin]` |
| `PUT`  | `/admin/shops/{id}/reject` | `{ reason }` | Từ chối hồ sơ mở Shop | `FR-SHOP-004` | `[Admin]` |
| `PUT`  | `/admin/shops/{id}/suspend` | `{ reason }` | Tạm ngưng hoạt động của Shop vi phạm | `FR-SHOP-006`, `007` | `[Admin]` |
| `PUT`  | `/admin/shops/{id}/ban` | `{ reason }` | Khóa vĩnh viễn gian hàng vi phạm | `FR-SHOP-006`, `007` | `[Admin]` |
| `PUT`  | `/admin/shops/{id}/unban` | — | Mở khóa gian hàng về trạng thái `Active` | `FR-ADMIN-002` | `[Admin]` |
| `GET`  | `/admin/users` | `?status=&role=&keyword=&page=&size=` | Quản lý danh sách người dùng toàn hệ thống | `FR-ADMIN-001` | `[Admin]` |
| `PUT`  | `/admin/users/{id}/status` | `{ status: Active\|Locked\|Suspended, reason }` | Khóa hoặc mở khóa tài khoản người dùng | `FR-AUTH-011` | `[Admin]` |
| `POST` | `/admin/categories` | `{ name, parentId?, iconUrl?, displayOrder }` | Tạo danh mục sản phẩm mới | `FR-ADMIN-003` | `[Admin]` |
| `PUT`  | `/admin/categories/{id}` | `{ name, parentId?, iconUrl?, displayOrder, isVisible }` | Cập nhật danh mục sản phẩm | `FR-ADMIN-003` | `[Admin]` |
| `DELETE`| `/admin/categories/{id}` | — | Xóa danh mục sản phẩm (Chặn xóa nếu còn sản phẩm con) | `FR-ADMIN-003` | `[Admin]` |

---

### 3.2 📦 Kiểm duyệt Sản phẩm & Phán quyết Tranh chấp
| Method | Endpoint | Request Body / Query Params | Mô tả nghiệp vụ | SRS Mapping | Quyền |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `GET`  | `/admin/products` | `?shopId=&categoryId=&isViolation=&page=&size=` | Xem danh sách sản phẩm của MỌI shop trên sàn | `FR-ADMIN-001` | `[Admin]` |
| `PUT`  | `/admin/products/{spuId}/ban` | `{ reason }` | Gỡ / Ẩn sản phẩm vi phạm bản quyền / chính sách toàn sàn | `FR-CAT-005` | `[Admin]` |
| `GET`  | `/admin/disputes` | `?page=&size=` | Danh sách tranh chấp hoàn hàng leo thang lên Admin | `FR-RETURN-005` | `[Admin]` |
| `GET`  | `/admin/disputes/{returnId}` | — | Xem chi tiết bằng chứng đối chứng giữa Buyer & Seller | `FR-RETURN-002` | `[Admin]` |
| `PUT`  | `/admin/disputes/{returnId}/resolve` | `{ decision: BuyerWins\|SellerWins, refundAmount?, note }` | **Phán quyết tranh chấp cuối cùng**: `BuyerWins` (Hoàn tiền từ tài khoản Shop về Buyer) / `SellerWins` (Giữ nguyên giao dịch cho Shop) | `FR-RETURN-006`, `007`, `008` | `[Admin]` |
| `PUT`  | `/admin/reviews/{reviewId}/hide` | `{ reason }` | Ẩn đánh giá vi phạm thuần phong mỹ tục / từ cấm | `FR-REVIEW-006` | `[Admin]` |
| `PUT`  | `/admin/livestreams/{sessionId}/terminate` | `{ reason }` | Cưỡng chế ngắt phòng Livestream vi phạm | `FR-LIVE-001` | `[Admin]` |

---

### 3.3 ⚡ Vouchers Sàn, Flash Sale, Payout & Dashboard Điều hành
| Method | Endpoint | Request Body / Query Params | Mô tả nghiệp vụ | SRS Mapping | Quyền |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `POST` | `/admin/discounts` | `{ code, name, discountType: PercentCart\|FixedCart\|PercentShip\|FreeShip, discountValue, minOrderAmount, maxDiscountAmount?, maxUses?, perUserLimit, validFrom, validTo, isPublic }` | Tạo Voucher Sàn (Platform Voucher Sàn tài trợ) | `FR-DISCOUNT-002`, `003`, `008` | `[Admin]` |
| `POST` | `/admin/flash-sales` | `{ name, startAt, endAt, bannerUrl? }` | Khởi tạo khung giờ chiến dịch Flash Sale toàn sàn | `FR-FS-001`, `FR-ADMIN-004` | `[Admin]` |
| `PUT`  | `/admin/flash-sales/items/{itemId}/approve` | — | Phê duyệt sản phẩm SKU vào Flash Sale | `FR-FS-003` | `[Admin]` |
| `PUT`  | `/admin/flash-sales/items/{itemId}/reject` | `{ reason }` | Từ chối sản phẩm tham gia Flash Sale | `FR-FS-003` | `[Admin]` |
| `GET`  | `/admin/payouts` | `?status=&page=&size=` | Danh sách các lệnh yêu cầu rút tiền của Seller | `FR-ADMIN-006` | `[Admin]` |
| `PUT`  | `/admin/payouts/{id}/approve` | `{ transferRef }` | Xác nhận đã chuyển khoản thành công (`locked_balance -= amount`) | `FR-WALLET-005` | `[Admin]` |
| `PUT`  | `/admin/payouts/{id}/reject` | `{ reason }` | Từ chối lệnh rút tiền & hoàn tiền lại ví (`locked_balance -= amount`, `balance += amount`) | `FR-WALLET-005` | `[Admin]` |
| `GET`  | `/admin/reports/dashboard` | `?from=&to=` | **Executive Dashboard**: GMV toàn sàn, Doanh thu hoa hồng thực nhận (Platform Net Revenue = $\sum \text{platform\_fee}$), Tỷ lệ hoàn hàng & tranh chấp | `FR-REPORT-001` | `[Admin]` |
| `GET`  | `/admin/audit-logs` | `?action=&userId=&from=&to=&page=&size=` | Tra cứu nhật ký hành động nhạy cảm của Admin | `NFR-SEC-006` | `[Admin]` |

---

### 3.4 🔐 Quản trị Phân quyền RBAC (Roles & Permissions Management)
| Method | Endpoint | Request Body / Query Params | Mô tả nghiệp vụ | SRS Mapping | Quyền |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `GET`  | `/admin/roles` | — | Xem danh sách tất cả các Vai trò (Roles) trong hệ thống | `FR-AUTH-001`, `FR-SHOP-005` | `[Admin]` |
| `POST` | `/admin/roles` | `{ name, description, permissionCodes: [] }` | Tạo vai trò mới (ví dụ: `Moderator`, `FinanceStaff`, `CustomerSupport`) | `FR-AUTH-001` | `[Admin]` |
| `PUT`  | `/admin/roles/{id}` | `{ name, description }` | Cập nhật tên / mô tả vai trò | `FR-AUTH-001` | `[Admin]` |
| `DELETE`| `/admin/roles/{id}` | — | Xóa vai trò tùy biến (Chặn xóa các System Roles `Admin`, `Seller`, `Buyer`) | `FR-AUTH-001` | `[Admin]` |
| `GET`  | `/admin/permissions` | — | Xem toàn bộ danh sách mã Permission chuẩn trong hệ thống (`{resource}:{action}`) | `FR-AUTH-001` | `[Admin]` |
| `GET`  | `/admin/roles/{id}/permissions` | — | Xem danh sách mã quyền đang gán cho một Role | `FR-AUTH-001` | `[Admin]` |
| `PUT`  | `/admin/roles/{id}/permissions` | `{ permissionCodes: [] }` | **Cập nhật ma trận quyền cho Role** (Gán/Bỏ gán danh sách permissions) | `FR-AUTH-001` | `[Admin]` |
| `GET`  | `/admin/users/{id}/roles` | — | Xem danh sách các Role đang gán cho một User | `FR-AUTH-006` | `[Admin]` |
| `POST` | `/admin/users/{id}/roles` | `{ roleId }` | Gán thêm Role cho User (ví dụ: bổ nhiệm User làm `Moderator` hoặc `Staff`) | `FR-AUTH-001` | `[Admin]` |
| `DELETE`| `/admin/users/{id}/roles/{roleId}` | — | Thu hồi Role của User | `FR-AUTH-001` | `[Admin]` |

---

# PHẦN 4 — INBOUND WEBHOOKS & REALTIME SIGNALR HUBS

### 4.1 🔔 Inbound Webhooks (Bên thứ ba gọi vào)
| Method | Endpoint | Xác thực bảo mật | Nghiệp vụ xử lý tự động (Idempotent) | SRS Mapping |
| :--- | :--- | :--- | :--- | :---: |
| `POST` | `/payments/webhook/momo` | HMAC SHA256 Signature Header | Cập nhật `Payments.status = Success` -> Ghi nhận doanh thu ví Shop & Trừ tồn kho vật lý qua RabbitMQ | `FR-PAY-003`, `FR-CAT-010` |
| `POST` | `/payments/webhook/vietqr`| Signature / Secret Header | Cập nhật thanh toán chuyển khoản VietQR thành công -> Ghi nhận doanh thu ví Shop | `FR-PAY-003` |
| `POST` | `/shipping/webhook/ghn` | Checksum HMAC Header | Cập nhật trạng thái vận đơn GHN. Khi `Delivered` -> Mở quyền Review | `FR-SHIP-006`, `007` |
| `POST` | `/shipping/webhook/ghtk`| Token Header | Cập nhật hành trình vận đơn GHTK tự động | `FR-SHIP-006`, `007` |
| `POST` | `/shipping/webhook/viettelpost` | Token Header | Cập nhật hành trình vận đơn ViettelPost tự động | `FR-SHIP-006`, `007` |

---

### 4.2 ⚡ Realtime SignalR Hubs (`NovaLive.RealtimeApi`)
| Hub Path | Event / Action | Hướng truyền | Mô tả sự kiện Realtime | SRS Mapping |
| :--- | :--- | :---: | :--- | :---: |
| `wss://api.novalive.vn/hubs/livestream` | `JoinLiveSession(sessionId)` | Client $\rightarrow$ Server | Người xem tham gia phòng Live -> Tăng viewer count | `FR-LIVE-004` |
| | `SendMessage(content)` | Client $\rightarrow$ Server | Gửi bình luận trong phiên Live | `FR-LIVE-004`, `008` |
| | `SendReaction(type)` | Client $\rightarrow$ Server | Thả tim bay / biểu cảm (Batching mỗi 500ms) | `FR-LIVE-004` |
| | `ReceiveMessage` | Server $\rightarrow$ Client | Broadcast tin nhắn chat mới tới toàn bộ người xem | `FR-LIVE-004` |
| | `ReceiveReaction` | Server $\rightarrow$ Client | Broadcast hiệu ứng tim bay Canvas realtime | `FR-LIVE-004` |
| | `ProductPinned` | Server $\rightarrow$ Client | Broadcast sản phẩm Seller vừa ghim kèm giá Flash đếm ngược | `FR-LIVE-005`, `006` |
| | `ProductUnpinned` | Server $\rightarrow$ Client | Broadcast thông báo gỡ ghim sản phẩm | `FR-LIVE-005` |
| | `ViewerCountUpdated` | Server $\rightarrow$ Client | Cập nhật số lượng người đang xem trực tiếp | `FR-LIVE-004` |
| `wss://api.novalive.vn/hubs/order` | `OrderPlaced` | Server $\rightarrow$ Client | Bắn âm thanh & pop-up thông báo đơn mới tức thì cho Seller | `FR-NOTI-002` |
| | `OrderStatusChanged` | Server $\rightarrow$ Client | Bắn cập nhật trạng thái đơn hàng thời gian thực cho Buyer | `FR-NOTI-003` |
| `wss://api.novalive.vn/hubs/payment` | `PaymentSuccess` | Server $\rightarrow$ Client | Bắn sự kiện thanh toán QR thành công -> Frontend tự đóng modal QR và chuyển trang thành công | `FR-NOTI-003` |
