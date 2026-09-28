# 🛒 THIẾT KẾ DATABASE — HỆ THỐNG NOVALIVE (PostgreSQL 17)

> Tài liệu thiết kế cơ sở dữ liệu quan hệ cho hệ thống E-commerce Multi-vendor Marketplace & Livestream Shopping.
>
> Chuẩn hóa 3NF, tích hợp Ký quỹ Escrow, Flash Sale Atomic UPDATE, Quản lý ví Shop và Agora RTC.
> Kiểu định danh (PK): `UUID` toàn bộ. Tiền tệ: `DECIMAL(18,2)`. Thời gian: `TIMESTAMPTZ` (UTC).

---

## 1. SƠ ĐỒ TỔNG QUAN 15 NHÓM BẢNG

```
1.  CORE          : Users, UserAddresses, UserOtps, RefreshTokens       (4 bảng)
2.  SHOP & WALLET : Shops, ShopVerifications, ShopAddresses,            (5 bảng)
                    ShopWallets, ShopWalletTransactions
3.  RBAC          : Roles, Resources, Permissions,                      (5 bảng)
                    RolePermissions, UserRoles
4.  PRODUCT       : Categories, Spus, Skus, SkuImages, ProductAttributes(5 bảng)
5.  INVENTORY     : Inventories, InventoryHistories                     (2 bảng)
6.  CART          : Carts, CartItems                                    (2 bảng)
7.  ORDER         : ParentOrders, SubOrders, OrderItems,                (6 bảng)
                    OrderStatusHistories, OrderReturns, OrderReturnItems
8.  DISCOUNT      : Discounts, DiscountUsages                           (2 bảng)
9.  FLASH_SALE    : FlashSaleCampaigns, FlashSaleItems                  (2 bảng)
10. PAYMENT       : Payments, PaymentEscrows, SellerPayouts             (3 bảng)
11. SHIPPING      : ShippingOrders                                      (1 bảng)
12. LIVESTREAM    : LivestreamSessions, LivestreamProducts,             (3 bảng)
                    LivestreamComments
13. REVIEW        : Reviews, ReviewImages                               (2 bảng)
14. NOTIFICATION  : Notifications                                       (1 bảng)
15. SYSTEM & EDA  : AuditLogs, OutboxMessages                           (2 bảng)
───────────────────────────────────────────────────────────────────────────────
TỔNG CỘNG: 45 BẢNG
```

---

## 2. CHI TIẾT SCHEMA TỪNG NHÓM BẢNG

### 2.1 Nhóm CORE (Xác thực & Người dùng)

```sql
-- 1. Users: Tài khoản người dùng (vừa là Buyer, vừa có thể mở Shop làm Seller)
CREATE TABLE Users (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email             VARCHAR(255) NOT NULL,
    phone             VARCHAR(20) NULL,
    password_hash     VARCHAR(255) NOT NULL,
    full_name         VARCHAR(200) NOT NULL,
    avatar_url        VARCHAR(500) NULL,
    is_seller         BOOLEAN NOT NULL DEFAULT FALSE,
    is_verified       BOOLEAN NOT NULL DEFAULT FALSE,
    is_active         BOOLEAN NOT NULL DEFAULT TRUE,
    deleted_at        TIMESTAMPTZ NULL,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at        TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE UNIQUE INDEX UX_users_email ON Users(email) WHERE deleted_at IS NULL;
CREATE UNIQUE INDEX UX_users_phone ON Users(phone) WHERE phone IS NOT NULL AND deleted_at IS NULL;

-- 2. UserAddresses: Sổ địa chỉ nhận hàng của Buyer (Hỗ trợ nhiều địa chỉ, 1 địa chỉ mặc định)
CREATE TABLE UserAddresses (
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id        UUID NOT NULL REFERENCES Users(id) ON DELETE CASCADE,
    recipient_name VARCHAR(150) NOT NULL,
    phone          VARCHAR(20) NOT NULL,
    province_id    INT NULL,              -- Mã Tỉnh/Thành theo chuẩn ĐVVC (GHN/GHTK) để tính cước
    province_name  VARCHAR(100) NOT NULL,
    district_id    INT NULL,              -- Mã Quận/Huyện theo chuẩn ĐVVC
    district_name  VARCHAR(100) NOT NULL,
    ward_code      VARCHAR(20) NULL,      -- Mã Phường/Xã theo chuẩn ĐVVC
    ward_name      VARCHAR(100) NOT NULL,
    detail_address VARCHAR(300) NOT NULL, -- Số nhà, tên đường, tòa nhà
    is_default     BOOLEAN NOT NULL DEFAULT FALSE,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at     TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_useraddresses_user ON UserAddresses(user_id);

-- 3. UserOtps: Mã OTP xác thực email, số điện thoại, quên mật khẩu
CREATE TABLE UserOtps (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id     UUID NOT NULL REFERENCES Users(id) ON DELETE CASCADE,
    otp_type    VARCHAR(30) NOT NULL CHECK (otp_type IN ('EmailVerify','PhoneVerify','ForgotPassword','Login2FA')),
    otp_hash    VARCHAR(255) NOT NULL, -- Hash SHA-256 hoặc HMAC, không lưu plain text
    expires_at  TIMESTAMPTZ NOT NULL,
    used_at     TIMESTAMPTZ NULL,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_userotps_user_type ON UserOtps(user_id, otp_type, expires_at);

-- 4. RefreshTokens: Quản lý Refresh Token theo thiết bị, hỗ trợ Token Rotation & Reuse Detection
CREATE TABLE RefreshTokens (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id     UUID NOT NULL REFERENCES Users(id) ON DELETE CASCADE,
    token_hash  VARCHAR(255) NOT NULL UNIQUE, -- SHA-256(token)
    device_info VARCHAR(500) NULL,           -- User-Agent / Device fingerprint
    ip_address  VARCHAR(45) NULL,
    expires_at  TIMESTAMPTZ NOT NULL,
    revoked_at  TIMESTAMPTZ NULL,            -- Set khi logout / refresh / token reuse
    created_at  TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_refreshtokens_user ON RefreshTokens(user_id, revoked_at);
```

