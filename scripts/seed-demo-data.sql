-- DEV DEMO DATA ONLY. Run after applying migrations and the standard RBAC/category seeders.
-- Idempotent: all generated IDs/codes are deterministic and inserts use ON CONFLICT.
-- Example: psql "$env:DATABASE_URL" -v ON_ERROR_STOP=1 -f scripts/seed-demo-data.sql
-- All demo accounts use: NovaDemo@2026!

\set ON_ERROR_STOP on
BEGIN;

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM roles WHERE name = 'Buyer')
     OR NOT EXISTS (SELECT 1 FROM roles WHERE name = 'Seller')
     OR NOT EXISTS (SELECT 1 FROM roles WHERE name = 'Admin') THEN
    RAISE EXCEPTION 'System roles missing. Let the API RBAC seeder run before this script.';
  END IF;
  IF NOT EXISTS (SELECT 1 FROM categories WHERE is_visible) THEN
    RAISE EXCEPTION 'Visible categories missing. Let the API category seeder run before this script.';
  END IF;
  IF NOT EXISTS (SELECT 1 FROM resources) OR NOT EXISTS (SELECT 1 FROM permissions) OR NOT EXISTS (SELECT 1 FROM role_permissions) THEN
    RAISE EXCEPTION 'RBAC resources/permissions missing. Let the API RBAC seeder run before this script.';
  END IF;
END $$;

-- Fixed demo password is hashed with the same BCrypt cost (12) as the application.
-- Accounts: standard demo admin/buyer/seller plus 8 extra shop owners and 30 extra buyers.
INSERT INTO users (id, email, password_hash, full_name, phone, is_seller, account_status, created_at, updated_at)
VALUES
  ('a0000000-0000-0000-0000-000000000000', 'admin@novalive.vn', '$2a$12$BPmi4/hKnET7lN0zZ3tST.a8/6MhjrriymRwxJoOP5qXyy8G3f1Ay', 'NovaLive Administrator', '0900000001', false, 'Active', now(), now()),
  ('a0000000-0000-0000-0000-000000000001', 'seller@novalive.vn', '$2a$12$BPmi4/hKnET7lN0zZ3tST.a8/6MhjrriymRwxJoOP5qXyy8G3f1Ay', 'NovaLive Demo Seller', '0900000002', true, 'Active', now(), now()),
  ('a0000000-0000-0000-0000-000000000002', 'buyer@novalive.vn', '$2a$12$BPmi4/hKnET7lN0zZ3tST.a8/6MhjrriymRwxJoOP5qXyy8G3f1Ay', 'Nguyễn Văn Khách', '0900000003', false, 'Active', now(), now())
ON CONFLICT (id) DO UPDATE SET email = EXCLUDED.email, password_hash = EXCLUDED.password_hash,
  full_name = EXCLUDED.full_name, phone = EXCLUDED.phone, is_seller = EXCLUDED.is_seller,
  account_status = 'Active', updated_at = now();

INSERT INTO user_roles (id, user_id, role_id, granted_at)
SELECT md5('novalive-demo-base-role:' || seed.role_name)::uuid, seed.user_id, r.id, now()
FROM (VALUES
  ('admin', 'a0000000-0000-0000-0000-000000000000'::uuid, 'Admin'),
  ('seller', 'a0000000-0000-0000-0000-000000000001'::uuid, 'Seller'),
  ('buyer', 'a0000000-0000-0000-0000-000000000002'::uuid, 'Buyer')
) AS seed(role_name, user_id, role_name_for_user)
JOIN roles r ON r.name = seed.role_name_for_user
ON CONFLICT DO NOTHING;

-- Admin accounts can exercise buyer-side demo flows with their own account.
INSERT INTO role_permissions (id, role_id, permission_id, granted_at)
SELECT md5('novalive-demo-admin-buyer-permission:' || p.code)::uuid, r.id, p.id, now()
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'Admin'
  AND p.code IN ('users:manage_own_addresses', 'carts:manage_own', 'orders:checkout',
                 'orders:view_own_buy', 'orders:cancel_own_buy', 'payments:initiate', 'returns:request_own')
ON CONFLICT DO NOTHING;

INSERT INTO shops (id, owner_id, shop_name, slug, description, rating_avg, rating_count,
                   status, created_at, created_by, updated_at, updated_by)
VALUES ('a1111111-1111-1111-1111-111111111111', 'a0000000-0000-0000-0000-000000000001',
        'NovaLive Flagship Store', 'novalive-flagship', 'Gian hàng demo NovaLive.', 5.00, 0, 'Active', now(),
        'a0000000-0000-0000-0000-000000000001', now(), 'a0000000-0000-0000-0000-000000000001')
ON CONFLICT (id) DO UPDATE SET owner_id = EXCLUDED.owner_id, shop_name = EXCLUDED.shop_name,
  slug = EXCLUDED.slug, rating_avg = EXCLUDED.rating_avg, rating_count = EXCLUDED.rating_count,
  status = 'Active', updated_at = now(), updated_by = EXCLUDED.updated_by;

CREATE TEMP TABLE _demo_ctx ON COMMIT DROP AS
SELECT
  (SELECT id FROM roles WHERE name = 'Buyer' LIMIT 1) AS buyer_role_id,
  (SELECT id FROM roles WHERE name = 'Seller' LIMIT 1) AS seller_role_id,
  (SELECT id FROM roles WHERE name = 'Admin' LIMIT 1) AS admin_role_id,
  s.id AS flagship_shop_id,
  s.owner_id AS flagship_seller_id
FROM shops s
WHERE s.slug = 'novalive-flagship'
LIMIT 1;

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM _demo_ctx) THEN
    RAISE EXCEPTION 'Unable to create or find the demo flagship shop.';
  END IF;
END $$;

-- One primary warehouse is added for the flagship and each demo shop below.
INSERT INTO users (id, email, password_hash, full_name, phone, is_seller, account_status, created_at, updated_at)
SELECT md5('novalive-demo-seller:' || n)::uuid,
       'demo.seller.' || lpad(n::text, 3, '0') || '@novalive.vn',
       '$2a$12$BPmi4/hKnET7lN0zZ3tST.a8/6MhjrriymRwxJoOP5qXyy8G3f1Ay',
       'Nova Demo Seller ' || lpad(n::text, 2, '0'),
       '0908' || lpad(n::text, 6, '0'), true, 'Active', now() - n * interval '1 day', now()
FROM generate_series(1, 8) AS g(n)
ON CONFLICT DO NOTHING;

INSERT INTO users (id, email, password_hash, full_name, phone, is_seller, account_status, created_at, updated_at)
SELECT md5('novalive-demo-buyer:' || n)::uuid,
       'demo.buyer.' || lpad(n::text, 3, '0') || '@novalive.vn',
       '$2a$12$BPmi4/hKnET7lN0zZ3tST.a8/6MhjrriymRwxJoOP5qXyy8G3f1Ay',
       'Nguyen Demo Buyer ' || lpad(n::text, 2, '0'),
       '0909' || lpad(n::text, 6, '0'), false, 'Active', now() - n * interval '1 day', now()
FROM generate_series(1, 30) AS g(n)
ON CONFLICT DO NOTHING;

INSERT INTO user_roles (id, user_id, role_id, granted_at)
SELECT md5('novalive-demo-seller-role:' || n)::uuid,
       md5('novalive-demo-seller:' || n)::uuid, c.seller_role_id, now()
FROM generate_series(1, 8) AS g(n)
CROSS JOIN _demo_ctx c
ON CONFLICT DO NOTHING;

INSERT INTO user_roles (id, user_id, role_id, granted_at)
SELECT md5('novalive-demo-buyer-role:' || n)::uuid,
       md5('novalive-demo-buyer:' || n)::uuid, c.buyer_role_id, now()
FROM generate_series(1, 30) AS g(n)
CROSS JOIN _demo_ctx c
ON CONFLICT DO NOTHING;

CREATE TEMP TABLE _demo_buyers ON COMMIT DROP AS
SELECT n AS buyer_no, md5('novalive-demo-buyer:' || n)::uuid AS user_id
FROM generate_series(1, 30) AS g(n);

CREATE TEMP TABLE _demo_shops (shop_no int PRIMARY KEY, shop_id uuid UNIQUE, owner_id uuid, shop_name text);
INSERT INTO _demo_shops VALUES
  (0, (SELECT flagship_shop_id FROM _demo_ctx), (SELECT flagship_seller_id FROM _demo_ctx), 'NovaLive Flagship Store');
