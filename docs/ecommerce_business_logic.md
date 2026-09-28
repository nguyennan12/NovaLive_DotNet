# 🛒 TÀI LIỆU BUSINESS LOGIC & RULES — HỆ THỐNG NOVALIVE (E-COMMERCE + LIVESTREAM)

> Tài liệu mô tả các quy tắc, điều kiện và luồng nghiệp vụ bắt buộc của hệ thống E-commerce Multi-vendor Marketplace kết hợp Livestream Shopping.
>
> Công nghệ: .NET 10, PostgreSQL 17, Redis, RabbitMQ, Agora RTC. Các vai trò: **Buyer**, **Seller**, **Admin**, **System**.

---

## 1. NGUYÊN TẮC CHUNG TOÀN HỆ THỐNG

### 1.1 Phân quyền & Vai trò (Roles)

| Vai trò | Phạm vi quyền & Trách nhiệm |
| :--- | :--- |
| `Admin` | Quản trị toàn sàn: Phê duyệt/Khóa shop, gỡ sản phẩm vi phạm, tạo chiến dịch Flash Sale toàn sàn, tạo Voucher sàn, phân xử tranh chấp hoàn tiền (Dispute), duyệt lệnh chi trả (Payout), xem Dashboard toàn sàn. |
| `Seller` | Quản trị gian hàng: Tạo/sửa sản phẩm (SPU/SKU), quản lý kho, xác nhận đơn hàng, tạo Voucher shop, đăng ký Flash Sale, phát Livestream & ghim sản phẩm, duyệt/từ chối yêu cầu hoàn hàng, phản hồi đánh giá. |
| `Buyer` | Mua sắm: Tìm kiếm/lọc sản phẩm, quản lý giỏ hàng, áp dụng Voucher 3 cấp, đặt đơn & thanh toán (MoMo, VietQR, COD), xem Livestream & mua trực tiếp, theo dõi vận đơn, đánh giá sản phẩm, khiếu nại/hoàn hàng. |
| `System` | Tác nhân tự động: Xử lý Webhook thanh toán/vận chuyển, tự động giải phóng tiền ký quỹ (Escrow Release T+7), chạy job hoàn kho timeout, đồng bộ tìm kiếm Elasticsearch, tính toán xếp hạng sao. |

### 1.2 Điều kiện & Ràng buộc cốt lõi

1. **Bảo mật & Token**: Sử dụng **JWT Bearer với thuật toán HMAC-SHA256 (HS256)**. Access Token có TTL 15 phút. Refresh Token có TTL 30 ngày (sử dụng cơ chế Rotation & Token Reuse Detection).
2. **Ký quỹ bảo vệ giao dịch (Escrow Model)**: Toàn bộ tiền thanh toán online được sàn giữ trong `PaymentEscrows`. Tiền chỉ được giải phóng (`Released`) và cộng vào ví Seller sau khi đơn hàng chuyển sang `Delivered` + hết thời hạn khiếu nại (7 ngày) mà không có tranh chấp.
3. **Checkout theo sản phẩm được chọn**: Buyer chỉ checkout những sản phẩm được tick chọn (`cartItemIds`) trong giỏ hàng. Các sản phẩm không được chọn vẫn giữ nguyên trong giỏ.
4. **Tách đơn đa Shop (Order Splitting)**: Một phiên checkout tạo ra 1 **Parent Order** (gom nhóm thanh toán) và N **Sub-Orders** tương ứng với N Shop có sản phẩm được chọn.
5. **Đơn vị tiền tệ**: Lưu trữ dạng số thực độ chính xác cao `DECIMAL(18,2)` hoặc số nguyên VNĐ, không dùng kiểu số thực dấu phẩy động (`FLOAT/REAL`) để tránh sai số làm tròn.
6. **Snapshot bất biến**: Thông tin SKU (`sku_snapshot_json`), giá bán, voucher áp dụng và địa chỉ giao hàng (`shipping_address_json`) được snapshot cố định tại thời điểm đặt hàng.
7. **Append-only Ledgers**: Không xóa vật lý dữ liệu tài chính, đơn hàng và tồn kho. Mọi biến động kho ghi vào `InventoryHistories`, biến động tiền ghi vào `ShopWalletTransactions` và `AuditLogs`.