---

### 2.2 Nhóm SHOP & VÍ NHÀ BÁN HÀNG (Shop & Wallet)

```sql
-- 4. Shops: Thông tin gian hàng của Seller
CREATE TABLE Shops (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    owner_id        UUID NOT NULL UNIQUE REFERENCES Users(id) ON DELETE RESTRICT,
    shop_name       VARCHAR(200) NOT NULL,
    slug            VARCHAR(200) NOT NULL UNIQUE,
    description     TEXT NULL,
    logo_url        VARCHAR(500) NULL,
    banner_url      VARCHAR(500) NULL,
    tax_code        VARCHAR(30) NULL,
    phone           VARCHAR(20) NULL,
    email           VARCHAR(255) NULL,
    rating_avg      DECIMAL(3,2) NOT NULL DEFAULT 0.00,
    rating_count    INT NOT NULL DEFAULT 0,
    status          VARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (status IN ('Pending','Active','Suspended','Closed')),
    deleted_at      TIMESTAMPTZ NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_shops_status ON Shops(status) WHERE deleted_at IS NULL;

-- 5. ShopVerifications: Hồ sơ KYC xác thực gian hàng
CREATE TABLE ShopVerifications (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    shop_id          UUID NOT NULL REFERENCES Shops(id) ON DELETE CASCADE,
    submitted_by     UUID NOT NULL REFERENCES Users(id) ON DELETE RESTRICT,
    id_card_front    VARCHAR(500) NOT NULL,
    id_card_back     VARCHAR(500) NOT NULL,
    business_license VARCHAR(500) NULL,
    bank_account     VARCHAR(30) NOT NULL,
    bank_name        VARCHAR(100) NOT NULL,
    bank_branch      VARCHAR(200) NULL,
    status           VARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (status IN ('Pending','Approved','Rejected')),
    reviewer_id      UUID NULL REFERENCES Users(id) ON DELETE SET NULL,
    reviewer_note    TEXT NULL,
    reviewed_at      TIMESTAMPTZ NULL,
    submitted_at     TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_shopverif_shop ON ShopVerifications(shop_id);
CREATE INDEX idx_shopverif_pending ON ShopVerifications(status) WHERE status = 'Pending';

-- 7. ShopAddresses: Địa chỉ kho lấy hàng / trả hàng của Shop (hỗ trợ nhiều kho)
CREATE TABLE ShopAddresses (
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    shop_id        UUID NOT NULL REFERENCES Shops(id) ON DELETE CASCADE,
    warehouse_name VARCHAR(150) NOT NULL, -- vd: "Kho Tổng Hà Nội", "Kho Tân Bình"
    contact_name   VARCHAR(150) NOT NULL,
    contact_phone  VARCHAR(20) NOT NULL,
    province_id    INT NULL,              -- Mã Tỉnh/Thành theo chuẩn ĐVVC (GHN/GHTK)
    province_name  VARCHAR(100) NOT NULL,
    district_id    INT NULL,              -- Mã Quận/Huyện theo chuẩn ĐVVC
    district_name  VARCHAR(100) NOT NULL,
    ward_code      VARCHAR(20) NULL,      -- Mã Phường/Xã theo chuẩn ĐVVC
    ward_name      VARCHAR(100) NOT NULL,
    detail_address VARCHAR(300) NOT NULL,
    is_primary     BOOLEAN NOT NULL DEFAULT FALSE, -- Kho lấy hàng mặc định
    is_return      BOOLEAN NOT NULL DEFAULT FALSE, -- Địa chỉ nhận hàng hoàn
    created_at     TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_shopaddresses_shop ON ShopAddresses(shop_id);

-- 7. ShopWallets: Quản lý số dư tài chính của Seller (Số dư khả dụng + Số dư giữ chân Escrow)
CREATE TABLE ShopWallets (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    shop_id         UUID NOT NULL UNIQUE REFERENCES Shops(id) ON DELETE RESTRICT,
    balance         DECIMAL(18,2) NOT NULL DEFAULT 0.00,        -- Số dư có thể rút
    holding_balance DECIMAL(18,2) NOT NULL DEFAULT 0.00,        -- Đang giữ ở Escrow
    currency        VARCHAR(3) NOT NULL DEFAULT 'VND',
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);

-- 8. ShopWalletTransactions: Sổ cái biến động số dư ví Shop (Append-only)
CREATE TABLE ShopWalletTransactions (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    wallet_id       UUID NOT NULL REFERENCES ShopWallets(id) ON DELETE RESTRICT,
    type            VARCHAR(30) NOT NULL CHECK (type IN ('EscrowRelease','CodCommissionDeduct','PayoutWithdrawal','PenaltyDeduct','ManualAdjustment')),
    amount          DECIMAL(18,2) NOT NULL, -- Âm nếu trừ, Dương nếu cộng
    balance_before  DECIMAL(18,2) NOT NULL,
    balance_after   DECIMAL(18,2) NOT NULL,
    ref_type        VARCHAR(30) NULL,       -- 'Order', 'PaymentEscrow', 'SellerPayout'
    ref_id          UUID NULL,
    description     TEXT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_wallettx_wallet ON ShopWalletTransactions(wallet_id, created_at);
CREATE INDEX idx_wallettx_ref ON ShopWalletTransactions(ref_type, ref_id);
```

