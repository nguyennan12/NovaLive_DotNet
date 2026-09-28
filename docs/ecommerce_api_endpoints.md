# 🔌 API ENDPOINTS — HỆ THỐNG NOVALIVE E-COMMERCE & LIVESTREAM

```
Base URL: https://api.novalive.vn/api/v1
Authentication: Bearer JWT (HMAC-SHA256 / HS256)

Roles & Ký hiệu:
  [Public]  = Không yêu cầu đăng nhập (Công khai)
  [Buyer]   = Yêu cầu Token người mua (Tài khoản User active)
  [Seller]  = Yêu cầu Token người bán (Đã có Shop active)
  [Admin]   = Yêu cầu Token quản trị viên
```

---

## 1. 🔐 AUTHENTICATION & IDENTITY

| Method | Endpoint | Payload / Params | Mô tả chức năng | Quyền |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/auth/register` | `{ email, password, phone, fullName }` | Đăng ký tài khoản, gửi OTP xác thực | `[Public]` |
| `POST` | `/auth/verify-otp` | `{ email, otp }` | Nhập mã OTP để kích hoạt tài khoản | `[Public]` |
| `POST` | `/auth/resend-otp` | `{ email }` | Gửi lại mã OTP (giới hạn 3 lần/15 phút) | `[Public]` |
| `POST` | `/auth/login` | `{ email, password }` | Đăng nhập, trả về `{ accessToken, refreshToken, expiresAt }` | `[Public]` |
| `POST` | `/auth/login/google` | `{ idToken }` | Đăng nhập/Đăng ký nhanh qua Google OAuth | `[Public]` |
| `POST` | `/auth/refresh` | `{ refreshToken }` | Cấp Access Token mới (Token Rotation) | `[Public]` |
| `POST` | `/auth/logout` | `{ refreshToken }` | Đăng xuất, thu hồi Token và đưa vào Blacklist | `[Buyer/Seller]` |
| `POST` | `/auth/forgot-password`| `{ email }` | Gửi mã OTP/link đặt lại mật khẩu | `[Public]` |
| `POST` | `/auth/reset-password` | `{ email, otp, newPassword }` | Đặt lại mật khẩu mới với OTP | `[Public]` |
| `POST` | `/auth/change-password`| `{ oldPassword, newPassword }` | Đổi mật khẩu tài khoản | `[Buyer/Seller]` |
| `GET`  | `/auth/me` | — | Lấy thông tin tài khoản hiện tại kèm Roles | `[Buyer/Seller/Admin]` |

---

## 2. 👤 USER PROFILE & SỔ ĐỊA CHỈ

| Method | Endpoint | Payload / Params | Mô tả chức năng | Quyền |
| :--- | :--- | :--- | :--- | :--- |
| `GET`  | `/users/me` | — | Xem hồ sơ cá nhân đầy đủ | `[Buyer]` |
| `PUT`  | `/users/me` | `{ fullName, phone, birthday, gender, avatarUrl }` | Cập nhật hồ sơ cá nhân | `[Buyer]` |
| `GET`  | `/users/me/addresses` | — | Danh sách địa chỉ nhận hàng của Buyer | `[Buyer]` |
| `POST` | `/users/me/addresses` | `{ recipientName, phone, provinceName, districtName, wardName, detailAddress, isDefault }` | Thêm địa chỉ nhận hàng mới | `[Buyer]` |
| `PUT`  | `/users/me/addresses/{id}` | `{ recipientName, phone, provinceName, districtName, wardName, detailAddress, isDefault }` | Sửa địa chỉ nhận hàng | `[Buyer]` |
| `DELETE` | `/users/me/addresses/{id}` | — | Xóa địa chỉ (không xóa địa chỉ mặc định) | `[Buyer]` |
| `PUT`  | `/users/me/addresses/{id}/default` | — | Đặt làm địa chỉ giao hàng mặc định | `[Buyer]` |
| `GET`  | `/users/me/wishlist` | — | Danh sách sản phẩm yêu thích | `[Buyer]` |
| `POST` | `/users/me/wishlist` | `{ spuId }` | Thêm sản phẩm vào yêu thích | `[Buyer]` |
| `DELETE` | `/users/me/wishlist/{spuId}` | — | Xóa sản phẩm khỏi yêu thích | `[Buyer]` |

---

## 3. 🏬 SHOP & QUẢN TRỊ GIAN HÀNG

| Method | Endpoint | Payload / Params | Mô tả chức năng | Quyền |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/shops/register` | `{ shopName, slug, description, phone, email, taxCode?, idCardFront, idCardBack, bankAccount, bankName, bankBranch }` | Nộp hồ sơ đăng ký mở gian hàng mới | `[Buyer]` |
| `GET`  | `/shops/me` | — | Xem thông tin chi tiết gian hàng của mình | `[Seller]` |
| `PUT`  | `/shops/me` | `{ shopName, description, logoUrl, bannerUrl, phone, email }` | Cập nhật thông tin gian hàng | `[Seller]` |
| `GET`  | `/shops/me/addresses` | — | Danh sách kho lấy hàng/trả hàng của Shop | `[Seller]` |
| `POST` | `/shops/me/addresses` | `{ warehouseName, contactName, contactPhone, provinceName, districtName, wardName, detailAddress, isPrimary, isReturn }` | Thêm kho lấy hàng/trả hàng mới | `[Seller]` |
| `GET`  | `/shops/{shopId}` | — | Trang thông tin công khai của Shop | `[Public]` |
| `GET`  | `/shops/{shopId}/products` | `?categoryId=&keyword=&sort=&page=&size=` | Danh sách sản phẩm của một Shop cụ thể | `[Public]` |
| `POST` | `/shops/{shopId}/follow` | — | Theo dõi gian hàng | `[Buyer]` |
| `DELETE` | `/shops/{shopId}/follow` | — | Bỏ theo dõi gian hàng | `[Buyer]` |
| `GET`  | `/seller/wallet` | — | Xem số dư ví khả dụng & số dư chờ Escrow | `[Seller]` |
| `GET`  | `/seller/wallet/transactions` | `?type=&from=&to=&page=&size=` | Lịch sử biến động số dư ví Shop | `[Seller]` |
| `POST` | `/seller/wallet/payout` | `{ amount, bankAccount, bankName }` | Yêu cầu rút tiền về tài khoản ngân hàng | `[Seller]` |