---

## 2. MODULE AUTH & IDENTITY (XÁC THỰC & PHÂN QUYỀN)

### 2.1 Đăng ký & Kích hoạt tài khoản
- Người dùng đăng ký bằng `email` hoặc `phone` + `password` $\rightarrow$ Tài khoản ở trạng thái `Unverified`.
- Hệ thống gửi mã OTP 6 chữ số (TTL 5 phút, lưu hash trong Redis `otp:{email}`).
- Nhập đúng OTP $\rightarrow$ Tài khoản chuyển sang `Active`.
- Giới hạn: Sau 5 lần nhập sai mật khẩu liên tiếp, tài khoản bị tạm khóa theo backoff: 5 phút $\rightarrow$ 15 phút $\rightarrow$ 1 giờ $\rightarrow$ 24 giờ.

### 2.2 Đăng nhập, Token Rotation & Thu hồi (Revocation)
- Đăng nhập thành công trả về: `accessToken` (HS256, payload chứa `userId`, `roles[]`, `shopId?`, `jti`), `refreshToken` (ngẫu nhiên 64 bytes) và `expiresAt`.
- Hash SHA-256 của Refresh Token được lưu trong bảng `RefreshTokens` gắn với `user_id`, `device_info`.
- **Token Rotation**: Khi gọi `/auth/refresh`, Refresh Token cũ bị thu hồi ngay lập tức (`revoked_at = NOW()`) và cấp cặp Access/Refresh Token mới.
- **Token Reuse Detection**: Nếu phát hiện một Refresh Token đã bị thu hồi (`revoked_at != NULL`) được gửi lên, hệ thống phát hiện hành vi tấn công chiếm đoạt token $\rightarrow$ Thu hồi toàn bộ Refresh Tokens của User đó và bắt đăng nhập lại.
- **Đăng xuất / Đổi mật khẩu**: Đưa `jti` của Access Token vào Redis Blacklist với TTL bằng thời gian sống còn lại; đánh dấu `revoked_at` cho Refresh Token.

---

## 3. MODULE SHOP & VÍ NHÀ BÁN HÀNG (SHOP & WALLET)

### 3.1 Vòng đời Shop
- Buyer có tài khoản `Active` được gửi yêu cầu đăng ký mở Shop $\rightarrow$ Tạo `Shops` (status `Pending`) và `ShopVerifications` (CCCD, GPKD, STK ngân hàng).
- Admin phê duyệt $\rightarrow$ `Shops.status = Active`, User được cấp Role `Seller`, khởi tạo `ShopWallets` với số dư 0.
- Nếu Shop vi phạm chính sách $\rightarrow$ Admin chuyển `Suspended` hoặc `Banned` $\rightarrow$ Toàn bộ sản phẩm của Shop bị ẩn khỏi tìm kiếm, chặn tạo đơn hàng mới.
- Một User chỉ được sở hữu tối đa 1 Shop (quan hệ 1-1).

### 3.2 Ví Shop & Tuần hoàn tài chính (`ShopWallets`)
- **Số dư khả dụng (`balance`)**: Tiền Seller có thể rút về tài khoản ngân hàng.
- **Số dư đóng băng (`holding_balance`)**: Tiền từ các đơn hàng online đang trong thời gian Escrow chờ giải phóng.
- **Luồng cộng tiền Escrow**: Khi đơn online hết thời hạn 7 ngày $\rightarrow$ Escrow giải phóng $\rightarrow$ `balance += (held_amount - platform_commission)`.
- **Luồng trừ phí sàn đơn COD**: Đơn COD giao thành công $\rightarrow$ Tiền mặt do shipper thu, hệ thống tự động trừ phí hoa hồng sàn (`platform_commission`) trực tiếp vào `ShopWallets.balance`. Nếu số dư âm vượt hạn mức, Shop bị tạm ngừng nhận đơn mới.
- **Lệnh rút tiền (`SellerPayouts`)**: Seller tạo yêu cầu rút tiền $\rightarrow$ Admin duyệt/Hệ thống tự động chuyển khoản qua cổng liên ngân hàng $\rightarrow$ Trừ `balance`.