---

### 2.3 Nhóm RBAC (Phân quyền)

```sql
-- 9. Roles
CREATE TABLE Roles (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(500) NULL,
    is_system   BOOLEAN NOT NULL DEFAULT FALSE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);

-- 10. Resources
CREATE TABLE Resources (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code        VARCHAR(80) NOT NULL UNIQUE,
    description VARCHAR(300) NULL
);

-- 11. Permissions
CREATE TABLE Permissions (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    resource_id UUID NOT NULL REFERENCES Resources(id) ON DELETE CASCADE,
    action      VARCHAR(30) NOT NULL CHECK (action IN ('Create','Read','Update','Delete','Approve','Export','Override','Suspend')),
    code        VARCHAR(120) NOT NULL UNIQUE,
    description VARCHAR(300) NULL,
    UNIQUE (resource_id, action)
);

-- 12. RolePermissions
CREATE TABLE RolePermissions (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    role_id       UUID NOT NULL REFERENCES Roles(id) ON DELETE CASCADE,
    permission_id UUID NOT NULL REFERENCES Permissions(id) ON DELETE CASCADE,
    granted_at    TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    UNIQUE (role_id, permission_id)
);

-- 13. UserRoles
CREATE TABLE UserRoles (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id    UUID NOT NULL REFERENCES Users(id) ON DELETE CASCADE,
    role_id    UUID NOT NULL REFERENCES Roles(id) ON DELETE CASCADE,
    granted_at TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    UNIQUE (user_id, role_id)
);
CREATE INDEX idx_userroles_user ON UserRoles(user_id);
```

---

### 2.4 Nhóm PRODUCT (Sản phẩm SPU & Biến thể SKU)

```sql
-- 14. Categories: Danh mục sản phẩm đa cấp
CREATE TABLE Categories (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    parent_id     UUID NULL REFERENCES Categories(id) ON DELETE RESTRICT,
    name          VARCHAR(200) NOT NULL,
    slug          VARCHAR(200) NOT NULL UNIQUE,
    icon_url      VARCHAR(500) NULL,
    display_order INT NOT NULL DEFAULT 0,
    is_visible    BOOLEAN NOT NULL DEFAULT TRUE,
    created_at    TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_categories_parent ON Categories(parent_id);

-- 15. Spus: Sản phẩm chuẩn cha (Standard Product Unit)
CREATE TABLE Spus (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    shop_id           UUID NOT NULL REFERENCES Shops(id) ON DELETE RESTRICT,
    category_id       UUID NOT NULL REFERENCES Categories(id) ON DELETE RESTRICT,
    name              VARCHAR(300) NOT NULL,
    description       TEXT NULL,
    brand             VARCHAR(150) NULL,
    thumbnail_url     VARCHAR(500) NULL,
    attributes_config JSONB NULL, -- Cấu hình trục biến thể: [{"name":"Màu","values":["Đỏ","Xanh"]},{"name":"Size","values":["S","M"]}]
    status            VARCHAR(20) NOT NULL DEFAULT 'Draft' CHECK (status IN ('Draft','Active','Inactive','Banned')),
    deleted_at        TIMESTAMPTZ NULL,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at        TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_spus_shop_status ON Spus(shop_id, status) WHERE deleted_at IS NULL;
CREATE INDEX idx_spus_category ON Spus(category_id) WHERE deleted_at IS NULL;

-- 16. Skus: Biến thể cụ thể bán ra (Stock Keeping Unit)
CREATE TABLE Skus (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    spu_id          UUID NOT NULL REFERENCES Spus(id) ON DELETE RESTRICT,
    shop_id         UUID NOT NULL REFERENCES Shops(id) ON DELETE RESTRICT,
    sku_code        VARCHAR(100) NOT NULL,
    attributes_json JSONB NULL, -- Giá trị biến thể: {"Màu":"Đỏ","Size":"L"}
    original_price  DECIMAL(18,2) NOT NULL CHECK (original_price >= 0),
    sell_price      DECIMAL(18,2) NOT NULL CHECK (sell_price >= 0),
    weight_gram     INT NOT NULL DEFAULT 200 CHECK (weight_gram > 0),
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    deleted_at      TIMESTAMPTZ NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    UNIQUE (shop_id, sku_code)
);
CREATE INDEX idx_skus_spu ON Skus(spu_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_skus_shop_active ON Skus(shop_id, is_active) WHERE deleted_at IS NULL;

-- 17. SkuImages: Ảnh chi tiết biến thể
CREATE TABLE SkuImages (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sku_id        UUID NOT NULL REFERENCES Skus(id) ON DELETE CASCADE,
    image_url     VARCHAR(500) NOT NULL,
    is_primary    BOOLEAN NOT NULL DEFAULT FALSE,
    display_order INT NOT NULL DEFAULT 0,
    created_at    TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_skuimages_sku ON SkuImages(sku_id);

-- 18. ProductAttributes: Thuộc tính kỹ thuật mở rộng của SPU (Chất liệu, Xuất xứ, v.v.)
CREATE TABLE ProductAttributes (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    spu_id        UUID NOT NULL REFERENCES Spus(id) ON DELETE CASCADE,
    attr_name     VARCHAR(100) NOT NULL,
    attr_value    VARCHAR(300) NOT NULL,
    display_order INT NOT NULL DEFAULT 0
);
CREATE INDEX idx_productattributes_spu ON ProductAttributes(spu_id);
```