INSERT INTO _demo_shops (shop_no, shop_id, owner_id, shop_name)
SELECT n,
       md5('novalive-demo-shop:' || n)::uuid,
       md5('novalive-demo-seller:' || n)::uuid,
       'Nova Demo Store ' || lpad(n::text, 2, '0')
FROM generate_series(1, 8) AS g(n)
ON CONFLICT DO NOTHING;

INSERT INTO shops (id, owner_id, shop_name, slug, description, logo_url, banner_url, phone, email,
                   rating_avg, rating_count, status, created_at, created_by, updated_at, updated_by)
SELECT shop_id, owner_id, shop_name, 'nova-demo-store-' || lpad(shop_no::text, 2, '0'),
       'Cửa hàng demo NovaLive với danh mục và đơn hàng mẫu liên kết đầy đủ.',
       'https://res.cloudinary.com/nguyennan12/image/upload/v1777444709/Livestream-ecommerce/Shops/69e364dfdf24f31846f15580/Products/rigceoxnr4fnqcs1bkpd.png',
       'https://res.cloudinary.com/nguyennan12/image/upload/v1777444709/Livestream-ecommerce/Shops/69e364dfdf24f31846f15580/Products/rigceoxnr4fnqcs1bkpd.png',
       '0912' || lpad(shop_no::text, 6, '0'), 'shop' || lpad(shop_no::text, 2, '0') || '@novalive.vn', 4.6, 18 + shop_no,
       'Active', now() - shop_no * interval '2 days', owner_id, now(), owner_id
FROM _demo_shops
WHERE shop_no > 0
ON CONFLICT DO NOTHING;

INSERT INTO shop_addresses (id, shop_id, warehouse_name, contact_name, contact_phone, province_id,
                            province_name, district_id, district_name, ward_code, ward_name,
                            detail_address, is_primary, is_return, created_at)
SELECT md5('novalive-demo-warehouse:' || shop_no)::uuid, shop_id,
       'Kho chính ' || shop_no, shop_name, '0912' || lpad(shop_no::text, 6, '0'), 202,
       'Hồ Chí Minh', 3695, 'Thành phố Thủ Đức', '90768', 'Phường An Khánh',
       '123 Đường Demo', true, true, now()
FROM _demo_shops s
WHERE NOT EXISTS (SELECT 1 FROM shop_addresses a WHERE a.shop_id = s.shop_id AND a.is_primary)
ON CONFLICT DO NOTHING;

CREATE TEMP TABLE _demo_warehouses ON COMMIT DROP AS
SELECT s.shop_no, s.shop_id, s.owner_id, s.shop_name,
       (SELECT a.id FROM shop_addresses a WHERE a.shop_id = s.shop_id ORDER BY a.is_primary DESC, a.created_at LIMIT 1) AS warehouse_id
FROM _demo_shops s;

INSERT INTO shop_verifications (id, shop_id, submitted_by, id_card_front, id_card_back, business_license,
                                bank_account, bank_name, status, reviewer_id, reviewer_note, reviewed_at, submitted_at)
SELECT md5('novalive-demo-kyc:' || shop_no)::uuid, shop_id, owner_id,
       'https://cdn.novalive.vn/demo/identity-front.jpg', 'https://cdn.novalive.vn/demo/identity-back.jpg',
       'https://cdn.novalive.vn/demo/business-license.pdf', '01234567' || lpad(shop_no::text, 2, '0'), 'Vietcombank',
       CASE WHEN shop_no % 4 = 0 THEN 'Pending' ELSE 'Approved' END,
       CASE WHEN shop_no % 4 = 0 THEN NULL ELSE (SELECT id FROM users WHERE email = 'admin@novalive.vn' LIMIT 1) END,
       CASE WHEN shop_no % 4 = 0 THEN NULL ELSE 'Hồ sơ demo đã duyệt' END,
       CASE WHEN shop_no % 4 = 0 THEN NULL ELSE now() - interval '1 day' END,
       now() - interval '2 days'
FROM _demo_shops
ON CONFLICT DO NOTHING;

INSERT INTO shop_wallets (id, shop_id, balance, locked_balance, currency, updated_at)
SELECT md5('novalive-demo-wallet:' || shop_id::text)::uuid, shop_id,
       (1250000 + shop_no * 225000) - (750000 + shop_no * 25000) - (325000 + shop_no * 15000),
       325000 + shop_no * 15000, 'VND', now()
FROM _demo_shops
ON CONFLICT (shop_id) DO UPDATE SET balance = EXCLUDED.balance,
  locked_balance = EXCLUDED.locked_balance, updated_at = now();

INSERT INTO shop_followers (id, shop_id, user_id, created_at)
SELECT md5('novalive-demo-follow:' || shop_no || ':' || buyer_no)::uuid, s.shop_id, b.user_id,
       now() - ((shop_no + buyer_no) % 20) * interval '1 day'
FROM _demo_shops s
CROSS JOIN _demo_buyers b
WHERE (shop_no + buyer_no) % 4 = 0
ON CONFLICT DO NOTHING;

-- Buyer addresses, active carts, and cart lines.
INSERT INTO user_addresses (id, user_id, recipient_name, phone, province_id, province_name, district_id,
                            district_name, ward_code, ward_name, detail_address, is_default, created_at, updated_at)
SELECT md5('novalive-demo-address:' || n)::uuid, b.user_id, 'Nguyen Demo Buyer ' || lpad(n::text, 2, '0'),
       '0909' || lpad(n::text, 6, '0'), 202, 'Hồ Chí Minh', 3695, 'Thành phố Thủ Đức', '90768',
       'Phường An Khánh', format('%s Đường Demo', 10 + n), true, now(), now()
FROM generate_series(1, 30) AS g(n)
JOIN _demo_buyers b ON b.buyer_no = n
ON CONFLICT DO NOTHING;

INSERT INTO user_addresses (id, user_id, recipient_name, phone, province_id, province_name, district_id,
                            district_name, ward_code, ward_name, detail_address, is_default, created_at, updated_at)
SELECT md5('novalive-demo-admin-address')::uuid, u.id, u.full_name, u.phone, 202, 'Hồ Chí Minh', 3695,
       'Thành phố Thủ Đức', '90768', 'Phường An Khánh', '10 Đường Demo', true, now(), now()
FROM users u WHERE u.email = 'admin@novalive.vn'
  AND NOT EXISTS (SELECT 1 FROM user_addresses a WHERE a.user_id = u.id AND a.is_default);

INSERT INTO carts (id, user_id, created_at, updated_at)
SELECT md5('novalive-demo-cart:' || buyer_no)::uuid, user_id, now() - interval '1 day', now()
FROM _demo_buyers
ON CONFLICT DO NOTHING;

INSERT INTO carts (id, user_id, created_at, updated_at)
SELECT md5('novalive-demo-admin-cart')::uuid, id, now(), now()
FROM users WHERE email = 'admin@novalive.vn'
ON CONFLICT DO NOTHING;

CREATE TEMP TABLE _demo_categories ON COMMIT DROP AS
SELECT id, row_number() OVER (ORDER BY display_order, id)::int AS category_no
FROM categories WHERE is_visible;

CREATE TEMP TABLE _demo_products ON COMMIT DROP AS
SELECT md5('novalive-demo-spu:' || s.shop_id::text || ':' || p.n)::uuid AS spu_id,
       s.shop_no, s.shop_id, c.id AS category_id, p.n AS product_no,
       (ARRAY['Tai nghe Bluetooth Nova AirBuds Pro', 'Bình giữ nhiệt inox 500ml', 'Đèn bàn LED chống cận',
              'Bàn phím cơ Keychron K2', 'Chuột không dây Logitech M650', 'Loa Bluetooth JBL Go 4',
              'Túi đeo chéo mini', 'Đồng hồ thông minh Amazfit', 'Sạc nhanh Anker USB-C 65W',
              'Giá đỡ laptop nhôm', 'Máy xay mini Bear', 'Camera hội nghị Logitech C920'])[((p.n - 1) % 12) + 1] AS product_name,
       (ARRAY['Nova', 'LocknLock', 'Rạng Đông', 'Keychron', 'Logitech', 'JBL', 'Nova', 'Amazfit',
              'Anker', 'Nova', 'Bear', 'Logitech'])[((p.n - 1) % 12) + 1] AS brand,
       CASE WHEN s.shop_no = 1 AND p.n = 1
         THEN 'https://images.unsplash.com/photo-1613629758552-443027994609?auto=format&fit=crop&w=1200&q=80'
         ELSE NULL END AS image_url,
       CASE WHEN p.n % 19 = 0 THEN 'Draft' WHEN p.n % 17 = 0 THEN 'Inactive' ELSE 'Active' END AS status,
       (450000 + p.n * 37500 + s.shop_no * 12500)::numeric(18,2) AS base_price,
       now() - ((p.n + s.shop_no) % 25) * interval '1 day' AS created_at