---

## 4. MODULE SẢN PHẨM & QUẢN LÝ TỒN KHO (CATALOG & INVENTORY)

### 4.1 Cấu trúc SPU & SKU 2 cấp
- **SPU (Standard Product Unit)**: Đại diện cho sản phẩm cha (Tên, Mô tả, Danh mục, Thương hiệu, Ảnh đại diện, Cấu hình trục biến thể: `[{"name":"Màu","values":["Đỏ","Xanh"]},{"name":"Size","values":["S","M"]}]`).
- **SKU (Stock Keeping Unit)**: Đại diện cho phân loại bán lẻ cụ thể (Mã SKU duy nhất trong shop, Tổ hợp thuộc tính Descartes `{"Màu":"Đỏ","Size":"S"}`, Giá gốc `original_price`, Giá bán `sell_price`, Trọng lượng `weight_gram`, Ảnh SKU).

### 4.2 Cơ chế quản lý tồn kho 2 trạng thái
- Tồn kho được quản lý ở cấp độ từng SKU trong bảng `Inventories`:
  $$\text{Tồn kho khả dụng (Available)} = \text{qty\_on\_hand} - \text{reserved\_qty}$$
- **Khi tạo đơn hàng / Checkout**: Tăng `reserved_qty`, giảm lượng khả dụng.
- **Khi đơn hàng thanh toán thành công / Giao hàng**: Trừ thực tế $\text{qty\_on\_hand} = \text{qty\_on\_hand} - \text{qty}$, đồng thời giảm $\text{reserved\_qty} = \text{reserved\_qty} - \text{qty}$.
- **Khi đơn bị Hủy / Timeout thanh toán**: Hoàn trả $\text{reserved\_qty} = \text{reserved\_qty} - \text{qty}$.
- Mọi biến động đều sinh bản ghi `InventoryHistories` (Append-only Ledger) ghi nhận loại thay đổi: `ReserveAdd`, `ReserveRelease`, `SaleConfirmed`, `ReturnIn`, `ManualAdjust`.

---

## 5. MODULE GIỎ HÀNG & CHECKOUT ĐA SHOP

### 5.1 Giỏ hàng Đa Shop
- Mỗi Buyer sở hữu 1 giỏ hàng duy nhất (`Carts`).
- `CartItems` lưu danh sách sản phẩm thuộc nhiều shop khác nhau, có snapshot giá tại thời điểm thêm vào giỏ.
- Cho phép Buyer cập nhật số lượng, xóa từng món, hoặc xóa hàng loạt các món đã chọn sau khi checkout.

### 5.2 Tính toán Checkout Nháp (Calculate Checkout Preview)
- Trước khi bấm đặt hàng, Frontend gọi API tính nháp `/orders/calculate-checkout` gửi kèm `{ cartItemIds[], addressId, vouchers[] }`.
- Server thực hiện tính toán thời gian thực:
  1. Gom nhóm sản phẩm được chọn theo `shop_id`.
  2. Tính tiền hàng từng shop: $\text{Subtotal}_{\text{shop}} = \sum (\text{unit\_price} \times \text{quantity})$.
  3. Tra cứu cước vận chuyển từng shop dựa trên địa chỉ kho shop và địa chỉ nhận của Buyer.
  4. Kiểm tra và áp dụng Voucher 3 cấp (Shop Voucher, Sàn SP, Sàn Freeship).
  5. Trả về bảng phân rã chi tiết tiền cho từng Sub-order và tổng thanh toán `grand_total`.

### 5.3 Luồng Đặt Đơn (Submit Checkout)
1. **Kiểm tra tồn kho**: Kiểm tra lượng khả dụng của từng SKU được chọn.
2. **Khóa giữ chỗ (Inventory Reservation)**: Tăng `reserved_qty` trong DB.
3. **Sinh đơn hàng phân cấp**:
   * Tạo 1 bản ghi vào bảng **`ParentOrders`** quản lý tổng tiền `grand_total`, thanh toán và địa chỉ nhận hàng snapshot.
   * Tạo N bản ghi vào bảng **`SubOrders`** (mỗi SubOrder tương ứng 1 Shop) quản lý sản phẩm, vận chuyển, hoa hồng và voucher riêng của từng Shop.