---

### 2.5 Nhóm INVENTORY (Tồn kho 2 trạng thái & Sổ cái Ledger)

```sql
-- 19. Inventories: Tồn kho hiện tại của SKU
CREATE TABLE Inventories (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sku_id        UUID NOT NULL UNIQUE REFERENCES Skus(id) ON DELETE RESTRICT,
    shop_id       UUID NOT NULL REFERENCES Shops(id) ON DELETE RESTRICT,
    qty_on_hand   INT NOT NULL DEFAULT 0 CHECK (qty_on_hand >= 0),   -- Tồn kho vật lý
    reserved_qty  INT NOT NULL DEFAULT 0 CHECK (reserved_qty >= 0),  -- Tồn kho đang giữ chỗ checkout
    min_stock     INT NOT NULL DEFAULT 5,                            -- Ngưỡng cảnh báo sắp hết
    last_updated  TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    CHECK (reserved_qty <= qty_on_hand)
);
CREATE INDEX idx_inventories_shop ON Inventories(shop_id);

-- 20. InventoryHistories: Sổ cái biến động tồn kho (Append-only)
CREATE TABLE InventoryHistories (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    inventory_id    UUID NOT NULL REFERENCES Inventories(id) ON DELETE RESTRICT,
    sku_id          UUID NOT NULL REFERENCES Skus(id) ON DELETE RESTRICT,
    change_type     VARCHAR(30) NOT NULL CHECK (change_type IN ('Import','SaleConfirmed','ReturnIn','ManualAdjust','ReserveAdd','ReserveRelease')),
    qty_before      INT NOT NULL,
    qty_change      INT NOT NULL, -- Âm = giảm, Dương = tăng
    reserved_before INT NOT NULL,
    reserved_change INT NOT NULL DEFAULT 0,
    qty_after       INT NOT NULL,
    ref_type        VARCHAR(30) NULL, -- 'Order', 'OrderReturn', 'FlashSale', 'Manual'
    ref_id          UUID NULL,
    note            TEXT NULL,
    created_by      UUID NULL REFERENCES Users(id) ON DELETE SET NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_invhist_sku ON InventoryHistories(sku_id, created_at);
CREATE INDEX idx_invhist_ref ON InventoryHistories(ref_type, ref_id);
```

---

### 2.6 Nhóm CART (Giỏ hàng)

```sql
-- 21. Carts: Giỏ hàng của Buyer
CREATE TABLE Carts (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id    UUID NOT NULL UNIQUE REFERENCES Users(id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);

-- 22. CartItems: Từng món hàng trong giỏ
CREATE TABLE CartItems (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cart_id    UUID NOT NULL REFERENCES Carts(id) ON DELETE CASCADE,
    sku_id     UUID NOT NULL REFERENCES Skus(id) ON DELETE CASCADE,
    shop_id    UUID NOT NULL REFERENCES Shops(id) ON DELETE CASCADE,
    quantity   INT NOT NULL CHECK (quantity > 0),
    unit_price DECIMAL(18,2) NOT NULL CHECK (unit_price >= 0), -- Snapshot giá lúc thêm vào giỏ
    added_at   TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    UNIQUE (cart_id, sku_id)
);
CREATE INDEX idx_cartitems_cart_shop ON CartItems(cart_id, shop_id);
```

---

### 2.7 Nhóm ORDER (Đơn hàng tách biệt ParentOrders & SubOrders)

