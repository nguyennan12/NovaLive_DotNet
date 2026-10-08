# SOFTWARE REQUIREMENTS SPECIFICATION (SRS) - NOVALIVE

> He thong: NovaLive E-commerce & Livestream Commerce Platform  
> Phien ban tai lieu: 1.0  
> Ngay cap nhat: 2026-10-07  
> Pham vi: Multi-vendor marketplace, livestream shopping, thanh toan truc tiep, van chuyen, admin va bao cao.

---

## 1. Gioi thieu

### 1.1 Muc dich

Tai lieu nay dac ta yeu cau phan mem cho he thong NovaLive. SRS dung lam co so thong nhat giua product, backend, frontend, QA, DevOps va stakeholder nghiep vu khi thiet ke, lap trinh, kiem thu va nghiem thu he thong.

Tai lieu nay khong thay the tai lieu kien truc, database hay API chi tiet. No tong hop cac yeu cau chuc nang, phi chuc nang, rang buoc nghiep vu, tich hop ngoai va tieu chi chap nhan cap he thong.

### 1.2 Pham vi san pham

NovaLive la nen tang thuong mai dien tu da nha ban ket hop livestream commerce. He thong cho phep:

- Buyer tim kiem san pham, them gio hang, checkout da shop, thanh toan va theo doi don hang.
- Seller mo shop, quan ly san pham, kho, don hang, voucher, flash sale, livestream va vi/doanh thu ban hang.
- Admin phe duyet shop, quan ly danh muc, flash sale, tranh chap, payout va bao cao san.
- System tu dong xu ly webhook, outbox event, ton kho, ghi nhan doanh thu truc tiep cho shop, tim kiem PostgreSQL va thong bao realtime.

### 1.3 Tai lieu lien quan

- `docs/ecommerce_system_architecture.md`
- `docs/ecommerce_database_design.md`
- `docs/ecommerce_business_logic.md`
- `docs/ecommerce_api_endpoints.md`

### 1.4 Thuat ngu

| Thuat ngu | Dinh nghia |
| :--- | :--- |
| Buyer | Nguoi mua hang tren nen tang. |
| Seller | Nguoi ban co shop da duoc phe duyet. |
| Admin | Quan tri vien toan san. |
| SPU | San pham cha, dai dien cho mot mat hang. |
| SKU | Bien the ban le cu the cua SPU. |
| ParentOrder | Don hang tong cua mot lan checkout. |
| SubOrder | Don hang con theo tung shop trong ParentOrder. |
| Direct Settlement | Co che thanh toan truc tiep ghi nhan doanh thu vao vi shop ngay khi quet/thanh toan thanh cong. |
| COD | Thanh toan tien mat khi nhan hang. |
| Outbox | Bang luu event trong cung DB transaction de publish bat dong bo len message bus. |
| ĐVVC | Don vi van chuyen: GHN, GHTK, ViettelPost. |

---

## 2. Mo ta tong quan

### 2.1 Boi canh he thong

NovaLive gom REST API, Realtime API, PostgreSQL, Redis, RabbitMQ, MinIO va Agora RTC. Product search su dung PostgreSQL Full-Text Search va `pg_trgm`.

### 2.2 Nhom nguoi dung

| Nhom nguoi dung | Mo ta | Muc tieu chinh |
| :--- | :--- | :--- |
| Guest | Chua dang nhap | Xem san pham, shop, livestream cong khai. |
| Buyer | Tai khoan active | Mua hang, thanh toan, theo doi don, review, hoan hang. |
| Seller | Buyer co shop active | Ban hang, quan ly kho, xu ly don, livestream, rut tien/doanh thu. |
| Admin | Quan tri vien | Kiem duyet, van hanh, tranh chap, payout, bao cao. |
| System | Tac vu tu dong | Xu ly webhook, job nen, outbox, doi soat doanh thu. |

### 2.3 Gia dinh va rang buoc