4. **Khởi tạo Thanh toán & Ký quỹ**: Tạo `Payments` (trỏ tới `parent_order_id`) ở trạng thái `Pending` và tạo `PaymentEscrows` (trỏ tới từng `sub_order_id`) ở trạng thái `Holding`.
5. **Xóa giỏ hàng**: Xóa đúng các `cartItemIds` đã được đặt hàng, giữ lại các món chưa chọn.
6. **Publish Event**: Bắn sự kiện `OrderPlacedEvent` lên RabbitMQ để gửi thông báo cho Buyer và các Seller liên quan.

---

## 6. MODULE KHUYẾN MÃI & VOUCHER 3 CẤP (DISCOUNT ENGINE)

### 6.1 Phân cấp Voucher
Hệ thống hỗ trợ áp dụng đồng thời 3 tầng voucher trong 1 lần checkout:
1. **Tầng 1: Shop Voucher (`Discounts.shop_id = Shop.Id`)**: Do Seller tài trợ, giảm giá trực tiếp vào tiền hàng của Shop đó.
2. **Tầng 2: Platform Product Voucher (`Discounts.shop_id = NULL`, `discount_type IN ('PercentCart','FixedCart')`)**: Do sàn tài trợ, giảm giá trên tổng tiền hàng toàn sàn, được phân bổ tỷ lệ về từng Sub-order.
3. **Tầng 3: Platform FreeShipping Voucher (`Discounts.shop_id = NULL`, `discount_type = 'FreeShip'`)**: Do sàn tài trợ, giảm trừ trực tiếp vào phí vận chuyển của các Sub-order.

### 6.2 Công thức phân bổ Voucher Sàn (Proration)
- Khi áp Voucher Sàn giảm $V$ đồng cho đơn gồm các Sub-order $S_1, S_2, ..., S_n$:
  $$\text{Tiền giảm cho Sub-order } S_i = V \times \frac{\text{Subtotal}_{S_i}}{\sum \text{Subtotal}}$$
- Số tiền giảm được ghi nhận snapshot vào từng `OrderItems.discount_amount` để phục vụ đối soát và hoàn tiền nếu có khiếu nại.
- Khi có hoàn hàng 1 phần: Buyer chỉ được hoàn đúng số tiền thực trả sau khi đã trừ phần voucher phân bổ.

---

## 7. MODULE FLASH SALE (POSTGRESQL ATOMIC)

### 7.1 Quy trình chiến dịch
1. **Admin** tạo Campaign Flash Sale (khung giờ `start_at` $\rightarrow$ `end_at`, trạng thái `Scheduled`).
2. **Seller** đăng ký SKU tham gia kèm giá Flash Sale (thấp hơn giá gốc $\ge 10\%$) và số lượng mở bán (`quantity`).
3. **Admin duyệt** $\rightarrow$ SKU chuyển trạng thái `Approved`.
4. **Active Campaign**: Khi đến khung giờ `start_at`, Campaign chuyển `Active`. Hệ thống cache danh sách sản phẩm và giá Flash Sale lên Redis để phục vụ truy vấn đọc (Read-heavy) với tốc độ cao.

### 7.2 Atomic Reserve với PostgreSQL Condition UPDATE
Để ngăn chặn hoàn toàn tình trạng Bán vượt tồn kho (Overselling) khi có hàng ngàn lượt tranh mua đồng thời:
- Khi Buyer bấm đặt hàng SKU Flash Sale, hệ thống thực thi câu lệnh UPDATE nguyên tử trực tiếp trong cơ sở dữ liệu:
```sql
UPDATE FlashSaleItems
SET reserved_qty = reserved_qty + @request_qty
WHERE id = @flashSaleItemId
  AND status = 'Approved'
  AND (quantity - (reserved_qty + sold_qty)) >= @request_qty;
```
- **Kiểm tra kết quả**:
  - `affected_rows == 1`: Reserve thành công $\rightarrow$ Cho phép chuyển sang bước tạo đơn hàng và thanh toán.
  - `affected_rows == 0`: Hết hàng Flash Sale hoặc không đủ số lượng yêu cầu $\rightarrow$ Báo lỗi ngay lập tức mà không làm sai lệch số liệu tồn kho.