```sql
-- 24. ParentOrders: Đơn hàng thanh toán tổng của Buyer (Quản lý toàn bộ giỏ checkout)
CREATE TABLE ParentOrders (
    id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    buyer_id              UUID NOT NULL REFERENCES Users(id) ON DELETE RESTRICT,
    order_code            VARCHAR(50) NOT NULL UNIQUE, -- vd: "NOVA-20260928-8X9Y"
    shipping_address_json JSONB NOT NULL,              -- Snapshot địa chỉ nhận hàng tại thời điểm checkout
    total_item_amount     DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (total_item_amount >= 0),
    total_shipping_fee    DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (total_shipping_fee >= 0),
    total_discount_amount DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (total_discount_amount >= 0),
    grand_total           DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (grand_total >= 0),
    currency_code         VARCHAR(3) NOT NULL DEFAULT 'VND',
    payment_status        VARCHAR(30) NOT NULL DEFAULT 'Pending' 
                          CHECK (payment_status IN ('Pending','Paid','PartiallyRefunded','FullyRefunded','Cancelled')),
    note                  TEXT NULL,
    created_at            TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at            TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_parentorders_buyer ON ParentOrders(buyer_id, created_at);

-- 25. SubOrders: Đơn hàng theo từng Shop (Quản lý kho, vận chuyển, hoa hồng và trạng thái fulfillment)
CREATE TABLE SubOrders (
    id                       UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    parent_order_id          UUID NOT NULL REFERENCES ParentOrders(id) ON DELETE RESTRICT,
    shop_id                  UUID NOT NULL REFERENCES Shops(id) ON DELETE RESTRICT,
    sub_order_code           VARCHAR(50) NOT NULL UNIQUE, -- vd: "NOVA-20260928-8X9Y-S1"
    warehouse_address_id     UUID NULL REFERENCES ShopAddresses(id) ON DELETE SET NULL,
    order_source             VARCHAR(20) NOT NULL DEFAULT 'Online' CHECK (order_source IN ('Online','Livestream','FlashSale')),
    item_amount              DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (item_amount >= 0),
    shipping_fee             DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (shipping_fee >= 0),
    shop_discount_amount     DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (shop_discount_amount >= 0),
    platform_discount_amount DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (platform_discount_amount >= 0),
    sub_total                DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (sub_total >= 0),
    seller_earnings          DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (seller_earnings >= 0), -- Số tiền Shop thực nhận (sau khi trừ phí sàn)
    status                   VARCHAR(30) NOT NULL DEFAULT 'PendingPayment' 
                             CHECK (status IN ('PendingPayment','Confirmed','Processing','Shipping','Delivered','Completed','Cancelled','ReturnRequested','Returned','Refunded')),
    cancel_reason            TEXT NULL,
    cancelled_by             UUID NULL REFERENCES Users(id) ON DELETE SET NULL,
    confirmed_at             TIMESTAMPTZ NULL,
    shipped_at               TIMESTAMPTZ NULL,
    delivered_at             TIMESTAMPTZ NULL,
    completed_at             TIMESTAMPTZ NULL,
    cancelled_at             TIMESTAMPTZ NULL,
    created_at               TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at               TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_suborders_parent ON SubOrders(parent_order_id);
CREATE INDEX idx_suborders_shop_status ON SubOrders(shop_id, status);

-- 26. OrderItems: Chi tiết dòng sản phẩm trong Sub-Order
CREATE TABLE OrderItems (
    id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sub_order_id          UUID NOT NULL REFERENCES SubOrders(id) ON DELETE CASCADE,
    sku_id                UUID NOT NULL REFERENCES Skus(id) ON DELETE RESTRICT,
    sku_snapshot_json     JSONB NOT NULL, -- Snapshot: { sku_code, spu_name, attributes, thumbnail_url }
    quantity              INT NOT NULL CHECK (quantity > 0),
    unit_price            DECIMAL(18,2) NOT NULL CHECK (unit_price >= 0),
    original_price        DECIMAL(18,2) NOT NULL CHECK (original_price >= 0),
    discount_amount       DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (discount_amount >= 0),
    line_total            DECIMAL(18,2) NOT NULL CHECK (line_total >= 0),
    status                VARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (status IN ('Active','Returned','PartialReturn'))
);
CREATE INDEX idx_orderitems_suborder ON OrderItems(sub_order_id);
CREATE INDEX idx_orderitems_sku ON OrderItems(sku_id);

-- 27. OrderStatusHistories: Lịch sử chuyển trạng thái Sub-Order (Audit trail)
CREATE TABLE OrderStatusHistories (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sub_order_id    UUID NOT NULL REFERENCES SubOrders(id) ON DELETE CASCADE,
    from_status     VARCHAR(30) NULL,
    to_status       VARCHAR(30) NOT NULL,
    changed_by      UUID NULL REFERENCES Users(id) ON DELETE SET NULL,
    changed_by_role VARCHAR(20) NULL CHECK (changed_by_role IN ('Buyer','Seller','Admin','System')),
    note            TEXT NULL,
    changed_at      TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_orderhist_suborder ON OrderStatusHistories(sub_order_id, changed_at);

-- 28. OrderReturns: Yêu cầu hoàn hàng / Trả hàng từ Buyer cho 1 Sub-Order
CREATE TABLE OrderReturns (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sub_order_id    UUID NOT NULL REFERENCES SubOrders(id) ON DELETE RESTRICT,
    buyer_id        UUID NOT NULL REFERENCES Users(id) ON DELETE RESTRICT,
    reason          VARCHAR(50) NOT NULL CHECK (reason IN ('WrongItem','Defective','DamagedInShipping','NotAsDescribed','ChangeOfMind')),
    evidence_urls   TEXT[] NOT NULL DEFAULT '{}',
    status          VARCHAR(30) NOT NULL DEFAULT 'Pending' 
                    CHECK (status IN ('Pending','SellerApproved','SellerRejected','AdminDispute','AdminApproved','AdminRejected','Completed')),
    refund_amount   DECIMAL(18,2) NOT NULL CHECK (refund_amount >= 0),
    seller_response TEXT NULL,
    admin_note      TEXT NULL,
    resolved_by     UUID NULL REFERENCES Users(id) ON DELETE SET NULL,
    resolved_at     TIMESTAMPTZ NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_orderreturns_suborder ON OrderReturns(sub_order_id);
CREATE INDEX idx_orderreturns_status ON OrderReturns(status) WHERE status IN ('Pending','AdminDispute');

-- 29. OrderReturnItems: Chi tiết từng món trong yêu cầu hoàn
CREATE TABLE OrderReturnItems (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    return_id     UUID NOT NULL REFERENCES OrderReturns(id) ON DELETE CASCADE,
    order_item_id UUID NOT NULL REFERENCES OrderItems(id) ON DELETE RESTRICT,
    quantity      INT NOT NULL CHECK (quantity > 0),
    reason_detail TEXT NULL
);
```

---

### 2.8 Nhóm DISCOUNT (Khuyến mãi & Voucher 3 cấp)