- Backend su dung .NET 10, ASP.NET Core Web API, PostgreSQL 17, Redis 7, RabbitMQ, MinIO, SignalR va Agora RTC.
- Authentication su dung JWT Bearer HS256, access token TTL 15 phut, refresh token TTL 30 ngay.
- Tat ca tien te luu bang `DECIMAL(18,2)` voi currency mac dinh `VND`.
- Tat ca ID chinh su dung `UUID`.
- Tim kiem san pham phai duoc xu ly bang PostgreSQL Full-Text Search + trigram index.
- Giao dich dat hang, giu cho ton kho, tao payment va ghi outbox phai nam trong mot database transaction.
- Cac bien dong tai chinh va ton kho phai co ledger append-only.

### 2.4 Phu thuoc ngoai

| Dich vu | Muc dich |
| :--- | :--- |
| MoMo | Thanh toan online va webhook IPN. |
| VietQR/OpenBanking | Chuyen khoan ngan hang dong va doi soat. |
| GHN/GHTK/ViettelPost | Tinh phi, tao van don, tracking, shipping webhook. |
| Agora RTC | Livestream video/audio. |
| MinIO | Luu tru anh, video, bang chung, media review. |
| RabbitMQ | Message bus cho event bat dong bo. |
| Redis | Cache, rate limit, JWT JTI blacklist, SignalR backplane. |

---

## 3. Yeu cau chuc nang

### 3.1 Auth & Identity

| ID | Yeu cau |
| :--- | :--- |
| FR-AUTH-001 | He thong phai cho phep dang ky tai khoan bang email hoac phone, password va full name. |
| FR-AUTH-002 | Tai khoan moi phai co `account_status = Unverified` cho den khi xac thuc OTP thanh cong. |
| FR-AUTH-003 | He thong phai gui OTP 6 chu so, TTL 5 phut, luu hash OTP trong `UserOtps`. |
| FR-AUTH-004 | Redis chi duoc dung cho rate limit/resend lock OTP va cac cache ngan han lien quan. |
| FR-AUTH-005 | He thong phai cho phep resend OTP voi gioi han 3 lan trong 15 phut. |
| FR-AUTH-006 | Dang nhap thanh cong phai tra ve access token, refresh token va thoi diem het han. |
| FR-AUTH-007 | Refresh token phai duoc hash SHA-256 truoc khi luu vao `RefreshTokens`. |
| FR-AUTH-008 | Khi refresh token, token cu phai bi revoke va cap cap token moi. |
| FR-AUTH-009 | Neu phat hien refresh token da revoke duoc dung lai, he thong phai revoke toan bo refresh token cua user. |
| FR-AUTH-010 | Logout va doi mat khau phai dua JWT `jti` vao Redis blacklist voi TTL bang thoi gian song con lai. |
| FR-AUTH-011 | He thong phai khoa tam tai khoan sau 5 lan dang nhap sai lien tiep theo co che backoff. |

### 3.2 User Profile, Address, Wishlist

| ID | Yeu cau |
| :--- | :--- |
| FR-USER-001 | Buyer phai xem va cap nhat ho so ca nhan gom full name, phone, birthday, gender va avatar. |
| FR-USER-002 | Buyer phai quan ly nhieu dia chi nhan hang. |
| FR-USER-003 | Moi Buyer duoc co mot dia chi mac dinh. |
| FR-USER-004 | He thong khong duoc cho xoa dia chi mac dinh neu Buyer van con dia chi khac chua duoc set default. |

### 3.3 Shop & Seller Onboarding

| ID | Yeu cau |
| :--- | :--- |
| FR-SHOP-001 | Buyer co `account_status = Active` phai gui duoc yeu cau mo shop. |
| FR-SHOP-002 | Moi user chi duoc so huu toi da mot shop. |
| FR-SHOP-003 | Yeu cau mo shop phai tao `Shops.status = Pending` va ho so `ShopVerifications`. |
| FR-SHOP-004 | Admin phai phe duyet hoac tu choi ho so KYC shop. |
| FR-SHOP-005 | Khi shop duoc phe duyet, he thong phai cap role Seller va tao `ShopWallets`. |
| FR-SHOP-006 | Admin phai tam ngung hoac ban shop vi pham. |
| FR-SHOP-007 | Shop bi `Suspended` hoac `Banned` khong duoc tao san pham moi, nhan don moi hoac hien trong ket qua tim kiem public. |
| FR-SHOP-008 | Seller phai quan ly nhieu dia chi kho lay hang/tra hang. |
| FR-SHOP-009 | Buyer phai follow/unfollow shop. |