- **Thanh toán thành công**:
```sql
UPDATE FlashSaleItems
SET reserved_qty = reserved_qty - @request_qty,
    sold_qty = sold_qty + @request_qty
WHERE id = @flashSaleItemId;
```
- **Hủy đơn / Hết hạn thanh toán 15 phút**:
```sql
UPDATE FlashSaleItems
SET reserved_qty = reserved_qty - @request_qty
WHERE id = @flashSaleItemId;
```
- **Giới hạn số lượng mua / người (`per_user_limit`)**: Kiểm tra tổng số lượng SKU đã mua trong đơn hàng thành công và đơn hàng đang giữ chỗ của User trước khi thực hiện UPDATE.

---

## 8. MODULE NGUỒN TIỀN, THANH TOÁN & ESCROW KÝ QUỸ

### 8.1 Nguồn tiền thanh toán đầu vào từ Buyer (Funding Sources Inflow)
Khi Buyer đặt đơn hàng, dòng tiền đầu vào được nạp vào hệ thống qua 3 kênh chính:
1. **Thanh toán Online (MoMo / Thẻ ATM / Visa / Mastercard)**:
   - Hệ thống sinh mã thanh toán / Payment URL kèm `idempotency_key`.
   - Tiền từ tài khoản Buyer được chuyển trực tiếp vào **Tài khoản Merchant của Sàn NovaLive** tại Cổng thanh toán.
   - Webhook IPN từ cổng thanh toán được xác thực chữ ký số HMAC-SHA256 $\rightarrow$ Cập nhật `Payments.status = Success`.
   - Timeout: Sau 15 phút không nhận được IPN thanh toán $\rightarrow$ Đơn hàng tự động hủy, giải phóng tồn kho `reserved_qty`.
2. **Chuyển khoản Ngân hàng (VietQR Động)**:
   - Hệ thống sinh mã VietQR động chứa nội dung chuyển khoản duy nhất (`NOVA_{order_id}`).
   - Buyer dùng app ngân hàng quét mã $\rightarrow$ Tiền chuyển thẳng vào **Tài khoản Ngân hàng Doanh nghiệp của Sàn NovaLive**.
   - Hệ thống đối soát biến động số dư qua Webhook/OpenBanking $\rightarrow$ Ghi nhận thanh toán tức thì.
3. **Tiền mặt COD (Cash On Delivery - Thanh toán khi nhận hàng)**:
   - Khi chọn COD, đơn hàng được tạo và tự động chuyển ngay sang trạng thái **`Confirmed`** để Seller đóng gói giao hàng (không bắt buộc nhập OTP, tối ưu trải nghiệm checkout 1 chạm).
   - `ParentOrders.payment_status` được khởi tạo là `Pending` (chờ thu tiền khi giao).
   - **Đường đi của dòng tiền COD**: Buyer giao tiền mặt cho Shipper (GHN/GHTK) khi nhận hàng (`Delivered`) $\rightarrow$ Shipper nộp về bưu cục ĐVVC $\rightarrow$ ĐVVC định kỳ (T+3 hoặc hàng tuần) làm lệnh **Đối soát COD** và chuyển khoản tiền tổng về Tài khoản Ngân hàng của Sàn NovaLive $\rightarrow$ `ParentOrders.payment_status` chuyển thành `Paid`.

---

### 8.2 Mô hình Két sắt Ký quỹ Trung gian (Escrow Custodian Model)
> ⚠️ **Nguyên tắc cốt lõi**: Toàn bộ tiền Buyer thanh toán (Online, VietQR, hoặc COD do ĐVVC chuyển về) đều do **Sàn NovaLive đứng tên nắm giữ trung gian** tại tài khoản ngân hàng của Sàn. **Tuyệt đối KHÔNG chuyển thẳng tiền cho Seller ngay khi thanh toán**.