```sql
-- 30. Discounts: Mã giảm giá / Voucher (Shop Voucher hoặc Platform Voucher)
CREATE TABLE Discounts (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    shop_id             UUID NULL REFERENCES Shops(id) ON DELETE CASCADE, -- NULL = Voucher Toàn Sàn của Admin
    created_by          UUID NOT NULL REFERENCES Users(id) ON DELETE RESTRICT,
    code                VARCHAR(50) NOT NULL UNIQUE,
    name                VARCHAR(200) NOT NULL,
    discount_type       VARCHAR(20) NOT NULL CHECK (discount_type IN ('PercentCart','FixedCart','PercentShip','FreeShip')),
    discount_value      DECIMAL(18,2) NOT NULL CHECK (discount_value > 0),
    min_order_amount    DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (min_order_amount >= 0),
    max_discount_amount DECIMAL(18,2) NULL CHECK (max_discount_amount > 0), -- Giới hạn tiền giảm tối đa của Percent
    max_uses            INT NULL CHECK (max_uses > 0),                      -- Tổng số lượt dùng toàn sàn
    used_count          INT NOT NULL DEFAULT 0 CHECK (used_count >= 0),
    per_user_limit      INT NOT NULL DEFAULT 1 CHECK (per_user_limit > 0),
    applies_to          VARCHAR(20) NOT NULL DEFAULT 'AllProducts' CHECK (applies_to IN ('AllProducts','SpecificSpus','SpecificCategories')),
    target_ids          UUID[] NULL,
    valid_from          TIMESTAMPTZ NOT NULL,
    valid_to            TIMESTAMPTZ NOT NULL,
    is_public           BOOLEAN NOT NULL DEFAULT TRUE,
    is_active           BOOLEAN NOT NULL DEFAULT TRUE,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    CHECK (valid_to > valid_from)
);
CREATE INDEX idx_discounts_shop_active ON Discounts(shop_id, is_active, valid_from, valid_to) WHERE is_active = TRUE;

-- 31. DiscountUsages: Ghi nhận lịch sử sử dụng voucher của User theo ParentOrder
CREATE TABLE DiscountUsages (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    discount_id     UUID NOT NULL REFERENCES Discounts(id) ON DELETE RESTRICT,
    user_id         UUID NOT NULL REFERENCES Users(id) ON DELETE RESTRICT,
    parent_order_id UUID NOT NULL REFERENCES ParentOrders(id) ON DELETE RESTRICT,
    discount_amount DECIMAL(18,2) NOT NULL CHECK (discount_amount >= 0),
    used_at         TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    UNIQUE (discount_id, parent_order_id)
);
CREATE INDEX idx_discusage_user ON DiscountUsages(user_id, discount_id);
```

---

### 2.9 Nhóm FLASH SALE

```sql
-- 32. FlashSaleCampaigns: Khung giờ chiến dịch Flash Sale
CREATE TABLE FlashSaleCampaigns (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(200) NOT NULL,
    start_at    TIMESTAMPTZ NOT NULL,
    end_at      TIMESTAMPTZ NOT NULL,
    status      VARCHAR(20) NOT NULL DEFAULT 'Scheduled' CHECK (status IN ('Scheduled','Active','Ended','Cancelled')),
    banner_url  VARCHAR(500) NULL,
    created_by  UUID NOT NULL REFERENCES Users(id) ON DELETE RESTRICT,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at  TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    CHECK (end_at > start_at)
);
CREATE INDEX idx_flashsale_status ON FlashSaleCampaigns(status, start_at, end_at);

-- 33. FlashSaleItems: SKU tham gia Flash Sale (Quản lý tồn kho Flash Sale Atomic trên PostgreSQL)
CREATE TABLE FlashSaleItems (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    campaign_id     UUID NOT NULL REFERENCES FlashSaleCampaigns(id) ON DELETE CASCADE,
    sku_id          UUID NOT NULL REFERENCES Skus(id) ON DELETE RESTRICT,
    shop_id         UUID NOT NULL REFERENCES Shops(id) ON DELETE RESTRICT,
    flash_price     DECIMAL(18,2) NOT NULL CHECK (flash_price >= 0),
    quantity        INT NOT NULL CHECK (quantity > 0),
    per_user_limit  INT NOT NULL DEFAULT 1 CHECK (per_user_limit > 0),
    reserved_qty    INT NOT NULL DEFAULT 0 CHECK (reserved_qty >= 0),
    sold_qty        INT NOT NULL DEFAULT 0 CHECK (sold_qty >= 0),
    status          VARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (status IN ('Pending','Approved','Rejected','Ended')),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    UNIQUE (campaign_id, sku_id),
    CHECK (reserved_qty + sold_qty <= quantity)
);
CREATE INDEX idx_flashsaleitems_campaign ON FlashSaleItems(campaign_id, status);
CREATE INDEX idx_flashsaleitems_sku ON FlashSaleItems(sku_id);
```

---

### 2.10 Nhóm PAYMENT & ESCROW KÝ QUỸ