---

## 4. 🗂️ CATEGORY (DANH MỤC SẢN PHẨM)

| Method | Endpoint | Payload / Params | Mô tả chức năng | Quyền |
| :--- | :--- | :--- | :--- | :--- |
| `GET`  | `/categories` | — | Lấy toàn bộ cây danh mục đa cấp (Tree JSON) | `[Public]` |
| `GET`  | `/categories/{id}` | — | Chi tiết danh mục kèm Breadcrumb | `[Public]` |
| `POST` | `/categories` | `{ name, parentId?, iconUrl?, displayOrder }` | Tạo danh mục mới | `[Admin]` |
| `PUT`  | `/categories/{id}` | `{ name, parentId?, iconUrl?, displayOrder, isVisible }` | Cập nhật danh mục | `[Admin]` |
| `DELETE` | `/categories/{id}` | — | Xóa danh mục (khi không có sản phẩm) | `[Admin]` |

---

## 5. 📦 SẢN PHẨM (SPU / SKU) & TỒN KHO

| Method | Endpoint | Payload / Params | Mô tả chức năng | Quyền |
| :--- | :--- | :--- | :--- | :--- |
| `GET`  | `/products` | `?shopId=&categoryId=&keyword=&minPrice=&maxPrice=&sort=&page=&size=` | Tìm kiếm & lọc danh sách sản phẩm (Elasticsearch) | `[Public]` |
| `GET`  | `/products/{spuId}` | — | Chi tiết sản phẩm SPU kèm toàn bộ SKU biến thể | `[Public]` |
| `GET`  | `/products/search` | `?keyword=&page=&size=` | Full-text search sản phẩm với gợi ý tự động | `[Public]` |
| `GET`  | `/seller/products` | `?status=&categoryId=&page=&size=` | Danh sách sản phẩm của Shop | `[Seller]` |
| `POST` | `/seller/products` | `{ name, description, categoryId, brand, thumbnail_url, attributesConfig: [], skus: [{ skuCode, attributesJson, originalPrice, sellPrice, weightGram, initialStock, images: [] }], attributes: [] }` | Tạo SPU kèm danh sách SKU biến thể | `[Seller]` |
| `PUT`  | `/seller/products/{spuId}` | `{ name, description, categoryId, brand, thumbnail_url, attributesConfig, attributes }` | Sửa thông tin SPU | `[Seller]` |
| `DELETE` | `/seller/products/{spuId}` | — | Soft-delete sản phẩm | `[Seller]` |
| `PUT`  | `/seller/skus/{skuId}` | `{ sellPrice, originalPrice, weightGram, isActive }` | Cập nhật giá bán/trọng lượng biến thể SKU | `[Seller]` |
| `GET`  | `/seller/inventory` | `?lowStock=true&page=&size=` | Bảng theo dõi tồn kho các SKU của Shop | `[Seller]` |
| `POST` | `/seller/inventory/adjust` | `{ skuId, qtyChange, changeType, note }` | Điều chỉnh tồn kho thủ công (Nhập hàng/Xuất hủy) | `[Seller]` |
| `GET`  | `/seller/inventory/histories` | `?skuId=&from=&to=&page=&size=` | Xem sổ cái lịch sử biến động tồn kho | `[Seller]` |