FROM _demo_shops s
CROSS JOIN generate_series(1, 12) AS p(n)
JOIN LATERAL (
  SELECT COALESCE(
    (SELECT cat.id FROM categories cat
      CROSS JOIN LATERAL unnest(CASE p.n
      WHEN 1 THEN ARRAY['%loa%', '%phụ kiện%']
      WHEN 2 THEN ARRAY['%bếp%', '%phụ kiện%']
      WHEN 3 THEN ARRAY['%bếp%', '%phụ kiện%']
      WHEN 4 THEN ARRAY['%bàn phím%', '%laptop%']
      WHEN 5 THEN ARRAY['%chuột%', '%phụ kiện%']
      WHEN 6 THEN ARRAY['%loa%']
      WHEN 7 THEN ARRAY['%túi%', '%phụ kiện%']
      WHEN 8 THEN ARRAY['%đồng hồ thông minh%']
      WHEN 9 THEN ARRAY['%phụ kiện%', '%điện thoại%']
      WHEN 10 THEN ARRAY['%laptop%']
      WHEN 11 THEN ARRAY['%bếp%', '%phụ kiện%']
      ELSE ARRAY['%phụ kiện%', '%laptop%'] END) WITH ORDINALITY AS wanted(pattern, priority)
      WHERE cat.is_visible AND cat.name ILIKE wanted.pattern
      ORDER BY wanted.priority, cat.display_order, cat.id LIMIT 1),
    (SELECT id FROM _demo_categories ORDER BY category_no LIMIT 1)
  ) AS id
) c ON c.id IS NOT NULL;

INSERT INTO spus (id, shop_id, category_id, name, description, brand, thumbnail_url,
                  attributes_config_json, status, created_at, created_by, updated_at, updated_by)
SELECT spu_id, shop_id, category_id, product_name,
       CASE product_no
         WHEN 1 THEN 'Tai nghe không dây Bluetooth, âm thanh rõ nét, micro đàm thoại tích hợp và hộp sạc nhỏ gọn. Phù hợp nghe nhạc, học tập và làm việc hằng ngày.'
         WHEN 2 THEN 'Bình giữ nhiệt inox dung tích 500ml, nắp kín chống rò rỉ và thiết kế gọn nhẹ để mang theo khi đi học hoặc đi làm.'
         WHEN 3 THEN 'Đèn bàn LED ánh sáng dịu, nhiều mức điều chỉnh và chân đế chắc chắn, hỗ trợ học tập và làm việc tại nhà.'
         WHEN 4 THEN 'Bàn phím cơ gọn gàng với kết nối USB-C, phím bấm ổn định và bố cục phù hợp cho góc làm việc hiện đại.'
         WHEN 5 THEN 'Chuột không dây thiết kế công thái học, kết nối ổn định và thao tác chính xác cho công việc hằng ngày.'
         WHEN 6 THEN 'Loa Bluetooth di động, âm thanh cân bằng, pin dùng lâu và thân máy nhỏ gọn cho nhu cầu nghe nhạc linh hoạt.'
         WHEN 7 THEN 'Túi đeo chéo mini có ngăn chứa tiện dụng, dây đeo điều chỉnh được và kiểu dáng phù hợp đi chơi hằng ngày.'
         WHEN 8 THEN 'Đồng hồ thông minh theo dõi vận động và giấc ngủ, nhận thông báo cơ bản, thiết kế nhẹ để đeo cả ngày.'
         WHEN 9 THEN 'Bộ sạc nhanh USB-C công suất 65W, hỗ trợ sạc thiết bị tương thích và có kích thước thuận tiện mang theo.'
         WHEN 10 THEN 'Giá đỡ laptop bằng nhôm giúp nâng màn hình lên tầm nhìn thoải mái, tạo khoảng thoáng cho bàn làm việc.'
         WHEN 11 THEN 'Máy xay mini nhỏ gọn dùng cho khẩu phần cá nhân, dễ thao tác và vệ sinh sau khi sử dụng.'
         ELSE 'Camera hội nghị độ phân giải cao, micro tích hợp và góc nhìn rộng cho các buổi học hoặc họp trực tuyến.'
       END,
       brand, image_url,
       '{"Màu sắc":["Đen","Trắng"],"Bảo hành":["12 tháng"]}'::jsonb,
       status, created_at, (SELECT owner_id FROM _demo_shops WHERE shop_id = p.shop_id), now(),
       (SELECT owner_id FROM _demo_shops WHERE shop_id = p.shop_id)
FROM _demo_products p
ON CONFLICT (id) DO UPDATE SET category_id = EXCLUDED.category_id, name = EXCLUDED.name,
  description = EXCLUDED.description, brand = EXCLUDED.brand, thumbnail_url = EXCLUDED.thumbnail_url,
  status = EXCLUDED.status, updated_at = now(), updated_by = EXCLUDED.updated_by;

CREATE TEMP TABLE _demo_skus ON COMMIT DROP AS
SELECT md5('novalive-demo-sku:' || p.spu_id::text || ':' || v.n)::uuid AS sku_id,
       p.spu_id, p.shop_id, p.shop_no, p.product_no, v.n AS variant_no,
       'NVD-' || lpad(p.shop_no::text, 2, '0') || '-' || lpad(p.product_no::text, 3, '0') || '-' || v.n AS sku_code,
       (p.base_price + CASE WHEN v.n = 2 THEN 50000 ELSE 0 END)::numeric(18,2) AS original_price,
       (p.base_price * 0.90 + CASE WHEN v.n = 2 THEN 50000 ELSE 0 END)::numeric(18,2) AS sell_price,
       CASE WHEN p.product_no % 17 = 0 AND v.n = 1 THEN 0
            WHEN p.product_no % 11 = 0 AND v.n = 1 THEN 3
            ELSE 18 + ((p.product_no * 7 + v.n * 13 + p.shop_no) % 83) END AS base_stock,
       p.created_at, p.image_url, p.product_name
FROM _demo_products p
CROSS JOIN generate_series(1, 2) AS v(n);

INSERT INTO skus (id, spu_id, shop_id, sku_code, attributes_json, original_price, sell_price,
                  weight_gram, is_active, created_at, created_by, updated_at, updated_by)
SELECT sku_id, spu_id, shop_id, sku_code,
       jsonb_build_object('Màu sắc', CASE WHEN variant_no = 1 THEN 'Đen' ELSE 'Trắng' END),
       original_price, sell_price, 250 + variant_no * 100, true, created_at,
       (SELECT owner_id FROM _demo_shops WHERE shop_id = s.shop_id), now(),
       (SELECT owner_id FROM _demo_shops WHERE shop_id = s.shop_id)
FROM _demo_skus s
ON CONFLICT DO NOTHING;

DELETE FROM sku_images si USING _demo_skus s
WHERE si.id = md5('novalive-demo-sku-image:' || s.sku_id::text)::uuid
  AND (s.image_url IS NULL OR si.image_url IS DISTINCT FROM s.image_url);

INSERT INTO sku_images (id, sku_id, image_url, is_primary, display_order, created_at)
SELECT md5('novalive-demo-sku-image:' || sku_id::text)::uuid, sku_id, image_url, true, 1, now()
FROM _demo_skus WHERE image_url IS NOT NULL
ON CONFLICT DO NOTHING;

INSERT INTO product_attributes (id, spu_id, attr_name, attr_value, display_order)
SELECT md5('novalive-demo-attr:' || p.spu_id::text || ':' || a.n)::uuid, p.spu_id,
       a.attr_name, a.attr_value, a.n
FROM _demo_products p
CROSS JOIN LATERAL (VALUES (1, 'Bảo hành', '12 tháng'), (2, 'Xuất xứ', 'Việt Nam'), (3, 'Đổi trả', '7 ngày')) a(n, attr_name, attr_value)
ON CONFLICT DO NOTHING;

INSERT INTO inventories (id, sku_id, shop_id, qty_on_hand, reserved_qty, min_stock, last_updated)
SELECT md5('novalive-demo-inventory:' || sku_id::text)::uuid, sku_id, shop_id, base_stock, 0, 5, now()
FROM _demo_skus
ON CONFLICT (sku_id) DO NOTHING;

INSERT INTO inventory_histories (id, inventory_id, sku_id, change_type, qty_before, qty_change,
                                 reserved_before, reserved_change, qty_after, ref_type, note,
                                 created_by, created_at, operation_id)