- Khi thanh toán thành công, hệ thống tạo bản ghi **`PaymentEscrows`** tương ứng cho từng Sub-Order ở trạng thái **`Holding` (Tạm giữ)**.
- **Mục đích bảo vệ 2 chiều**:
  - *Bảo vệ Buyer*: Nếu hàng giả, hư hỏng hoặc Shop không giao $\rightarrow$ Sàn chủ động hoàn tiền ngay từ quỹ Escrow mà không phụ thuộc vào việc Shop có đồng ý hay không.
  - *Bảo vệ Seller*: Đảm bảo Seller chắc chắn nhận được tiền sau khi giao hàng thành công và hết hạn khiếu nại.
- **Thời hạn tạm giữ (Hold Duration)**: Bắt đầu từ lúc giao hàng thành công (`Delivered`) và kéo dài **7 ngày (T+7)** để chờ hết thời hạn khiếu nại/đổi trả của Buyer.

---

### 8.3 Công thức Bóc tách Dòng tiền & Quyết toán Đơn hàng (Financial Settlement)
Khi Buyer thanh toán một đơn hàng:
$$\text{Tổng tiền Buyer trả} = \text{Tiền hàng (Subtotal)} + \text{Phí Ship} - \text{Voucher Shop} - \text{Voucher Sàn}$$

Khi đơn hàng kết thúc thành công (sau T+7 ngày không phát sinh khiếu nại), hệ thống tự động bóc tách và phân bổ dòng tiền:

| Dòng tiền phân bổ | Công thức tính | Nguồn chi trả / Thụ hưởng |
| :--- | :--- | :--- |
| 🚚 **Cước vận chuyển** | $=\text{Phí ship thực tế}$ | Trả cho ĐVVC (GHN / GHTK). |
| 🏦 **Doanh thu phí Sàn** | $=\text{Tiền hàng} \times \text{Commission Rate (vd: 5\%)}$ | Thu về tài khoản doanh thu của Sàn NovaLive. |
| 💳 **Phí cổng thanh toán** | $=\text{Tổng giá trị thanh toán} \times 1.5\%$ | Trả cho Cổng thanh toán (MoMo/Ngân hàng). |
| 🎁 **Trợ giá khuyến mại Sàn** | $=\text{Giá trị Voucher do Sàn phát hành}$ | Sàn NovaLive tự bù tiền túi vào đơn hàng cho Seller. |
| 💰 **Tiền thực nhận của Seller** | **$=\text{Tiền hàng} - \text{Phí sàn} - \text{Phí cổng TT} - \text{Voucher Shop tự giảm}$** | Cộng vào Số dư khả dụng trong ví **`ShopWallets.balance`**. |

---

### 8.4 Vòng đời Ký quỹ Escrow & Xử lý Tranh chấp
$$\text{Pending Payment} \xrightarrow{\text{Buyer trả tiền}} \text{Holding (T+7)} \xrightarrow{\text{Giao xong + Hết T+7}} \text{Released (Cộng ví Seller)}$$
- Khi Buyer tạo yêu cầu trả hàng / khiếu nại $\rightarrow$ `PaymentEscrows` chuyển sang trạng thái **`Disputed` (Đóng băng tranh chấp)**.
- **Kết quả phán quyết**:
  - **Buyer thắng**: `PaymentEscrows.status = Refunded` $\rightarrow$ Hệ thống hoàn tiền từ tài khoản Sàn về phương thức thanh toán gốc / ví của Buyer.
  - **Seller thắng**: `PaymentEscrows.status = Released` $\rightarrow$ Giải phóng tiền ký quỹ và cộng vào `ShopWallets.balance` của Seller.

---

### 8.5 Dòng tiền đầu ra & Rút tiền của Người bán (Seller Payouts Outflow)
1. Sau khi Escrow chuyển `Released`, tiền được ghi nhận vào `ShopWallets.balance` (Số dư khả dụng) của Shop.
2. Seller gửi yêu cầu rút tiền (`SellerPayouts`):
   - Nhập số tiền muốn rút (phải $\le \text{ShopWallets.balance}$ và $\ge \text{Rút tối thiểu (vd: 50.000đ)}$).
   - Chọn tài khoản ngân hàng thụ hưởng (đã được KYC xác minh chính chủ ở bảng `ShopVerifications`).