---

## 6. 🛒 GIỎ HÀNG (CART)

| Method | Endpoint | Payload / Params | Mô tả chức năng | Quyền |
| :--- | :--- | :--- | :--- | :--- |
| `GET`  | `/cart` | — | Lấy toàn bộ giỏ hàng (gom nhóm theo từng Shop) | `[Buyer]` |
| `POST` | `/cart/items` | `{ skuId, quantity }` | Thêm sản phẩm vào giỏ (cộng dồn nếu đã có) | `[Buyer]` |
| `PUT`  | `/cart/items/{cartItemId}` | `{ quantity }` | Cập nhật số lượng (quantity=0 sẽ xóa) | `[Buyer]` |
| `DELETE` | `/cart/items/{cartItemId}` | — | Xóa 1 món khỏi giỏ hàng | `[Buyer]` |
| `DELETE` | `/cart/items` | `{ cartItemIds: [] }` | Xóa nhiều món đã chọn cùng lúc | `[Buyer]` |

---

## 7. 📝 ĐẶT HÀNG & QUẢN LÝ ĐƠN HÀNG (ORDERS)

| Method | Endpoint | Payload / Params | Mô tả chức năng | Quyền |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/orders/calculate-checkout` | `{ cartItemIds: [], shippingAddressId, shopVouchers: [{ shopId, code }], platformProductVoucherCode?, platformFreeshipVoucherCode? }` | **Tính nháp Checkout Realtime** (trả về chi tiết từng shop, phí ship, voucher phân bổ) | `[Buyer]` |
| `POST` | `/orders/checkout` | `{ cartItemIds: [], shippingAddressId, shippingProviders: [{ shopId, provider, serviceCode }], shopVouchers: [{ shopId, code }], platformProductVoucherCode?, platformFreeshipVoucherCode?, paymentMethod: MoMo\|VietQR\|COD, note? }` | **Tạo đơn hàng chính thức** (Tách Parent + Sub-orders, reserve tồn kho) | `[Buyer]` |
| `GET`  | `/orders` | `?status=&from=&to=&page=&size=` | Lịch sử mua hàng của Buyer | `[Buyer]` |
| `GET`  | `/orders/{orderId}` | — | Chi tiết đơn hàng kèm Sub-orders và Timeline | `[Buyer]` |
| `POST` | `/orders/{orderId}/cancel` | `{ reason }` | Buyer hủy đơn (khi đơn chưa Confirmed) | `[Buyer]` |
| `GET`  | `/seller/orders` | `?status=&from=&to=&page=&size=` | Danh sách đơn hàng cần xử lý của Shop | `[Seller]` |
| `GET`  | `/seller/orders/{subOrderId}` | — | Chi tiết Sub-order của Shop | `[Seller]` |
| `PUT`  | `/seller/orders/{subOrderId}/confirm` | `{ warehouseAddressId }` | Seller xác nhận đơn & chọn kho lấy hàng | `[Seller]` |
| `POST` | `/seller/orders/{subOrderId}/cancel` | `{ reason }` | Seller hủy đơn (kèm lý do hết hàng/sự cố) | `[Seller]` |

---

## 8. 🎟️ VOUCHER & KHUYẾN MÃI (DISCOUNTS)

| Method | Endpoint | Payload / Params | Mô tả chức năng | Quyền |
| :--- | :--- | :--- | :--- | :--- |
| `GET`  | `/discounts/shop/{shopId}` | — | Danh sách Voucher đang phát hành của Shop | `[Public]` |
| `GET`  | `/discounts/platform` | — | Danh sách Voucher toàn sàn (SP & Freeship) | `[Public]` |
| `POST` | `/discounts/validate` | `{ code, shopId?, cartItemIds: [] }` | Kiểm tra tính hợp lệ và số tiền giảm của mã | `[Buyer]` |
| `GET`  | `/seller/discounts` | `?status=&page=&size=` | Danh sách Voucher do Shop tạo | `[Seller]` |
| `POST` | `/seller/discounts` | `{ code, name, discountType: PercentCart\|FixedCart, discountValue, minOrderAmount, maxDiscountAmount?, maxUses?, perUserLimit, appliesTo, targetIds?, validFrom, validTo, isPublic }` | Tạo Voucher giảm giá mới cho Shop | `[Seller]` |
| `PUT`  | `/seller/discounts/{id}` | `{ name, maxUses?, validTo, isActive }` | Chỉnh sửa hạn mức/thời hạn Voucher | `[Seller]` |
| `POST` | `/admin/discounts` | `{ code, name, discountType: PercentCart\|FixedCart\|PercentShip\|FreeShip, discountValue, minOrderAmount, maxDiscountAmount?, maxUses?, perUserLimit, validFrom, validTo, isPublic }` | Tạo Voucher toàn sàn do Sàn tài trợ | `[Admin]` |

---

## 9. ⚡ FLASH SALE CAMPAIGN

| Method | Endpoint | Payload / Params | Mô tả chức năng | Quyền |
| :--- | :--- | :--- | :--- | :--- |
| `GET`  | `/flash-sales/active` | — | Khung giờ Flash Sale đang diễn ra | `[Public]` |
| `GET`  | `/flash-sales/{campaignId}/products` | `?page=&size=` | Danh sách sản phẩm Flash Sale kèm % đã bán | `[Public]` |
| `POST` | `/seller/flash-sales/{campaignId}/register` | `{ skuId, flashPrice, quantity, perUserLimit }` | Seller đăng ký SKU tham gia Flash Sale | `[Seller]` |
| `GET`  | `/seller/flash-sales/registrations` | `?status=&page=&size=` | Danh sách SKU Shop đã gửi duyệt Flash Sale | `[Seller]` |
| `POST` | `/admin/flash-sales` | `{ name, startAt, endAt, bannerUrl? }` | Admin tạo khung giờ Flash Sale mới | `[Admin]` |
| `PUT`  | `/admin/flash-sales/items/{itemId}/approve` | — | Admin phê duyệt SKU vào Flash Sale | `[Admin]` |
| `PUT`  | `/admin/flash-sales/items/{itemId}/reject` | `{ reason }` | Admin từ chối SKU | `[Admin]` |

---

## 10. 💳 THANH TOÁN & WEBHOOKS

| Method | Endpoint | Payload / Params | Mô tả chức năng | Quyền |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/payments/initiate` | `{ orderId, method: MoMo\|VietQR }` | Khởi tạo giao dịch, nhận QR Code / PayUrl | `[Buyer]` |
| `GET`  | `/payments/{paymentId}/status` | — | Polling trạng thái thanh toán | `[Buyer]` |
| `POST` | `/payments/webhook/momo` | `(Payload webhook MoMo)` | Callback IPN từ cổng thanh toán MoMo | `[Public - HMAC Signature]` |
| `POST` | `/payments/webhook/vietqr` | `(Payload webhook VietQR)` | Callback IPN từ cổng thanh toán VietQR | `[Public - Signature]` |