SELECT md5('novalive-demo-import-history:' || s.sku_id::text)::uuid,
       i.id, s.sku_id, 'Import', 0, s.base_stock, 0, 0, s.base_stock,
       'DemoSeed', 'Tồn đầu kỳ dữ liệu demo', owner.owner_id, s.created_at,
       md5('novalive-demo-import-op:' || s.sku_id::text)::uuid
FROM _demo_skus s
JOIN inventories i ON i.sku_id = s.sku_id
JOIN _demo_shops owner ON owner.shop_id = s.shop_id
WHERE s.base_stock > 0
ON CONFLICT DO NOTHING;

INSERT INTO cart_items (id, cart_id, sku_id, shop_id, quantity, unit_price, added_at, updated_at)
SELECT md5('novalive-demo-cart-item:' || b.buyer_no || ':' || s.sku_id::text)::uuid,
       md5('novalive-demo-cart:' || b.buyer_no)::uuid, s.sku_id, s.shop_id,
       1 + (b.buyer_no % 3), s.sell_price, now() - interval '2 hours', now()
FROM _demo_buyers b
JOIN LATERAL (SELECT * FROM _demo_skus ORDER BY sku_id OFFSET ((b.buyer_no - 1) % 10) LIMIT 3) s ON true
ON CONFLICT DO NOTHING;

-- Current, upcoming, and ended platform flash sales, plus shop-owned vouchers.
INSERT INTO flash_sale_campaigns (id, name, start_at, end_at, status, banner_url, created_at, updated_at)
VALUES
  (md5('novalive-demo-flash:active')::uuid, 'NovaLive Flash Sale đang diễn ra', now() - interval '1 hour', now() + interval '23 hours', 'Active', NULL, now(), now()),
  (md5('novalive-demo-flash:upcoming')::uuid, 'NovaLive Flash Sale sắp mở', now() + interval '1 day', now() + interval '2 days', 'Scheduled', NULL, now(), now()),
  (md5('novalive-demo-flash:ended')::uuid, 'NovaLive Flash Sale đã kết thúc', now() - interval '4 days', now() - interval '3 days', 'Ended', NULL, now() - interval '4 days', now())
ON CONFLICT DO NOTHING;

CREATE TEMP TABLE _demo_flash_items ON COMMIT DROP AS
SELECT md5('novalive-demo-flash-item:' || c.campaign_key || ':' || s.sku_id::text)::uuid AS item_id,
       CASE c.campaign_key WHEN 'active' THEN md5('novalive-demo-flash:active')::uuid
            WHEN 'upcoming' THEN md5('novalive-demo-flash:upcoming')::uuid
            ELSE md5('novalive-demo-flash:ended')::uuid END AS campaign_id,
       s.sku_id, s.shop_id, s.sell_price, c.campaign_key
FROM (VALUES ('active'), ('upcoming'), ('ended')) c(campaign_key)
CROSS JOIN LATERAL (
  SELECT * FROM _demo_skus WHERE base_stock > 0 ORDER BY sku_id LIMIT 24
) s;

INSERT INTO flash_sale_items (id, campaign_id, sku_id, shop_id, flash_price, quantity,
                              per_user_limit, reserved_qty, sold_qty, status, created_at)
SELECT item_id, campaign_id, sku_id, shop_id, round(sell_price * 0.80, 2),
       CASE (row_number() over (partition by campaign_id order by item_id)) % 4
           WHEN 1 THEN 30
           WHEN 2 THEN 25
           WHEN 3 THEN 20
           ELSE 35
       END,
       2, 0,
       CASE campaign_key 
           WHEN 'ended' THEN 26 + ((row_number() over (partition by campaign_id order by item_id)) % 5)
           WHEN 'active' THEN 14 + ((row_number() over (partition by campaign_id order by item_id)) % 6) * 2
           ELSE 0 
       END,
       CASE campaign_key WHEN 'active' THEN 'Approved' WHEN 'upcoming' THEN 'Pending' ELSE 'Ended' END,
       now()
FROM _demo_flash_items
ON CONFLICT DO NOTHING;

INSERT INTO discounts (id, shop_id, code, name, discount_type, discount_value, min_order_amount,
                       max_discount_amount, max_uses, used_count, per_user_limit, applies_to,
                       valid_from, valid_to, is_public, is_active, created_at, updated_at)
VALUES
  (md5('novalive-demo-voucher:platform10')::uuid, NULL, 'DEMO-NOVA10', 'Giảm 10% toàn sàn', 'PercentCart', 10, 200000, 80000, 1000, 0, 2, 'AllProducts', now() - interval '1 day', now() + interval '30 days', true, true, now(), now()),
  (md5('novalive-demo-voucher:platform50')::uuid, NULL, 'DEMO-NOVA50K', 'Giảm 50.000đ toàn sàn', 'FixedCart', 50000, 400000, NULL, 500, 0, 1, 'AllProducts', now() - interval '1 day', now() + interval '14 days', true, true, now(), now()),
  (md5('novalive-demo-voucher:freeship')::uuid, NULL, 'DEMO-FREESHIP', 'Miễn phí vận chuyển đến 50.000đ', 'FreeShip', 50000, 0, NULL, 1000, 0, 3, 'AllProducts', now() - interval '1 day', now() + interval '30 days', true, true, now(), now()),
  (md5('novalive-demo-voucher:freeship30')::uuid, NULL, 'DEMO-FREESHIP30', 'Giảm phí vận chuyển 30.000đ', 'FreeShip', 30000, 150000, NULL, 500, 0, 2, 'AllProducts', now() - interval '1 day', now() + interval '21 days', true, true, now(), now()),
  (md5('novalive-demo-voucher:ship50')::uuid, NULL, 'DEMO-SHIP50', 'Giảm 50% phí vận chuyển', 'PercentShip', 50, 200000, 30000, 600, 0, 2, 'AllProducts', now() - interval '1 day', now() + interval '21 days', true, true, now(), now()),
  (md5('novalive-demo-voucher:ship100')::uuid, NULL, 'DEMO-SHIP100', 'Miễn phí phí vận chuyển đơn lớn', 'PercentShip', 100, 350000, 50000, 300, 0, 1, 'AllProducts', now() - interval '1 day', now() + interval '14 days', true, true, now(), now()),
  (md5('novalive-demo-voucher:future')::uuid, NULL, 'DEMO-NOVA-FUTURE', 'Ưu đãi sắp mở', 'PercentCart', 15, 300000, 100000, 250, 0, 1, 'AllProducts', now() + interval '2 days', now() + interval '10 days', true, true, now(), now()),
  (md5('novalive-demo-voucher:expired')::uuid, NULL, 'DEMO-NOVA-OLD', 'Ưu đãi đã kết thúc', 'FixedCart', 30000, 100000, NULL, 100, 0, 1, 'AllProducts', now() - interval '20 days', now() - interval '10 days', true, false, now() - interval '20 days', now())
ON CONFLICT (id) DO UPDATE SET code = EXCLUDED.code, name = EXCLUDED.name,
  discount_type = EXCLUDED.discount_type, discount_value = EXCLUDED.discount_value,
  min_order_amount = EXCLUDED.min_order_amount, max_discount_amount = EXCLUDED.max_discount_amount,
  max_uses = EXCLUDED.max_uses, per_user_limit = EXCLUDED.per_user_limit,
  valid_from = EXCLUDED.valid_from, valid_to = EXCLUDED.valid_to,
  is_public = EXCLUDED.is_public, is_active = EXCLUDED.is_active, updated_at = now();

INSERT INTO discounts (id, shop_id, code, name, discount_type, discount_value, min_order_amount,
                       max_discount_amount, max_uses, used_count, per_user_limit, applies_to,
                       target_ids, valid_from, valid_to, is_public, is_active, created_at, updated_at)
SELECT md5('novalive-demo-shop-voucher:' || shop_no)::uuid, shop_id,
       'SHOP' || lpad(shop_no::text, 2, '0') || '-DEMO15', 'Ưu đãi riêng ' || shop_name, 'PercentCart', 15, 150000,
       90000, 300, 0, 2, 'AllProducts', NULL, now() - interval '1 day', now() + interval '21 days', true, true, now(), now()
FROM _demo_shops
ON CONFLICT DO NOTHING;