### 3.4 Catalog, SKU, Inventory

| ID | Yeu cau |
| :--- | :--- |
| FR-CAT-001 | Admin phai quan ly cay danh muc da cap. |
| FR-CAT-002 | Seller phai tao SPU gom ten, mo ta, danh muc, thuong hieu, anh dai dien va cau hinh bien the. |
| FR-CAT-003 | Seller phai tao nhieu SKU cho mot SPU. |
| FR-CAT-004 | SKU phai co ma duy nhat trong pham vi shop. |
| FR-CAT-005 | Seller phai cap nhat gia, trong luong va trang thai active cua SKU. |
| FR-CAT-006 | Seller phai dieu chinh ton kho thu cong va he thong phai ghi `InventoryHistories`. |
| FR-CAT-007 | Ton kho kha dung phai bang `qty_on_hand - reserved_qty`. |
| FR-CAT-008 | `reserved_qty` khong duoc vuot `qty_on_hand`. |
| FR-CAT-009 | Khi checkout, he thong phai tang `reserved_qty`. |
| FR-CAT-010 | Khi thanh toan thanh cong, he thong phai giam ca `qty_on_hand` va `reserved_qty`. |
| FR-CAT-011 | Khi huy don hoac timeout thanh toan, he thong phai giam `reserved_qty` de hoan giu cho. |

### 3.5 Product Search

| ID | Yeu cau |
| :--- | :--- |
| FR-SEARCH-001 | Guest va Buyer phai tim kiem, loc va sap xep san pham public. |
| FR-SEARCH-002 | Tim kiem keyword phai dung PostgreSQL `tsvector` va `pg_trgm`. |
| FR-SEARCH-003 | Ket qua search phai chi tra ve SPU active, shop active va SKU con ban. |
| FR-SEARCH-004 | He thong phai cap nhat `search_vector` khi SPU/SKU/thuoc tinh lien quan thay doi. |
| FR-SEARCH-005 | API search phai ho tro pagination. |

### 3.6 Cart & Checkout

| ID | Yeu cau |
| :--- | :--- |
| FR-CART-001 | Moi Buyer phai co mot gio hang duy nhat. |
| FR-CART-002 | Buyer phai them SKU vao gio; neu SKU da ton tai thi cong don quantity. |
| FR-CART-003 | Buyer phai sua quantity, xoa tung item hoac xoa nhieu item. |
| FR-CART-004 | Checkout chi xu ly cac `cartItemIds` duoc chon. |
| FR-CHECKOUT-001 | He thong phai tinh checkout preview truoc khi dat hang. |
| FR-CHECKOUT-002 | Checkout preview phai gom nhom item theo shop. |
| FR-CHECKOUT-003 | Checkout preview phai tinh tien hang, phi ship, voucher shop, voucher san va `grand_total`. |
| FR-CHECKOUT-004 | Submit checkout phai tao mot ParentOrder va N SubOrders theo so shop. |
| FR-CHECKOUT-005 | OrderItems phai luu snapshot SKU, gia, discount va line total tai thoi diem dat hang. |
| FR-CHECKOUT-006 | Checkout phai tao Payment `Pending` cho ParentOrder. |
| FR-CHECKOUT-007 | Checkout thanh cong phai xoa dung cac cart item da dat, giu lai item chua chon. |
| FR-CHECKOUT-008 | Checkout phai ghi `OutboxMessage(OrderPlacedEvent)` trong cung transaction. |

### 3.7 Discount & Voucher

| ID | Yeu cau |
| :--- | :--- |
| FR-DISCOUNT-001 | He thong phai ho tro voucher shop. |
| FR-DISCOUNT-002 | He thong phai ho tro platform product voucher. |
| FR-DISCOUNT-003 | He thong phai ho tro platform freeship voucher. |
| FR-DISCOUNT-004 | He thong phai kiem tra valid_from, valid_to, max_uses, per_user_limit va min_order_amount. |
| FR-DISCOUNT-005 | Platform voucher phai duoc phan bo ty le ve tung SubOrder. |
| FR-DISCOUNT-006 | Discount snapshot phai duoc luu de doi soat va tinh refund. |
| FR-DISCOUNT-007 | Seller chi duoc quan ly voucher cua shop minh. |
| FR-DISCOUNT-008 | Admin duoc tao voucher toan san. |