```sql
-- 34. Payments: Giao dịch thanh toán của Buyer (1 Payment cho 1 ParentOrder)
CREATE TABLE Payments (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    parent_order_id  UUID NOT NULL REFERENCES ParentOrders(id) ON DELETE RESTRICT,
    method           VARCHAR(20) NOT NULL CHECK (method IN ('MoMo','VietQR','COD')),
    amount           DECIMAL(18,2) NOT NULL CHECK (amount > 0),
    transaction_ref  VARCHAR(200) NULL UNIQUE, -- Mã giao dịch từ MoMo / VietQR
    gateway_response JSONB NULL,
    status           VARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (status IN ('Pending','Success','Failed','Expired','Refunded')),
    paid_at          TIMESTAMPTZ NULL,
    expired_at       TIMESTAMPTZ NULL,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at       TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_payments_parent_order ON Payments(parent_order_id);

-- 35. PaymentEscrows: Ký quỹ giữ tiền từng Sub-Order
CREATE TABLE PaymentEscrows (
    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sub_order_id UUID NOT NULL UNIQUE REFERENCES SubOrders(id) ON DELETE RESTRICT,
    payment_id   UUID NOT NULL REFERENCES Payments(id) ON DELETE RESTRICT,
    shop_id      UUID NOT NULL REFERENCES Shops(id) ON DELETE RESTRICT,
    held_amount  DECIMAL(18,2) NOT NULL CHECK (held_amount > 0),
    platform_fee DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (platform_fee >= 0),
    status       VARCHAR(20) NOT NULL DEFAULT 'Holding' CHECK (status IN ('Holding','Released','Disputed','Refunded','PartialRefund')),
    hold_until   TIMESTAMPTZ NULL, -- T+7 sau khi Delivered
    released_at  TIMESTAMPTZ NULL,
    refunded_at  TIMESTAMPTZ NULL,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at   TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_escrow_shop_status ON PaymentEscrows(shop_id, status);
CREATE INDEX idx_escrow_hold_until ON PaymentEscrows(hold_until) WHERE status = 'Holding';

-- 36. SellerPayouts: Lệnh chuyển tiền về tài khoản ngân hàng của Seller
CREATE TABLE SellerPayouts (
    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    shop_id      UUID NOT NULL REFERENCES Shops(id) ON DELETE RESTRICT,
    amount       DECIMAL(18,2) NOT NULL CHECK (amount > 0),
    bank_account VARCHAR(30) NOT NULL,
    bank_name    VARCHAR(100) NOT NULL,
    transfer_ref VARCHAR(200) NULL,
    status       VARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (status IN ('Pending','Processing','Completed','Failed')),
    processed_by UUID NULL REFERENCES Users(id) ON DELETE SET NULL,
    note         TEXT NULL,
    scheduled_at TIMESTAMPTZ NOT NULL,
    completed_at TIMESTAMPTZ NULL,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at   TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_payout_shop_status ON SellerPayouts(shop_id, status);
```

---

### 2.11 Nhóm SHIPPING (Vận chuyển)

```sql
-- 37. ShippingOrders: Vận đơn kết nối với ĐVVC (GHN / GHTK / ViettelPost) cho từng SubOrder
CREATE TABLE ShippingOrders (
    id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sub_order_id          UUID NOT NULL UNIQUE REFERENCES SubOrders(id) ON DELETE RESTRICT,
    provider              VARCHAR(20) NOT NULL CHECK (provider IN ('GHN','GHTK','ViettelPost')),
    service_code          VARCHAR(50) NOT NULL,
    tracking_code         VARCHAR(100) NULL UNIQUE,
    provider_order_id     VARCHAR(100) NULL,
    pickup_address_json   JSONB NOT NULL, -- Địa chỉ kho shop (snapshot)
    delivery_address_json JSONB NOT NULL, -- Địa chỉ nhận của Buyer (snapshot)
    cod_amount            DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (cod_amount >= 0),
    shipping_fee          DECIMAL(18,2) NOT NULL CHECK (shipping_fee >= 0),
    weight_gram           INT NOT NULL CHECK (weight_gram > 0),
    status                VARCHAR(30) NOT NULL DEFAULT 'ReadyToPick' 
                          CHECK (status IN ('ReadyToPick','Picking','Delivering','Delivered','Failed','Returned','Cancelled')),
    webhook_payload       JSONB NULL,
    estimated_at          TIMESTAMPTZ NULL,
    picked_at             TIMESTAMPTZ NULL,
    delivered_at          TIMESTAMPTZ NULL,
    created_at            TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at            TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_shipping_suborder ON ShippingOrders(sub_order_id);
CREATE INDEX idx_shipping_tracking ON ShippingOrders(provider, tracking_code) WHERE tracking_code IS NOT NULL;
CREATE INDEX idx_shipping_status ON ShippingOrders(status, updated_at);
```

---

### 2.12 Nhóm LIVESTREAM COMMERCE (Agora RTC)