-- Seed linked orders across demo buyers plus one admin-owned order from Nova Demo Store 01.
CREATE TEMP TABLE _demo_orders ON COMMIT DROP AS
WITH sku_pool AS (
  SELECT s.*, row_number() OVER (ORDER BY sku_id)::int AS pool_no,
         count(*) OVER ()::int AS pool_size
  FROM _demo_skus s WHERE base_stock > 10
)
SELECT n AS order_no,
       md5('novalive-demo-parent-order:' || n)::uuid AS parent_id,
       md5('novalive-demo-sub-order:' || n)::uuid AS sub_id,
       md5('novalive-demo-order-item:' || n)::uuid AS item_id,
        CASE WHEN n = 61 THEN (SELECT id FROM users WHERE email = 'admin@novalive.vn') ELSE b.user_id END AS buyer_id,
        p.sku_id, p.shop_id, p.shop_no, p.spu_id, p.sku_code,
       p.product_name, p.sell_price, p.base_stock,
       (1 + (n % 3))::int AS quantity,
        CASE WHEN n = 61 THEN 'Confirmed'
          ELSE CASE n % 12 WHEN 0 THEN 'PendingPayment' WHEN 1 THEN 'Cancelled' WHEN 2 THEN 'Confirmed'
            WHEN 3 THEN 'Shipping' WHEN 4 THEN 'Delivered' WHEN 5 THEN 'ReturnRequested' ELSE 'Completed' END
        END AS order_status,
       (now() - ((n % 4) + 3) * interval '1 day') AS created_at,
        CASE WHEN n = 61 THEN true WHEN n % 12 IN (0, 1) THEN false ELSE true END AS is_paid,
       (SELECT warehouse_id FROM _demo_warehouses w WHERE w.shop_id = p.shop_id) AS warehouse_id,
       p.original_price
FROM generate_series(1, 61) AS g(n)
LEFT JOIN _demo_buyers b ON b.buyer_no = ((n - 1) % 30) + 1
JOIN sku_pool p ON p.pool_no = CASE WHEN n = 61
  THEN (SELECT pool_no FROM sku_pool WHERE shop_no = 1 AND product_no = 1 AND variant_no = 1)
  ELSE ((n * 7 - 1) % p.pool_size) + 1 END;

INSERT INTO parent_orders (id, buyer_id, order_code, shipping_address_json, total_item_amount,
                           total_shipping_fee, total_discount_amount, grand_total, currency_code,
                           payment_status, note, created_at, updated_at)
SELECT parent_id, buyer_id, 'DEMO-' || to_char(created_at, 'YYYYMM') || '-' || lpad(order_no::text, 4, '0'),
       jsonb_build_object('recipientName', 'Khách demo', 'phone', '0909000000', 'provinceName', 'Hồ Chí Minh',
         'districtName', 'Thành phố Thủ Đức', 'wardName', 'Phường An Khánh', 'detailAddress', '123 Đường Demo'),
       sell_price * quantity, 25000, CASE WHEN order_no % 4 = 0 AND is_paid THEN 30000 ELSE 0 END,
       sell_price * quantity + 25000 - CASE WHEN order_no % 4 = 0 AND is_paid THEN 30000 ELSE 0 END,
       'VND', CASE WHEN order_status = 'PendingPayment' THEN 'Pending'
         WHEN order_status = 'Cancelled' THEN 'Cancelled' ELSE 'Paid' END,
       'Đơn demo liên kết sản phẩm, tồn kho, thanh toán và vận chuyển.', created_at, created_at
FROM _demo_orders
ON CONFLICT DO NOTHING;

INSERT INTO sub_orders (id, parent_order_id, shop_id, sub_order_code, warehouse_address_id, order_source,
                        item_amount, shipping_fee, shop_discount_amount, platform_discount_amount,
                        shipping_discount_amount, sub_total, seller_earnings, status, confirmed_at,
                        shipped_at, delivered_at, completed_at, cancelled_at, cancel_reason,
                        created_at, updated_at)
SELECT sub_id, parent_id, shop_id, 'DEMO-S-' || lpad(order_no::text, 4, '0'), warehouse_id,
       CASE WHEN order_no % 3 = 0 THEN 'Livestream' ELSE 'Online' END,
       sell_price * quantity, 25000, 0, CASE WHEN order_no % 4 = 0 AND is_paid THEN 30000 ELSE 0 END,
       0, sell_price * quantity + 25000 - CASE WHEN order_no % 4 = 0 AND is_paid THEN 30000 ELSE 0 END,
       (sell_price * quantity) * 0.90, order_status,
       CASE WHEN is_paid THEN created_at + interval '2 minutes' END,
       CASE WHEN order_status IN ('Shipping','Delivered','ReturnRequested','Completed') THEN created_at + interval '1 day' END,
       CASE WHEN order_status IN ('Delivered','ReturnRequested','Completed') THEN created_at + interval '2 days' END,
       CASE WHEN order_status = 'Completed' THEN created_at + interval '3 days' END,
       CASE WHEN order_status = 'Cancelled' THEN created_at + interval '5 minutes' END,
       CASE WHEN order_status = 'Cancelled' THEN 'Khách demo huỷ đơn' END,
       created_at, created_at + interval '3 days'
FROM _demo_orders
ON CONFLICT DO NOTHING;

INSERT INTO order_items (id, sub_order_id, sku_id, sku_snapshot_json, quantity, unit_price,
                         original_price, discount_amount, line_total, status)
SELECT item_id, sub_id, sku_id,
       jsonb_build_object('spuName', product_name, 'skuCode', sku_code, 'thumbnailUrl',
         (SELECT thumbnail_url FROM spus WHERE id = _demo_orders.spu_id), 'attributesJson', '{"Màu sắc":"Đen"}'),
       quantity, sell_price, original_price, 0, sell_price * quantity, 'Active'
FROM _demo_orders
ON CONFLICT DO NOTHING;

INSERT INTO payments (id, parent_order_id, method, amount, transaction_ref, gateway_response,
                      status, paid_at, expired_at, created_at, updated_at)
SELECT md5('novalive-demo-payment:' || order_no)::uuid, parent_id,
       CASE WHEN order_no % 2 = 0 THEN 'MoMo' ELSE 'VietQR' END,
       sell_price * quantity + 25000 - CASE WHEN order_no % 4 = 0 AND is_paid THEN 30000 ELSE 0 END,
       CASE WHEN is_paid THEN 'DEMO-TXN-' || lpad(order_no::text, 4, '0') END,
       CASE WHEN is_paid THEN '{"demo":true,"result":"success"}'::jsonb END,
       CASE WHEN is_paid THEN 'Success' WHEN order_status = 'Cancelled' THEN 'Failed' ELSE 'Pending' END,
       CASE WHEN is_paid THEN created_at + interval '2 minutes' END,
       CASE WHEN order_status = 'PendingPayment' THEN now() + interval '15 minutes' END,
       created_at, created_at + interval '2 minutes'
FROM _demo_orders
ON CONFLICT DO NOTHING;

INSERT INTO order_status_histories (id, sub_order_id, from_status, to_status, changed_by, changed_by_role, note, changed_at)
SELECT md5('novalive-demo-status:' || order_no || ':1')::uuid, sub_id, NULL, 'PendingPayment', buyer_id, 'Buyer', 'Khởi tạo đơn demo', created_at
FROM _demo_orders
ON CONFLICT DO NOTHING;
INSERT INTO order_status_histories (id, sub_order_id, from_status, to_status, changed_by, changed_by_role, note, changed_at)
SELECT md5('novalive-demo-status:' || order_no || ':2')::uuid, sub_id, 'PendingPayment', order_status,
       buyer_id, 'System', 'Cập nhật trạng thái demo', created_at + interval '2 minutes'
FROM _demo_orders WHERE order_status <> 'PendingPayment'
ON CONFLICT DO NOTHING;

INSERT INTO shipping_orders (id, sub_order_id, provider, service_code, tracking_code, provider_order_id,
                             pickup_address_json, delivery_address_json, cod_amount, shipping_fee, weight_gram,
                             status, estimated_at, picked_at, delivered_at, created_at, updated_at)
SELECT md5('novalive-demo-shipping:' || order_no)::uuid, sub_id, 'GHN', 'ECO', 'DEMO' || lpad(order_no::text, 8, '0'),
       'GHN-DEMO-' || lpad(order_no::text, 4, '0'),
       jsonb_build_object('contactName', 'Nova Demo Store', 'phone', '0912000000', 'detailAddress', 'Kho Demo, TP.HCM'),
       jsonb_build_object('recipientName', 'Khách demo', 'phone', '0909000000', 'detailAddress', '123 Đường Demo, TP.HCM'),
       0, 25000, 500,
       CASE WHEN order_status IN ('Delivered','ReturnRequested','Completed') THEN 'Delivered'
            WHEN order_status = 'Shipping' THEN 'Delivering' ELSE 'ReadyToPick' END,
       (created_at + interval '4 days')::timestamptz,
       CASE WHEN order_status IN ('Shipping','Delivered','ReturnRequested','Completed') THEN created_at + interval '1 day' END,
       CASE WHEN order_status IN ('Delivered','ReturnRequested','Completed') THEN created_at + interval '2 days' END,
       created_at, created_at + interval '2 days'