### 3.8 Flash Sale

| ID | Yeu cau |
| :--- | :--- |
| FR-FS-001 | Admin phai tao campaign flash sale voi khung gio bat dau/ket thuc. |
| FR-FS-002 | Seller phai dang ky SKU tham gia flash sale. |
| FR-FS-003 | Admin phai approve/reject SKU dang ky. |
| FR-FS-004 | Gia flash sale phai thap hon gia goc toi thieu 10%. |
| FR-FS-005 | Khi buyer dat SKU flash sale, he thong phai reserve bang PostgreSQL conditional atomic update. |
| FR-FS-006 | He thong phai chan overselling trong moi tinh huong concurrency. |
| FR-FS-007 | Thanh toan thanh cong phai chuyen `reserved_qty` sang `sold_qty`. |
| FR-FS-008 | Huy don hoac timeout phai release `reserved_qty`. |
| FR-FS-009 | He thong phai enforce `per_user_limit`. |

### 3.9 Payment, COD, Wallet & Doanh thu Shop

| ID | Yeu cau |
| :--- | :--- |
| FR-PAY-001 | Buyer phai chon MoMo, VietQR hoac COD khi checkout. |
| FR-PAY-002 | MoMo/VietQR phai sinh payment URL hoac QR code de Buyer quet thanh toan. |
| FR-PAY-003 | Payment webhook phai duoc xac thuc chu ky truoc khi xu ly. |
| FR-PAY-004 | Payment online qua 15 phut chua thanh cong phai het han va kich hoat rollback ton kho. |
| FR-PAY-005 | Khi quet/thanh toan MoMo/VietQR thanh cong, he thong cap nhat Payment `Success` va cong truc tiep tien vao doanh thu / so du kha dung `balance` cua tung Shop. |
| FR-PAY-006 | COD phai cho phep SubOrder chuyen `Confirmed` ngay sau checkout; khi giao hang thanh cong va doi soat tien, he thong ghi nhan doanh thu vao vi shop. |
| FR-WALLET-001 | Shop wallet phai co `balance` (so du kha dung / doanh thu nhan truc tiep) va `locked_balance` (tien dang cho rut). |
| FR-WALLET-002 | Moi bien dong vi (nhan doanh thu don hang, rut tien, hoan tien) phai tao `ShopWalletTransactions`. |
| FR-WALLET-003 | Seller phai tao payout request neu amount hop le va du balance. |
| FR-WALLET-004 | Tao payout phai tru `balance` va cong `locked_balance`. |
| FR-WALLET-005 | Payout thanh cong phai giam `locked_balance`; payout fail phai hoan tien ve `balance`. |

### 3.10 Shipping & Fulfillment

| ID | Yeu cau |
| :--- | :--- |
| FR-SHIP-001 | He thong phai tinh phi ship theo shop, kho lay hang, dia chi nhan va item. |
| FR-SHIP-002 | Seller phai xac nhan SubOrder va chon kho lay hang. |
| FR-SHIP-003 | Seller phai tao van don voi GHN, GHTK hoac ViettelPost. |
| FR-SHIP-004 | He thong phai luu tracking code, provider order id va shipping fee. |
| FR-SHIP-005 | Buyer va Seller phai xem tracking don hang. |
| FR-SHIP-006 | He thong phai nhan webhook trang thai van chuyen tu ĐVVC. |
| FR-SHIP-007 | Delivered webhook phai cap nhat SubOrder delivered va mo quyen review. |
| FR-SHIP-008 | Background job phai polling cac don shipping qua han neu webhook bi miss. |

### 3.11 Return & Dispute