---

## 11. 🚚 VẬN CHUYỂN & GIAO HÀNG (SHIPPING)

| Method | Endpoint | Payload / Params | Mô tả chức năng | Quyền |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/shipping/calculate-fee` | `{ shopId, warehouseAddressId, shippingAddressId, items: [{ skuId, quantity, weightGram }] }` | Tra cứu cước vận chuyển thời gian thực từ ĐVVC | `[Buyer/Seller]` |
| `POST` | `/seller/shipping/create-order` | `{ subOrderId, provider: GHN\|GHTK, serviceCode, pickupAddressId }` | Tạo đơn vận chuyển sang ĐVVC, nhận tracking code | `[Seller]` |
| `GET`  | `/seller/shipping/{subOrderId}/label` | — | Tải file PDF phiếu giao hàng (Shipping Label) | `[Seller]` |
| `GET`  | `/orders/{orderId}/tracking` | — | Lấy lộ trình vận đơn từ ĐVVC | `[Buyer/Seller]` |
| `POST` | `/shipping/webhook/ghn` | `(Payload webhook GHN)` | Nhận cập nhật trạng thái vận đơn từ GHN | `[Public - HMAC Signature]` |
| `POST` | `/shipping/webhook/ghtk` | `(Payload webhook GHTK)` | Nhận cập nhật trạng thái vận đơn từ GHTK | `[Public - Signature]` |

---

## 12. 🔄 HOÀN HÀNG & TRANH CHẤP (RETURNS & DISPUTES)

| Method | Endpoint | Payload / Params | Mô tả chức năng | Quyền |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/returns` | `{ subOrderId, reason, items: [{ orderItemId, quantity, reasonDetail? }], evidenceUrls: [] }` | Buyer gửi yêu cầu trả hàng / hoàn tiền | `[Buyer]` |
| `GET`  | `/returns` | `?status=&page=&size=` | Danh sách yêu cầu hoàn hàng của Buyer | `[Buyer]` |
| `GET`  | `/returns/{returnId}` | — | Xem chi tiết tiến trình khiếu nại | `[Buyer/Seller/Admin]` |
| `GET`  | `/seller/returns` | `?status=&page=&size=` | Danh sách yêu cầu hoàn hàng cần Shop xử lý | `[Seller]` |
| `PUT`  | `/seller/returns/{returnId}/approve` | `{ note? }` | Seller đồng ý nhận lại hàng hoàn | `[Seller]` |
| `PUT`  | `/seller/returns/{returnId}/reject` | `{ reason }` | Seller từ chối yêu cầu hoàn hàng | `[Seller]` |
| `PUT`  | `/seller/returns/{returnId}/received` | — | Seller xác nhận đã nhận lại hàng hoàn | `[Seller]` |
| `GET`  | `/admin/disputes` | `?page=&size=` | Danh sách tranh chấp leo thang lên Admin | `[Admin]` |
| `PUT`  | `/admin/disputes/{returnId}/resolve` | `{ decision: BuyerWins\|SellerWins, refundAmount?, note }` | Admin ra phán quyết cuối cùng giải quyết tranh chấp | `[Admin]` |