FROM _demo_orders
WHERE order_status IN ('Processing','Shipping','Delivered','ReturnRequested','Completed')
ON CONFLICT DO NOTHING;

INSERT INTO discount_usages (id, discount_id, user_id, parent_order_id, discount_amount, used_at)
SELECT md5('novalive-demo-discount-usage:' || order_no)::uuid,
       md5('novalive-demo-voucher:platform10')::uuid, buyer_id, parent_id,
       LEAST(round(sell_price * quantity * 0.10, 2), 80000), created_at + interval '2 minutes'
FROM _demo_orders WHERE is_paid AND order_no % 4 = 0
ON CONFLICT DO NOTHING;
UPDATE discounts d SET used_count = (SELECT count(*) FROM discount_usages u WHERE u.discount_id = d.id), updated_at = now()
WHERE d.code = 'DEMO-NOVA10';

-- Returns and reviews are attached to real delivered order items.
INSERT INTO order_returns (id, sub_order_id, buyer_id, reason, evidence_urls, status, refund_amount,
                           seller_response, admin_note, created_at, updated_at)
SELECT md5('novalive-demo-return:' || order_no)::uuid, sub_id, buyer_id,
       CASE WHEN order_no % 2 = 0 THEN 'Defective' ELSE 'WrongItem' END,
       ARRAY['https://cdn.novalive.vn/demo/returns/evidence.jpg'],
       CASE WHEN order_no % 2 = 0 THEN 'Pending' ELSE 'SellerApproved' END,
       sell_price * quantity, CASE WHEN order_no % 2 = 0 THEN NULL ELSE 'Đã tiếp nhận yêu cầu demo.' END,
       NULL, created_at + interval '2 days', created_at + interval '2 days'
FROM _demo_orders WHERE order_status = 'ReturnRequested'
ON CONFLICT DO NOTHING;

INSERT INTO order_return_items (id, return_id, order_item_id, quantity, reason_detail)
SELECT md5('novalive-demo-return-item:' || o.order_no)::uuid,
       md5('novalive-demo-return:' || o.order_no)::uuid, o.item_id, 1, 'Sản phẩm demo cần kiểm tra.'
FROM _demo_orders o WHERE o.order_status = 'ReturnRequested'
ON CONFLICT DO NOTHING;

INSERT INTO reviews (id, order_item_id, sku_id, shop_id, buyer_id, rating, title, content, seller_reply,
                     seller_replied_at, edit_count, is_visible, created_at, updated_at)
SELECT md5('novalive-demo-review:' || order_no)::uuid, item_id, sku_id, shop_id, buyer_id,
       CASE WHEN order_no % 4 = 0 THEN 4 ELSE 5 END,
       CASE WHEN order_no % 4 = 0 THEN 'Sản phẩm ổn' ELSE 'Rất hài lòng' END,
       'Đơn giao nhanh, sản phẩm đúng mô tả. Đánh giá demo liên kết đơn đã hoàn tất.',
       'Cảm ơn bạn đã ủng hộ cửa hàng!', created_at + interval '4 days', 0, true,
       created_at + interval '4 days', created_at + interval '4 days'
FROM _demo_orders WHERE order_status = 'Completed' AND order_no % 2 = 0
ON CONFLICT DO NOTHING;

INSERT INTO review_images (id, review_id, media_url, media_type, created_at)
SELECT md5('novalive-demo-review-image:' || order_no)::uuid,
       md5('novalive-demo-review:' || order_no)::uuid,
       'https://cdn.novalive.vn/demo/reviews/product-photo.jpg', 'image', created_at + interval '4 days'
FROM _demo_orders WHERE order_status = 'Completed' AND order_no % 10 = 0
ON CONFLICT DO NOTHING;

-- Stock ledger: initial imports plus sales for paid orders; inventory ends at the corresponding balance.
WITH sold AS (
  SELECT sku_id, sum(quantity)::int AS qty_sold
  FROM _demo_orders WHERE is_paid GROUP BY sku_id
)
UPDATE inventories i
SET qty_on_hand = GREATEST(s.base_stock - COALESCE(sold.qty_sold, 0), 0), last_updated = now()
FROM _demo_skus s LEFT JOIN sold ON sold.sku_id = s.sku_id
WHERE i.sku_id = s.sku_id;

WITH sales AS (
  SELECT o.*,
         sum(o.quantity) OVER (PARTITION BY o.sku_id ORDER BY o.created_at, o.order_no) AS cumulative_qty
  FROM _demo_orders o
  WHERE o.is_paid
)
INSERT INTO inventory_histories (id, inventory_id, sku_id, change_type, qty_before, qty_change,
                                 reserved_before, reserved_change, qty_after, ref_type, ref_id, note,
                                 created_by, created_at, operation_id)
SELECT md5('novalive-demo-sale-history:' || sales.order_no)::uuid, i.id, sales.sku_id, 'SaleConfirmed',
       sales.base_stock - sales.cumulative_qty + sales.quantity, -sales.quantity, 0, 0,
       sales.base_stock - sales.cumulative_qty,
       'Order', sales.sub_id, 'Xuất kho đơn demo đã thanh toán.', sales.buyer_id,
       sales.created_at + interval '2 minutes', md5('novalive-demo-sale-op:' || sales.order_no)::uuid
FROM sales JOIN inventories i ON i.sku_id = sales.sku_id
ON CONFLICT DO NOTHING;

-- Shop wallets and payouts, with one live/scheduled/ended session per shop for the UI.
INSERT INTO shop_wallet_transactions (id, wallet_id, type, amount, balance_before, balance_after,
                                     ref_type, description, created_at)
SELECT md5('novalive-demo-wallet-tx:' || s.shop_no)::uuid, w.id, 'ManualAdjustment',
       1250000 + s.shop_no * 225000, 0, 1250000 + s.shop_no * 225000,
       'DemoSeed', 'Số dư mở đầu cho gian hàng demo.', now() - interval '6 days'
FROM _demo_shops s JOIN shop_wallets w ON w.shop_id = s.shop_id
ON CONFLICT DO NOTHING;

INSERT INTO seller_payouts (id, shop_id, amount, bank_account, bank_name, transfer_ref, status,
                            processed_by, note, scheduled_at, completed_at, created_at, updated_at)
SELECT md5('novalive-demo-payout:' || shop_no || ':done')::uuid, shop_id, 750000 + shop_no * 25000,
       '012345670' || lpad(shop_no::text, 2, '0'), 'Vietcombank', 'DEMO-PAYOUT-' || lpad(shop_no::text, 2, '0'), 'Completed',
       (SELECT id FROM users WHERE email = 'admin@novalive.vn' LIMIT 1), 'Đợt chi trả demo đã hoàn tất.',
       now() - interval '5 days', now() - interval '4 days', now() - interval '5 days', now() - interval '4 days'
FROM _demo_shops
ON CONFLICT DO NOTHING;

INSERT INTO seller_payouts (id, shop_id, amount, bank_account, bank_name, status, note, scheduled_at, created_at, updated_at)
SELECT md5('novalive-demo-payout:' || shop_no || ':pending')::uuid, shop_id, 325000 + shop_no * 15000,
       '012345670' || lpad(shop_no::text, 2, '0'), 'Vietcombank', 'Pending', 'Yêu cầu chi trả demo đang chờ.',
       now() + interval '1 day', now(), now()
FROM _demo_shops
ON CONFLICT DO NOTHING;

INSERT INTO shop_wallet_transactions (id, wallet_id, type, amount, balance_before, balance_after,
                                     ref_type, ref_id, description, created_at)
SELECT md5('novalive-demo-wallet-payout-done:' || s.shop_no)::uuid, w.id, 'PayoutWithdrawal',
       -(750000 + s.shop_no * 25000), 1250000 + s.shop_no * 225000,
       (1250000 + s.shop_no * 225000) - (750000 + s.shop_no * 25000),
       'SellerPayout', md5('novalive-demo-payout:' || s.shop_no || ':done')::uuid,
       'Ghi nhận khoản giải ngân demo đã hoàn tất.', now() - interval '4 days'
FROM _demo_shops s JOIN shop_wallets w ON w.shop_id = s.shop_id
ON CONFLICT DO NOTHING;