| ID | Yeu cau |
| :--- | :--- |
| FR-RETURN-001 | Buyer chi duoc tao return khi SubOrder `Delivered` va con trong 7 ngay. |
| FR-RETURN-002 | Return request phai gom reason, item, quantity va evidence media. |
| FR-RETURN-003 | Seller phai approve hoac reject return trong 3 ngay. |
| FR-RETURN-004 | Qua han hoac seller reject phai cho phep leo thang AdminDispute. |
| FR-RETURN-005 | Admin phai resolve dispute voi ket qua BuyerWins hoac SellerWins. |
| FR-RETURN-006 | BuyerWins phai refund dung so tien hop le tu doanh thu shop / tai khoan shop. |
| FR-RETURN-007 | SellerWins SubOrder giu nguyen trang thai hoan thanh. |

### 3.12 Livestream Commerce

| ID | Yeu cau |
| :--- | :--- |
| FR-LIVE-001 | Seller phai start/end livestream. |
| FR-LIVE-002 | Server phai cap Agora token theo role Publisher hoac Subscriber. |
| FR-LIVE-003 | Guest/Buyer phai xem danh sach live active va chi tiet phien live. |
| FR-LIVE-004 | SignalR LivestreamHub phai ho tro join session, chat, reaction va viewer count. |
| FR-LIVE-005 | Seller phai pin/unpin SKU trong live. |
| FR-LIVE-006 | ProductPinned phai broadcast realtime den viewer trong cung live session. |
| FR-LIVE-007 | Viewer phai mua ngay san pham pinned bang flow add-to-cart va checkout nhanh. |
| FR-LIVE-008 | He thong phai luu comment live va metadata san pham pinned. |

### 3.13 Review & Rating

| ID | Yeu cau |
| :--- | :--- |
| FR-REVIEW-001 | Buyer chi duoc review OrderItem da delivered. |
| FR-REVIEW-002 | Moi OrderItem chi duoc review mot lan. |
| FR-REVIEW-003 | Buyer chi duoc sua review mot lan trong 30 ngay. |
| FR-REVIEW-004 | Review phai ho tro anh/video dinh kem. |
| FR-REVIEW-005 | Seller duoc reply review mot lan. |
| FR-REVIEW-006 | Admin duoc an review vi pham. |
| FR-REVIEW-007 | Rating trung binh cua shop phai duoc tinh lai bat dong bo. |

### 3.14 Notification, Realtime, Event

| ID | Yeu cau |
| :--- | :--- |
| FR-NOTI-001 | He thong phai tao in-app notification cho cac su kien quan trong. |
| FR-NOTI-002 | Seller phai nhan thong bao realtime khi co don moi. |
| FR-NOTI-003 | Buyer phai nhan thong bao realtime khi payment thanh cong va order status thay doi. |
| FR-EVENT-001 | Cac event nghiep vu phai ghi vao Outbox truoc khi publish RabbitMQ. |
| FR-EVENT-002 | Outbox publisher phai retry khi publish fail va luu error_message. |
| FR-EVENT-003 | Consumer phai xu ly idempotent de tranh tac dung phu khi retry. |

### 3.15 Admin & Reports

| ID | Yeu cau |
| :--- | :--- |
| FR-ADMIN-001 | Admin phai xem danh sach shop theo status. |
| FR-ADMIN-002 | Admin phai approve, suspend, ban hoac close shop theo chinh sach. |
| FR-ADMIN-003 | Admin phai tao/sua/xoa danh muc. |
| FR-ADMIN-004 | Admin phai tao campaign flash sale va duyet item dang ky. |
| FR-ADMIN-005 | Admin phai xem va resolve dispute. |
| FR-ADMIN-006 | Admin phai xem va duyet payout. |
| FR-REPORT-001 | Admin dashboard phai hien GMV, so don, user moi, revenue phi san, ty le return/dispute. |
| FR-REPORT-002 | Seller dashboard phai hien doanh thu, so du vi, top san pham va hieu qua livestream. |

---

## 4. Yeu cau phi chuc nang

### 4.1 Bao mat