```sql
-- 38. LivestreamSessions: Phiên phát trực tiếp của Shop
CREATE TABLE LivestreamSessions (
    id                 UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    shop_id            UUID NOT NULL REFERENCES Shops(id) ON DELETE RESTRICT,
    host_id            UUID NOT NULL REFERENCES Users(id) ON DELETE RESTRICT,
    title              VARCHAR(300) NOT NULL,
    thumbnail_url      VARCHAR(500) NULL,
    agora_channel_name VARCHAR(200) NOT NULL UNIQUE,
    status             VARCHAR(20) NOT NULL DEFAULT 'Scheduled' CHECK (status IN ('Scheduled','Live','Ended','Cancelled')),
    viewer_count       INT NOT NULL DEFAULT 0,
    peak_viewer_count  INT NOT NULL DEFAULT 0,
    scheduled_at       TIMESTAMPTZ NULL,
    started_at         TIMESTAMPTZ NULL,
    ended_at           TIMESTAMPTZ NULL,
    playback_url       VARCHAR(500) NULL,
    created_at         TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at         TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_livestream_shop_status ON LivestreamSessions(shop_id, status);

-- 39. LivestreamProducts: Sản phẩm ghim trong phiên live
CREATE TABLE LivestreamProducts (
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    session_id     UUID NOT NULL REFERENCES LivestreamSessions(id) ON DELETE CASCADE,
    sku_id         UUID NOT NULL REFERENCES Skus(id) ON DELETE RESTRICT,
    flash_price    DECIMAL(18,2) NULL CHECK (flash_price >= 0),
    quantity_limit INT NULL CHECK (quantity_limit > 0),
    sold_in_live   INT NOT NULL DEFAULT 0 CHECK (sold_in_live >= 0),
    display_order  INT NOT NULL DEFAULT 0,
    is_pinned      BOOLEAN NOT NULL DEFAULT FALSE,
    pinned_at      TIMESTAMPTZ NULL,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    UNIQUE (session_id, sku_id)
);
CREATE INDEX idx_liveprod_session ON LivestreamProducts(session_id, display_order);

-- 40. LivestreamComments: Bình luận trong live
CREATE TABLE LivestreamComments (
    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    session_id   UUID NOT NULL REFERENCES LivestreamSessions(id) ON DELETE CASCADE,
    user_id      UUID NULL REFERENCES Users(id) ON DELETE SET NULL,
    display_name VARCHAR(100) NOT NULL,
    content      VARCHAR(500) NOT NULL,
    is_question  BOOLEAN NOT NULL DEFAULT FALSE,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_livecomment_session ON LivestreamComments(session_id, created_at);
```

---

### 2.13 Nhóm REVIEW & NOTIFICATION & SYSTEM

```sql
-- 41. Reviews: Đánh giá sản phẩm đã mua
CREATE TABLE Reviews (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_item_id     UUID NOT NULL UNIQUE REFERENCES OrderItems(id) ON DELETE RESTRICT, -- Ràng buộc 1 món chỉ review 1 lần
    sku_id            UUID NOT NULL REFERENCES Skus(id) ON DELETE RESTRICT,
    shop_id           UUID NOT NULL REFERENCES Shops(id) ON DELETE RESTRICT,
    buyer_id          UUID NOT NULL REFERENCES Users(id) ON DELETE RESTRICT,
    rating            SMALLINT NOT NULL CHECK (rating BETWEEN 1 AND 5),
    title             VARCHAR(200) NULL,
    content           TEXT NULL,
    seller_reply      TEXT NULL,
    seller_replied_at TIMESTAMPTZ NULL,
    edit_count        SMALLINT NOT NULL DEFAULT 0 CHECK (edit_count <= 1), -- Chỉ cho phép sửa tối đa 1 lần
    is_visible        BOOLEAN NOT NULL DEFAULT TRUE,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    updated_at        TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_reviews_sku ON Reviews(sku_id, rating) WHERE is_visible = TRUE;
CREATE INDEX idx_reviews_shop ON Reviews(shop_id, rating) WHERE is_visible = TRUE;

-- 42. ReviewImages: Ảnh / Video đính kèm review
CREATE TABLE ReviewImages (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    review_id   UUID NOT NULL REFERENCES Reviews(id) ON DELETE CASCADE,
    media_url   VARCHAR(500) NOT NULL,
    media_type  VARCHAR(10) NOT NULL DEFAULT 'image' CHECK (media_type IN ('image','video')),
    created_at  TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_reviewimages_review ON ReviewImages(review_id);

-- 43. Notifications: Thông báo in-app
CREATE TABLE Notifications (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id    UUID NOT NULL REFERENCES Users(id) ON DELETE CASCADE,
    type       VARCHAR(50) NOT NULL,
    title      VARCHAR(200) NOT NULL,
    body       TEXT NOT NULL,
    ref_type   VARCHAR(30) NULL, -- 'Order','Payment','Livestream'
    ref_id     UUID NULL,
    is_read    BOOLEAN NOT NULL DEFAULT FALSE,
    read_at    TIMESTAMPTZ NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_notifications_user_unread ON Notifications(user_id, is_read, created_at) WHERE is_read = FALSE;

-- 44. AuditLogs: Nhật ký kiểm toán các thao tác nhạy cảm
CREATE TABLE AuditLogs (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id     UUID NULL REFERENCES Users(id) ON DELETE SET NULL,
    action      VARCHAR(80) NOT NULL,
    entity_type VARCHAR(50) NOT NULL,
    entity_id   UUID NULL,
    old_data    JSONB NULL,
    new_data    JSONB NULL,
    ip_address  VARCHAR(45) NULL,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW())
);
CREATE INDEX idx_auditlogs_entity ON AuditLogs(entity_type, entity_id);

-- 45. OutboxMessages: Bảng Outbox Pattern cho MassTransit / Event-Driven Architecture
CREATE TABLE OutboxMessages (
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    event_type     VARCHAR(200) NOT NULL,
    payload        JSONB NOT NULL,
    created_at     TIMESTAMPTZ NOT NULL DEFAULT TIMEZONE('utc', NOW()),
    processed_at   TIMESTAMPTZ NULL,
    retry_count    INT NOT NULL DEFAULT 0,
    error_message  TEXT NULL
);
CREATE INDEX idx_outbox_unprocessed ON OutboxMessages(processed_at, created_at) WHERE processed_at IS NULL;
```