INSERT INTO shop_wallet_transactions (id, wallet_id, type, amount, balance_before, balance_after,
                                     ref_type, ref_id, description, created_at)
SELECT md5('novalive-demo-wallet-payout-lock:' || s.shop_no)::uuid, w.id, 'PayoutLock',
       -(325000 + s.shop_no * 15000),
       (1250000 + s.shop_no * 225000) - (750000 + s.shop_no * 25000),
       (1250000 + s.shop_no * 225000) - (750000 + s.shop_no * 25000) - (325000 + s.shop_no * 15000),
       'SellerPayout', md5('novalive-demo-payout:' || s.shop_no || ':pending')::uuid,
       'Khóa số dư cho yêu cầu giải ngân demo đang chờ.', now()
FROM _demo_shops s JOIN shop_wallets w ON w.shop_id = s.shop_id
ON CONFLICT DO NOTHING;

CREATE TEMP TABLE _demo_sessions ON COMMIT DROP AS
SELECT md5('novalive-demo-live:' || shop_no || ':' || st.status_key)::uuid AS session_id,
       shop_no, shop_id, owner_id, st.status_key,
       'demo-novalive-' || lpad(shop_no::text, 2, '0') || '-' || st.status_key AS channel_name
FROM _demo_shops
CROSS JOIN (VALUES ('Live'), ('Scheduled'), ('Ended')) st(status_key)
WHERE (st.status_key <> 'Live' OR shop_no <= 3);

INSERT INTO livestream_sessions (id, shop_id, host_id, title, thumbnail_url, agora_channel_name,
                                 status, viewer_count, peak_viewer_count, scheduled_at, started_at, ended_at,
                                 playback_url, created_at, created_by, updated_at, updated_by)
SELECT session_id, shop_id, owner_id,
       CASE status_key WHEN 'Live' THEN 'Đang live: săn deal công nghệ' WHEN 'Scheduled' THEN 'Livestream sắp diễn ra' ELSE 'Livestream tổng kết tuần' END,
       'https://res.cloudinary.com/nguyennan12/image/upload/c_fill,h_600,w_600/v1/Livestream-ecommerce/Shops/69e364dfdf24f31846f15580/Products/pdmlr4dssgu4mwygvtir.jpg',
       channel_name, status_key,
       CASE status_key WHEN 'Live' THEN 145 + shop_no * 12 WHEN 'Ended' THEN 720 + shop_no * 20 ELSE 0 END,
       CASE status_key WHEN 'Live' THEN 260 + shop_no * 10 WHEN 'Ended' THEN 900 + shop_no * 20 ELSE 0 END,
       CASE status_key WHEN 'Scheduled' THEN now() + interval '2 hours' ELSE now() - interval '1 day' END,
       CASE status_key WHEN 'Live' THEN now() - interval '45 minutes' WHEN 'Ended' THEN now() - interval '2 days' END,
       CASE status_key WHEN 'Ended' THEN now() - interval '1 day' END,
       CASE status_key WHEN 'Ended' THEN 'https://cdn.novalive.vn/demo/live/replay.mp4' END,
       now() - interval '2 days', owner_id, now(), owner_id
FROM _demo_sessions
ON CONFLICT DO NOTHING;

INSERT INTO livestream_products (id, session_id, sku_id, flash_price, quantity_limit, sold_in_live,
                                display_order, is_pinned, pinned_at, created_at)
SELECT md5('novalive-demo-live-product:' || s.session_id::text || ':' || p.sku_id::text)::uuid,
       s.session_id, p.sku_id,
       CASE WHEN p.sell_price > 0 THEN round(p.sell_price * 0.90, 2) END,
       40, CASE WHEN s.status_key = 'Ended' THEN 8 ELSE 2 END,
       row_number() OVER (PARTITION BY s.session_id ORDER BY p.sku_id)::int,
       row_number() OVER (PARTITION BY s.session_id ORDER BY p.sku_id) = 1,
       CASE WHEN row_number() OVER (PARTITION BY s.session_id ORDER BY p.sku_id) = 1 THEN now() - interval '30 minutes' END,
       now() - interval '1 day'
FROM _demo_sessions s
JOIN LATERAL (SELECT * FROM _demo_skus WHERE shop_id = s.shop_id ORDER BY sku_id LIMIT 4) p ON true
ON CONFLICT DO NOTHING;

INSERT INTO livestream_comments (id, session_id, user_id, display_name, content, is_question, created_at)
SELECT md5('novalive-demo-comment:' || s.session_id::text || ':' || b.buyer_no)::uuid,
       s.session_id, b.user_id, 'Demo Buyer ' || lpad(b.buyer_no::text, 2, '0'),
       CASE WHEN b.buyer_no % 2 = 0 THEN 'Sản phẩm này còn màu trắng không ạ?' ELSE 'Deal này tốt quá!' END,
       b.buyer_no % 2 = 0, now() - (b.buyer_no % 30) * interval '1 minute'
FROM _demo_sessions s CROSS JOIN _demo_buyers b
WHERE b.buyer_no <= 6
ON CONFLICT DO NOTHING;

INSERT INTO notifications (id, user_id, type, title, body, ref_type, ref_id, is_read, read_at, created_at)
SELECT md5('novalive-demo-notification:buyer:' || buyer_no)::uuid, user_id, 'OrderUpdate',
       'Đơn hàng demo đang được xử lý', 'Theo dõi trạng thái đơn hàng trong mục Đơn mua.',
       'Order', md5('novalive-demo-parent-order:' || buyer_no)::uuid, buyer_no % 3 = 0,
       CASE WHEN buyer_no % 3 = 0 THEN now() - interval '1 hour' END, now() - interval '2 hours'
FROM _demo_buyers
ON CONFLICT DO NOTHING;
INSERT INTO notifications (id, user_id, type, title, body, is_read, created_at)
SELECT md5('novalive-demo-notification:seller:' || shop_no)::uuid, owner_id, 'InventoryLow',
       'Một số SKU sắp hết hàng', 'Kiểm tra mục Kho hàng để bổ sung tồn.', false, now()
FROM _demo_shops
ON CONFLICT DO NOTHING;

-- Inactive authentication artifacts only: used OTPs and revoked tokens cannot log in.
INSERT INTO user_otps (id, user_id, otp_type, otp_hash, expires_at, used_at, created_at)
SELECT md5('novalive-demo-expired-otp:' || buyer_no)::uuid, user_id, 'EmailVerify',
       md5('expired-demo-otp:' || buyer_no), now() - interval '1 day', now() - interval '2 days', now() - interval '2 days'
FROM _demo_buyers WHERE buyer_no <= 10
ON CONFLICT DO NOTHING;
INSERT INTO refresh_tokens (id, user_id, token_hash, device_info, ip_address, expires_at, revoked_at, created_at)
SELECT md5('novalive-demo-revoked-token:' || buyer_no)::uuid, user_id,
       md5('revoked-demo-token:' || buyer_no), 'Demo seed', '127.0.0.1', now() - interval '1 day', now(), now() - interval '2 days'
FROM _demo_buyers WHERE buyer_no <= 10
ON CONFLICT DO NOTHING;

INSERT INTO audit_logs (id, user_id, action, entity_type, entity_id, new_data_json, ip_address, created_at)
SELECT md5('novalive-demo-audit:' || n)::uuid,
       (SELECT id FROM users WHERE email = 'admin@novalive.vn' LIMIT 1),
       'DemoSeed', 'DemoScenario', md5('novalive-demo-spu:' || (SELECT shop_id::text FROM _demo_shops WHERE shop_no = 0) || ':' || n)::uuid,
       jsonb_build_object('seed', 'novalive-demo', 'scenario', n), '127.0.0.1', now() - n * interval '1 hour'
FROM generate_series(1, 12) AS g(n)
ON CONFLICT DO NOTHING;
INSERT INTO outbox_messages (id, event_type, payload, created_at, processed_at, retry_count)
SELECT md5('novalive-demo-processed-outbox:' || n)::uuid, 'DemoSeedCompleted',
       jsonb_build_object('seed', 'novalive-demo', 'sequence', n), now() - interval '1 day', now(), 0
FROM generate_series(1, 5) AS g(n)
ON CONFLICT DO NOTHING;