3. **Thực thi chuyển tiền**:
   - `ShopWallets.balance` bị trừ ngay lập tức, `ShopWallets.locked_balance` tăng tương ứng để chống double-spending.
   - Admin/Kế toán duyệt (hoặc hệ thống tự động gọi API Chi hộ ngân hàng) $\rightarrow$ Tiền chuyển từ Tài khoản Ngân hàng của Sàn về Tài khoản Ngân hàng của Seller $\rightarrow$ `SellerPayouts.status = Completed`.

---

## 9. MODULE VẬN CHUYỂN & FULFILLMENT (SHIPPING)

### 9.1 Tích hợp đơn vị vận chuyển (GHN / GHTK / ViettelPost)
- Seller có thể cấu hình nhiều địa chỉ kho lấy hàng (`ShopAddresses`).
- Khi Seller bấm "Xác nhận & Giao hàng":
  1. Hệ thống gọi API ĐVVC tạo vận đơn lấy hàng (Pickup request).
  2. Nhận lại `tracking_code` và lưu vào `ShippingOrders`.
  3. Cung cấp API tải/in Phiếu giao hàng (Shipping Label PDF) theo chuẩn của ĐVVC.
- Sub-Order chuyển trạng thái: `Confirmed` $\rightarrow$ `Shipping`.

### 9.2 Đồng bộ trạng thái vận chuyển
- ĐVVC gửi Webhook cập nhật tiến trình: `ReadyToPick` $\rightarrow$ `Picking` $\rightarrow$ `Delivering` $\rightarrow$ `Delivered` $\rightarrow$ `Failed/Returned`.
- Khi Webhook báo `Delivered`:
  1. Cập nhật `SubOrder.status = Delivered`, `delivered_at = NOW()`.
  2. Bắt đầu đếm ngược thời hạn khiếu nại 7 ngày (`hold_until = NOW() + 7 days`).
  3. Mở quyền viết Đánh giá (Review) cho Buyer.
- **Polling Fallback**: Background Job quét định kỳ 6 tiếng/lần các đơn `Shipping` quá 3 ngày để chủ động tra cứu trạng thái, phòng trường hợp Webhook của ĐVVC bị miss.

---

## 10. MODULE HOÀN HÀNG & TRANH CHẤP (RETURN & DISPUTE)

### 10.1 Điều kiện & Quy trình 3 bước
1. **Bước 1: Buyer gửi yêu cầu hoàn hàng (`OrderReturns`)**:
   - Điều kiện: Sub-Order ở trạng thái `Delivered` và còn trong vòng **7 ngày**.
   - Cung cấp: Lý do (`WrongItem`, `Defective`, `DamagedInShipping`, `ChangeOfMind`), số lượng hoàn từng món trong `OrderReturnItems`, và ảnh/video bằng chứng.
   - Escrow chuyển trạng thái `Disputed`.
2. **Bước 2: Seller phản hồi (Thời hạn 3 ngày)**:
   - **Đồng ý (`SellerApproved`)**: Buyer gửi hàng về địa chỉ Shop $\rightarrow$ Seller nhận được hàng và bấm "Xác nhận nhận hàng hoàn" $\rightarrow$ Hoàn tiền Buyer, cộng lại kho hàng còn tốt.
   - **Từ chối (`SellerRejected`)**: Seller nêu rõ lý do từ chối kèm ảnh/video đối chứng.
3. **Bước 3: Leo thang Tranh chấp lên Admin (`AdminDispute`)**:
   - Nếu Seller từ chối hoặc quá 3 ngày không phản hồi $\rightarrow$ Đơn tự động chuyển lên Admin phân xử.
   - Admin kiểm tra bằng chứng của 2 bên và ra phán quyết cuối cùng trong 3 ngày làm việc:
     * **Buyer thắng**: Hoàn tiền từ Escrow cho Buyer, ghi nhận lỗi vào điểm uy tín của Shop.
     * **Seller thắng**: Hủy khiếu nại, giải phóng Escrow chuyển tiền cho Seller.

---

## 11. MODULE LIVESTREAM COMMERCE (AGORA RTC)