---

## 13. 📹 LIVESTREAM COMMERCE

| Method | Endpoint | Payload / Params | Mô tả chức năng | Quyền |
| :--- | :--- | :--- | :--- | :--- |
| `GET`  | `/livestreams/active` | `?page=&size=` | Danh sách các phiên đang phát trực tiếp | `[Public]` |
| `GET`  | `/livestreams/{sessionId}` | — | Chi tiết phiên live + Agora RTC Token xem live | `[Public]` |
| `GET`  | `/livestreams/{sessionId}/pinned-products` | — | Danh sách sản phẩm đang ghim trong live | `[Public]` |
| `POST` | `/seller/livestreams/start` | `{ title, thumbnailUrl?, warehouseAddressId }` | Khởi tạo phiên phát live & nhận Agora Publisher Token | `[Seller]` |
| `POST` | `/seller/livestreams/{sessionId}/end` | — | Kết thúc phiên live & lưu tổng kết doanh số | `[Seller]` |
| `POST` | `/seller/livestreams/{sessionId}/pin-product` | `{ skuId, flashPrice?, quantityLimit? }` | Ghim sản phẩm lên màn hình live (SignalR push) | `[Seller]` |
| `DELETE` | `/seller/livestreams/{sessionId}/pin-product/{skuId}` | — | Gỡ ghim sản phẩm | `[Seller]` |

---