-- Check the key relationships used by demo screens before committing.
DO $$
DECLARE orphan_count bigint;
BEGIN
  SELECT count(*) INTO orphan_count
  FROM users u LEFT JOIN roles r ON r.name = CASE WHEN u.is_seller THEN 'Seller' ELSE 'Buyer' END
  LEFT JOIN user_roles ur ON ur.user_id = u.id AND ur.role_id = r.id
  WHERE (u.email LIKE 'demo.seller.%@novalive.vn' OR u.email LIKE 'demo.buyer.%@novalive.vn')
    AND ur.id IS NULL;
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % demo users without their role link.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM shops s LEFT JOIN users u ON u.id = s.owner_id
  WHERE s.slug LIKE 'nova-demo-store-%' AND u.id IS NULL;
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % shops without owners.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM spus p LEFT JOIN shops s ON s.id = p.shop_id LEFT JOIN categories c ON c.id = p.category_id
  WHERE p.name ~ '[0-9]{2}-[0-9]{2}$' AND (s.id IS NULL OR c.id IS NULL);
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % products with a missing shop/category.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count FROM skus s LEFT JOIN spus p ON p.id = s.spu_id WHERE s.sku_code LIKE 'NVD-%' AND p.id IS NULL;
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % SKUs without SPUs.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count FROM skus s LEFT JOIN shops h ON h.id = s.shop_id WHERE s.sku_code LIKE 'NVD-%' AND h.id IS NULL;
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % SKUs without shops.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM inventories i LEFT JOIN skus s ON s.id = i.sku_id
  WHERE i.id = md5('novalive-demo-inventory:' || i.sku_id::text)::uuid AND s.id IS NULL;
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % inventories without SKUs.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM _demo_buyers b LEFT JOIN carts c ON c.user_id = b.user_id
  WHERE c.id IS NULL;
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % carts without buyers.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM cart_items ci LEFT JOIN carts c ON c.id = ci.cart_id LEFT JOIN skus s ON s.id = ci.sku_id LEFT JOIN shops h ON h.id = ci.shop_id
  WHERE s.sku_code LIKE 'NVD-%' AND (c.id IS NULL OR s.id IS NULL OR h.id IS NULL OR h.id <> s.shop_id);
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % cart items with broken links.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM inventories i JOIN skus s ON s.id = i.sku_id
  WHERE s.sku_code LIKE 'NVD-%' AND i.shop_id <> s.shop_id;
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % inventory records linked to the wrong shop.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count FROM sub_orders o LEFT JOIN parent_orders p ON p.id = o.parent_order_id WHERE o.sub_order_code LIKE 'DEMO-S-%' AND p.id IS NULL;
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % suborders without parent orders.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM parent_orders p LEFT JOIN users u ON u.id = p.buyer_id
  WHERE p.order_code LIKE 'DEMO-%' AND u.id IS NULL;
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % parent orders without buyers.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM sub_orders o LEFT JOIN shops h ON h.id = o.shop_id LEFT JOIN shop_addresses a ON a.id = o.warehouse_address_id
  WHERE o.sub_order_code LIKE 'DEMO-S-%' AND (h.id IS NULL OR a.id IS NULL);
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % suborders without a shop/warehouse.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count FROM order_items i LEFT JOIN sub_orders o ON o.id = i.sub_order_id LEFT JOIN skus s ON s.id = i.sku_id WHERE i.id IN (SELECT item_id FROM _demo_orders) AND (o.id IS NULL OR s.id IS NULL);
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % order items with broken links.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count FROM payments p LEFT JOIN parent_orders o ON o.id = p.parent_order_id WHERE p.transaction_ref LIKE 'DEMO-TXN-%' AND o.id IS NULL;
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % payments without parent orders.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM shipping_orders sh LEFT JOIN sub_orders so ON so.id = sh.sub_order_id
  WHERE sh.tracking_code LIKE 'DEMO%' AND so.id IS NULL;
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % shipments without suborders.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM _demo_orders o LEFT JOIN discount_usages du ON du.parent_order_id = o.parent_id
  LEFT JOIN discounts d ON d.id = du.discount_id LEFT JOIN users u ON u.id = du.user_id LEFT JOIN parent_orders p ON p.id = du.parent_order_id
  WHERE o.is_paid AND o.order_no % 4 = 0 AND (d.id IS NULL OR u.id IS NULL OR p.id IS NULL);
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % voucher usages with broken links.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM flash_sale_items fi LEFT JOIN flash_sale_campaigns fc ON fc.id = fi.campaign_id LEFT JOIN skus s ON s.id = fi.sku_id LEFT JOIN shops h ON h.id = fi.shop_id
  WHERE fi.id IN (SELECT item_id FROM _demo_flash_items) AND (fc.id IS NULL OR s.id IS NULL OR h.id IS NULL);
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % flash-sale items with broken links.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM _demo_orders o LEFT JOIN order_returns r ON r.id = md5('novalive-demo-return:' || o.order_no)::uuid
  LEFT JOIN sub_orders so ON so.id = r.sub_order_id LEFT JOIN users u ON u.id = r.buyer_id
  WHERE o.order_status = 'ReturnRequested' AND (so.id IS NULL OR u.id IS NULL);
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % returns with broken links.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM _demo_orders o LEFT JOIN reviews r ON r.id = md5('novalive-demo-review:' || o.order_no)::uuid
  LEFT JOIN order_items oi ON oi.id = r.order_item_id LEFT JOIN skus s ON s.id = r.sku_id
  LEFT JOIN shops h ON h.id = r.shop_id LEFT JOIN users u ON u.id = r.buyer_id
  WHERE o.order_status = 'Completed' AND o.order_no % 2 = 0
    AND (oi.id IS NULL OR s.id IS NULL OR h.id IS NULL OR u.id IS NULL);
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % reviews with broken links.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM livestream_products lp LEFT JOIN livestream_sessions ls ON ls.id = lp.session_id LEFT JOIN skus s ON s.id = lp.sku_id
  WHERE lp.id IN (SELECT md5('novalive-demo-live-product:' || ss.session_id::text || ':' || ds.sku_id::text)::uuid FROM _demo_sessions ss JOIN _demo_skus ds ON ds.shop_id = ss.shop_id ORDER BY ds.sku_id LIMIT 10000)
    AND (ls.id IS NULL OR s.id IS NULL);
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % live products with broken links.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM shop_wallet_transactions tx LEFT JOIN shop_wallets w ON w.id = tx.wallet_id
  WHERE tx.id IN (SELECT md5('novalive-demo-wallet-tx:' || shop_no)::uuid FROM _demo_shops) AND w.id IS NULL;
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % wallet transactions without wallets.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM _demo_shops s
  JOIN seller_payouts p ON p.id = md5('novalive-demo-payout:' || s.shop_no || ':pending')::uuid
  JOIN shop_wallets w ON w.shop_id = s.shop_id
  WHERE p.status = 'Pending' AND w.locked_balance < p.amount;
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % pending payouts without enough locked wallet balance.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM order_return_items ri LEFT JOIN order_returns r ON r.id = ri.return_id LEFT JOIN order_items oi ON oi.id = ri.order_item_id
  WHERE ri.id IN (SELECT md5('novalive-demo-return-item:' || order_no)::uuid FROM _demo_orders WHERE order_status = 'ReturnRequested')
    AND (r.id IS NULL OR oi.id IS NULL);
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % return items with broken links.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM review_images ri LEFT JOIN reviews r ON r.id = ri.review_id
  WHERE ri.id IN (SELECT md5('novalive-demo-review-image:' || order_no)::uuid FROM _demo_orders WHERE order_status = 'Completed' AND order_no % 10 = 0)
    AND r.id IS NULL;
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % review images without reviews.', orphan_count; END IF;
  SELECT count(*) INTO orphan_count
  FROM sku_images si LEFT JOIN skus s ON s.id = si.sku_id
  WHERE si.id IN (SELECT md5('novalive-demo-sku-image:' || sku_id::text)::uuid FROM _demo_skus)
    AND s.id IS NULL;
  IF orphan_count <> 0 THEN RAISE EXCEPTION 'Demo seed has % SKU images without SKUs.', orphan_count; END IF;
END $$;

COMMIT;

-- Summary for psql output.
SELECT 'products' AS entity, count(DISTINCT p.id) AS seeded FROM spus p JOIN skus s ON s.spu_id = p.id WHERE s.sku_code LIKE 'NVD-%';
SELECT 'demo_skus' AS entity, count(*) AS seeded FROM skus WHERE sku_code LIKE 'NVD-%';
SELECT 'demo_orders' AS entity, count(*) AS seeded FROM parent_orders WHERE order_code LIKE 'DEMO-%';
SELECT 'demo_payments_success' AS entity, count(*) AS seeded FROM payments WHERE transaction_ref LIKE 'DEMO-TXN-%' AND status = 'Success';