| ID | Yeu cau |
| :--- | :--- |
| NFR-SEC-001 | Tat ca API private phai yeu cau Bearer JWT hop le. |
| NFR-SEC-002 | JWT phai co `jti` va bi kiem tra voi Redis blacklist. |
| NFR-SEC-003 | Password phai duoc hash bang thuat toan manh nhu BCrypt/Argon2. |
| NFR-SEC-004 | OTP va refresh token khong duoc luu plain text. |
| NFR-SEC-005 | Webhook thanh toan/van chuyen phai xac thuc chu ky hoac secret. |
| NFR-SEC-006 | Admin action nhay cam phai ghi `AuditLogs`. |
| NFR-SEC-007 | Seller chi duoc truy cap tai nguyen thuoc shop cua minh. |
| NFR-SEC-008 | File upload phai kiem tra MIME type, size limit va quet noi dung nguy hiem neu co pipeline ho tro. |

### 4.2 Hieu nang

| ID | Yeu cau |
| :--- | :--- |
| NFR-PERF-001 | API public product listing/search nen co P95 <= 500ms voi cache/index phu hop. |
| NFR-PERF-002 | Checkout transaction nen hoan thanh P95 <= 1.5s, khong tinh thoi gian goi payment gateway. |
| NFR-PERF-003 | SignalR message trong livestream nen co latency noi bo P95 <= 300ms. |
| NFR-PERF-004 | Product search phai co GIN index cho `search_vector` va trigram index cho ten san pham. |
| NFR-PERF-005 | Bao cao lich su nang phai dung read replica hoac bang tong hop, khong anh huong write path. |

### 4.3 Tin cay va nhat quan du lieu

| ID | Yeu cau |
| :--- | :--- |
| NFR-REL-001 | Checkout phai atomic: neu bat ky buoc nao fail, ton kho, order, payment va cart khong duoc o trang thai nua voi. |
| NFR-REL-002 | Flash sale reserve phai chong overselling bang conditional update trong PostgreSQL. |
| NFR-REL-003 | Payment webhook phai idempotent theo transaction reference/payment id. |
| NFR-REL-004 | Shipping webhook phai idempotent theo provider, tracking code va event timestamp. |
| NFR-REL-005 | Outbox publisher phai retry va khong lam mat event. |
| NFR-REL-006 | Ledger tai chinh va ton kho khong duoc xoa vat ly. |

### 4.4 Kha nang mo rong va bao tri

| ID | Yeu cau |
| :--- | :--- |
| NFR-MAINT-001 | Codebase phai theo Clean Architecture: Domain, Application, Infrastructure, Api. |
| NFR-MAINT-002 | Use case phai duoc to chuc theo Command/Query va handler rieng. |
| NFR-MAINT-003 | Validation phai dung pipeline behavior hoac validator tap trung. |
| NFR-MAINT-004 | Adapter cho payment, shipping, storage, Agora phai di qua interface application layer. |
| NFR-MAINT-005 | Logic tien, ton kho phai co unit test rieng. |

### 4.5 Kha dung va van hanh

| ID | Yeu cau |
| :--- | :--- |
| NFR-OPS-001 | He thong phai chay duoc bang Docker Compose cho moi truong staging/production co ban. |
| NFR-OPS-002 | API phai expose health checks cho database, Redis, RabbitMQ va storage. |
| NFR-OPS-003 | Log phai co correlation id/request id. |
| NFR-OPS-004 | Metrics can co: request latency, error rate, queue depth, outbox pending, payment webhook fail, order timeout count. |
| NFR-OPS-005 | Background workers phai co co che retry, dead-letter hoac error tracking. |

---

## 5. Mo hinh du lieu cap cao

| Module | Entity chinh |
| :--- | :--- |
| Identity | Users, UserAddresses, UserOtps, RefreshTokens, UserRoles, Roles |
| Shop | Shops, ShopVerifications, ShopAddresses, ShopFollowers |
| Wallet | ShopWallets, ShopWalletTransactions, SellerPayouts |
| Catalog | Categories, Spus, Skus, SkuImages, ProductAttributes |
| Inventory | Inventories, InventoryHistories |
| Cart | Carts, CartItems |
| Order | ParentOrders, SubOrders, OrderItems, OrderStatusHistories |
| Discount | Discounts, DiscountUsages |
| Flash Sale | FlashSaleCampaigns, FlashSaleItems |
| Payment | Payments |
| Shipping | ShippingOrders |
| Livestream | LivestreamSessions, LivestreamProducts, LivestreamComments |
| Review | Reviews, ReviewImages |
| System | Notifications, AuditLogs, OutboxMessages |