### 11.1 Kiến trúc phiên Live
- **Agora RTC SDK**: Xử lý luồng Video/Audio độ trễ siêu thấp (<1s). Server ASP.NET Core đóng vai trò cấp phát `AgoraToken` an toàn theo role `Publisher` (Seller) hoặc `Subscriber` (Viewer).
- **SignalR WebSockets (`LivestreamHub`)**:
  * Phát sự kiện realtime: Pin sản phẩm (`ProductPinned`), Đếm người xem (`ViewerCountUpdated`), Reaction/Thả tim theo batch (`LikesUpdated`), Bình luận (`NewComment`).
  * Giữ buffer 100 bình luận mới nhất trong bộ nhớ cache để client tải mượt mà.

### 11.2 Ghim sản phẩm & Mua ngay (Instant Buy)
- Seller ghim sản phẩm lên màn hình live kèm **Giá Flash trong Stream** (có thời hạn đếm ngược 60s - 300s và số lượng giới hạn).
- **Luồng Mua Ngay (Instant Buy)**:
  * Viewer bấm "Mua ngay" trên thẻ sản phẩm ghim $\rightarrow$ Hệ thống tự động thêm SKU vào giỏ, gán `selectedIds = [skuId]` và chuyển thẳng đến màn hình Checkout (bỏ qua trang giỏ hàng).
  * Nếu sự kiện ghim chưa kịp tải `skuId`, hệ thống tự động fallback lấy `sku_default` từ danh sách sản phẩm trong phiên live đã nạp sẵn.

---

## 12. MODULE ĐÁNH GIÁ & XẾP HẠNG (REVIEW & RATING)

### 12.1 Ràng buộc đánh giá
- Chỉ Buyer đã mua sản phẩm và Sub-Order đã chuyển sang `Delivered` mới được viết review.
- Mỗi dòng sản phẩm trong đơn (`order_item_id`) chỉ được đánh giá **1 lần duy nhất** (Ràng buộc `UNIQUE (order_item_id)`).
- Buyer được phép **chỉnh sửa đánh giá 1 lần duy nhất trong vòng 30 ngày** kể từ khi gửi.
- Seller có quyền phản hồi công khai 1 lần duy nhất cho mỗi đánh giá của khách hàng.
- Admin có quyền ẩn các đánh giá vi phạm thuần phong mỹ tục, spam hoặc bôi nhọ đối thủ.

### 12.2 Tính toán điểm sao trung bình
- Điểm đánh giá sao trung bình của SKU và SPU = $\frac{\sum \text{rating}}{\text{count(reviews)}}$ (chỉ tính các review đang hiển thị `is_visible = TRUE`).
- Để tối ưu hiệu năng ghi, việc tính toán lại sao trung bình của Shop được thực hiện bất đồng bộ qua Message Queue / Background Worker định kỳ mỗi 5 phút, không khóa dòng bảng `Shops`.

---

## 13. MODULE BÁO CÁO & DASHBOARD THỐNG KÊ

### 13.1 Phân cấp Báo cáo
1. **Admin Platform Dashboard**:
   - GMV (Gross Merchandise Value) toàn sàn theo ngày/tuần/tháng.
   - Doanh thu hoa hồng thực nhận của sàn (Net Revenue = $\sum \text{platform\_fee}$ từ các Escrow đã `Released`).
   - Tỷ lệ hoàn hàng & Tỷ lệ tranh chấp toàn sàn.
   - Thống kê hiệu quả chuyển đổi từ Livestream và Flash Sale.
2. **Seller Shop Dashboard**:
   - Doanh thu đơn hàng đã giao thành công của Shop.
   - Số dư ví Shop khả dụng và số dư đang chờ Escrow giải phóng.
   - Top 10 sản phẩm bán chạy nhất.
   - Thống kê doanh thu phát sinh từ các buổi Livestream của Shop.

### 13.2 Tối ưu hiệu năng truy vấn
- Các chỉ số thời gian thực trong ngày được tổng hợp qua **Redis Counters**.
- Các báo cáo lịch sử chuyên sâu, biểu đồ xu hướng được thực hiện trên **PostgreSQL Read Replica** hoặc các bảng tổng hợp định kỳ (`DailyShopStats`), hoàn toàn tách biệt khỏi đường ghi (Write path) của giao dịch đặt hàng.