## 14. ⭐ ĐÁNH GIÁ (REVIEWS)

| Method | Endpoint | Payload / Params | Mô tả chức năng | Quyền |
| :--- | :--- | :--- | :--- | :--- |
| `GET`  | `/products/{spuId}/reviews` | `?rating=&page=&size=` | Danh sách đánh giá sản phẩm | `[Public]` |
| `POST` | `/reviews` | `{ orderItemId, rating, title?, content?, mediaUrls: [] }` | Viết đánh giá sản phẩm sau khi nhận hàng | `[Buyer]` |
| `PUT`  | `/reviews/{reviewId}` | `{ rating, title?, content?, mediaUrls: [] }` | Chỉnh sửa đánh giá (cho phép 1 lần trong 30 ngày) | `[Buyer]` |
| `POST` | `/seller/reviews/{reviewId}/reply` | `{ replyText }` | Seller phản hồi đánh giá của khách hàng | `[Seller]` |
| `PUT`  | `/admin/reviews/{reviewId}/hide` | `{ reason }` | Admin ẩn đánh giá vi phạm chính sách | `[Admin]` |

---

## 15. 📊 QUẢN TRỊ ADMIN & BÁO CÁO

| Method | Endpoint | Payload / Params | Mô tả chức năng | Quyền |
| :--- | :--- | :--- | :--- | :--- |
| `GET`  | `/admin/shops` | `?status=&page=&size=` | Quản lý danh sách toàn bộ gian hàng | `[Admin]` |
| `PUT`  | `/admin/shops/{id}/approve` | `{ note? }` | Duyệt kích hoạt gian hàng mới | `[Admin]` |
| `PUT`  | `/admin/shops/{id}/ban` | `{ reason }` | Khóa gian hàng vi phạm | `[Admin]` |
| `GET`  | `/admin/reports/dashboard` | — | Thống kê GMV toàn sàn, số đơn, người dùng mới | `[Admin]` |
| `GET`  | `/admin/payouts` | `?status=&page=&size=` | Danh sách lệnh yêu cầu rút tiền của Seller | `[Admin]` |
| `PUT`  | `/admin/payouts/{id}/approve` | `{ transferRef }` | Xác nhận đã chuyển khoản ngân hàng cho Seller | `[Admin]` |
| `GET`  | `/seller/reports/dashboard` | `?from=&to=` | Thống kê doanh thu, đơn hàng của Shop | `[Seller]` |
| `POST` | `/upload/media` | `(multipart/form-data: file, type)` | Upload ảnh/video lên CDN MinIO S3 | `[Buyer/Seller]` |

---

## 16. ⚡ WEBSOCKET / SIGNALR REALTIME HUBS (`NovaLive.RealtimeApi`)

| Hub Path | Event / Action | Hướng truyền | Mô tả sự kiện Realtime |
| :--- | :--- | :---: | :--- |
| `wss://api.novalive.vn/hubs/livestream` | `JoinLiveSession(sessionId)` | Client $\rightarrow$ Server | Người xem tham gia phòng Live |
| | `SendMessage(content)` | Client $\rightarrow$ Server | Gửi bình luận trong phiên Live |
| | `SendReaction(type)` | Client $\rightarrow$ Server | Thả tim / biểu cảm |
| | `ReceiveMessage` | Server $\rightarrow$ Client | Nhận tin nhắn chat mới từ người xem khác |
| | `ReceiveReaction` | Server $\rightarrow$ Client | Nhận hiệu ứng tim bay realtime |
| | `ProductPinned` | Server $\rightarrow$ Client | Seller ghim sản phẩm kèm giá Flash khuyến mãi |
| | `ProductUnpinned` | Server $\rightarrow$ Client | Seller gỡ ghim sản phẩm |
| `wss://api.novalive.vn/hubs/order` | `OrderPlaced` | Server $\rightarrow$ Client | Bắn thông báo đơn mới tức thì cho Seller |
| | `OrderStatusChanged` | Server $\rightarrow$ Client | Bắn cập nhật trạng thái đơn cho Buyer |
| `wss://api.novalive.vn/hubs/payment` | `PaymentSuccess` | Server $\rightarrow$ Client | Bắn kết quả thanh toán QR MoMo/VietQR thành công |