---

## 6. Trang thai chinh

### 6.1 User

`Unverified -> Active -> Locked/Suspended -> Active`  
`Deleted` la trang thai ket thuc logic cho tai khoan da xoa/vo hieu hoa.

### 6.2 Shop

`Pending -> Active -> Suspended -> Active`  
`Active -> Banned/Closed`

### 6.3 SubOrder

`PendingPayment -> Confirmed -> Processing -> Shipping -> Delivered -> Completed`  
Nhanh phu: `Cancelled`, `ReturnRequested`, `Returned`, `Refunded`.

### 6.4 Payment

`Pending -> Success`  
Nhanh phu: `Pending -> Failed/Expired`, `Success -> Refunded`.

### 6.5 Return

`Pending -> SellerApproved -> Completed`  
`Pending -> SellerRejected -> AdminDispute -> AdminApproved/AdminRejected -> Completed`

---

## 7. Interface ngoai

### 7.1 REST API

Base URL du kien: `https://api.novalive.vn/api/v1`.

API phai duoc versioning theo `/api/v1`. Response loi phai theo dang ProblemDetails JSON. Pagination phai thong nhat cac tham so `page` va `size`.

### 7.2 Realtime API

SignalR hub paths:

- `/hubs/livestream`
- `/hubs/order`
- `/hubs/payment`

WebSocket authentication dung JWT trong query param `access_token` khi client khong gan duoc Authorization header.

### 7.3 Webhook

Webhook public bat buoc xac thuc chu ky:

- `/payments/webhook/momo`
- `/payments/webhook/vietqr`
- `/shipping/webhook/ghn`
- `/shipping/webhook/ghtk`
- `/shipping/webhook/viettelpost`

Webhook handler phai idempotent va khong duoc tin payload truoc khi verify.

---

## 8. Tieu chi chap nhan cap he thong

| ID | Tieu chi |
| :--- | :--- |
| AC-001 | Buyer co the dang ky, verify OTP, login, refresh token va logout. |
| AC-002 | Seller co the dang ky shop, duoc admin approve va tao san pham/SKU/ton kho. |
| AC-003 | Buyer co the search san pham bang keyword bang PostgreSQL Full-Text Search va trigram. |
| AC-004 | Buyer co the checkout item tu nhieu shop va he thong tao dung ParentOrder/SubOrders. |
| AC-005 | Checkout that bai giua chung khong de ton kho reserved sai hoac don hang nua voi. |
| AC-006 | Flash sale khong ban vuot so luong khi co nhieu request dong thoi. |
| AC-007 | Payment online (VietQR/MoMo) quet thanh cong cap nhat Payment `Success` va cong truc tiep vao doanh thu / vi Shop. |
| AC-008 | COD ghi nhan doanh thu seller sau khi don hang giao thanh cong va doi soat. |
| AC-009 | Return trong 7 ngay cho phep seller/admin xu ly va hoan tien hop le. |
| AC-010 | Seller payout phai lock balance va co ledger day du. |
| AC-011 | Livestream phai broadcast pin product/chat/reaction realtime cho viewer cung session. |
| AC-012 | Admin co the quan ly shop, flash sale, dispute, payout va dashboard. |

---

## 9. Yeu cau kiem thu toi thieu

- Unit test cho price calculator, discount proration, inventory reservation, wallet credit, payout locking.
- Integration test cho checkout transaction, payment webhook, shipping webhook, return/dispute.
- Concurrency test cho flash sale atomic reserve va checkout cung SKU.
- API contract test cho cac endpoint public/private quan trong.
- Security test cho JWT blacklist, refresh token reuse detection, webhook signature.
- Realtime test cho SignalR group join, broadcast va reconnect.

---

## 10. Ngoai pham vi phien ban nay

- He thong goi y ca nhan hoa nang cao.
- Search engine rieng ngoai PostgreSQL.
- Multi-currency.
- Multi-language UI.
- Affiliate/referral program.
- Native accounting ledger tich hop truc tiep ngan hang chi tiet hon payout/refund co ban.
