-- Seed a rich, comprehensive order pipeline for NovaLive Flagship Store and platform demo
-- Covers all order lifecycle stages: PendingPayment, Confirmed (with/without warehouse), Shipping, Delivered, Completed, ReturnRequested
-- Synchronizes ShopWallet, ShopWalletTransactions, ShippingOrders, Payments, OrderStatusHistories, and Dashboard Analytics.

BEGIN;

-- 1. Ensure all existing sub_orders have accurate seller_earnings (95% of sub_total, 5% platform fee)
UPDATE sub_orders
SET seller_earnings = ROUND(sub_total * 0.95, 2)
WHERE seller_earnings = 0 OR seller_earnings IS NULL;

-- 2. Define constants and target shop info
DO $$
DECLARE
  v_flagship_shop_id uuid := 'a1111111-1111-1111-1111-111111111111';
  v_flagship_wallet_id uuid;
  v_flagship_warehouse_id uuid;
  v_buyer_id uuid := 'a0000000-0000-0000-0000-000000000002'; -- buyer@novalive.vn
BEGIN
  -- Get or create wallet
  SELECT id INTO v_flagship_wallet_id FROM shop_wallets WHERE shop_id = v_flagship_shop_id;
  IF v_flagship_wallet_id IS NULL THEN
    v_flagship_wallet_id := md5('novalive-flagship-wallet')::uuid;
    INSERT INTO shop_wallets (id, shop_id, balance, locked_balance, currency, created_at, updated_at)
    VALUES (v_flagship_wallet_id, v_flagship_shop_id, 0, 0, 'VND', now(), now());
  END IF;

  -- Get primary warehouse
  SELECT id INTO v_flagship_warehouse_id FROM shop_addresses
  WHERE shop_id = v_flagship_shop_id AND is_primary = true LIMIT 1;
  IF v_flagship_warehouse_id IS NULL THEN
    SELECT id INTO v_flagship_warehouse_id FROM shop_addresses
    WHERE shop_id = v_flagship_shop_id LIMIT 1;
  END IF;

  -- 3. Update 2 SKUs to low stock so the seller action card shows lowStockSkus = 2
  UPDATE inventories SET qty_on_hand = 3, min_stock = 5 WHERE sku_id = '63e3aa15-e809-5aa9-32ba-2763c7ffb208'; -- Máy xay mini Bear
  UPDATE inventories SET qty_on_hand = 4, min_stock = 5 WHERE sku_id = '1a7480a4-077f-aad4-6907-d95ec34f994a'; -- Camera hội nghị Logitech C920
END $$;

-- 4. Temporary table defining pipeline orders for Flagship store
CREATE TEMP TABLE _pipeline_orders ON COMMIT DROP AS
WITH sku_list AS (
  SELECT s.id AS sku_id, s.sku_code, s.sell_price, s.original_price, sp.id AS spu_id, sp.name AS spu_name,
         row_number() OVER (ORDER BY s.sku_code) AS row_num
  FROM skus s
  JOIN spus sp ON sp.id = s.spu_id
  WHERE s.shop_id = 'a1111111-1111-1111-1111-111111111111'
),
buyers AS (
  SELECT u.id AS buyer_id, u.full_name, u.phone,
         row_number() OVER (ORDER BY u.email) AS b_num
  FROM users u
  JOIN user_roles ur ON ur.user_id = u.id
  WHERE ur.role_id = '00000000-0000-0000-0000-000000000003'
),
pipeline_def AS (
  SELECT
    g.idx,
    g.order_code,
    g.sub_code,
    g.order_status,
    g.payment_method,
    g.payment_status,
    g.has_warehouse,
    g.created_days_ago,
    g.created_hours_ago,
    g.qty,
    g.sku_index,
    g.buyer_index,
    g.source
  FROM (
    VALUES
      -- 1) Pending Payment (Khách vừa đặt online, đang chờ thanh toán QR)
      (101, 'NL-2611-0101', 'NL-2611-0101-S1', 'PendingPayment', 'VietQR', 'Pending', true, 0, 1, 1, 1, 1, 'Online'),
      (102, 'NL-2611-0102', 'NL-2611-0102-S1', 'PendingPayment', 'MoMo', 'Pending', true, 0, 2, 2, 3, 2, 'Online'),

      -- 2) Confirmed - NO warehouse selected yet (Cần chọn kho và chuẩn bị đơn - Action required!)
      (103, 'NL-2611-0103', 'NL-2611-0103-S1', 'Confirmed', 'VietQR', 'Paid', false, 0, 3, 1, 2, 3, 'Online'),
      (104, 'NL-2611-0104', 'NL-2611-0104-S1', 'Confirmed', 'COD', 'Pending', false, 0, 5, 2, 4, 4, 'Online'),
      (105, 'NL-2611-0105', 'NL-2611-0105-S1', 'Confirmed', 'MoMo', 'Paid', false, 0, 6, 1, 5, 5, 'Livestream'),

      -- 3) Confirmed - Warehouse selected (Sẵn sàng tạo vận đơn!)
      (106, 'NL-2611-0106', 'NL-2611-0106-S1', 'Confirmed', 'VietQR', 'Paid', true, 0, 8, 1, 6, 6, 'Online'),
      (107, 'NL-2611-0107', 'NL-2611-0107-S1', 'Confirmed', 'COD', 'Pending', true, 0, 10, 1, 7, 7, 'Online'),

      -- 4) Shipping - Đang giao hàng (Có mã vận đơn GHN, có thể bấm giả lập giao hàng thành công)
      (108, 'NL-2611-0108', 'NL-2611-0108-S1', 'Shipping', 'VietQR', 'Paid', true, 1, 2, 1, 8, 8, 'Online'),
      (109, 'NL-2611-0109', 'NL-2611-0109-S1', 'Shipping', 'COD', 'Pending', true, 1, 6, 2, 9, 9, 'Online'),
      (110, 'NL-2611-0110', 'NL-2611-0110-S1', 'Shipping', 'MoMo', 'Paid', true, 2, 4, 1, 10, 10, 'Livestream'),

      -- 5) Delivered - Giao hàng thành công (Đã đối soát ví, có thể đánh giá hoặc yêu cầu trả hàng)
      (111, 'NL-2611-0111', 'NL-2611-0111-S1', 'Delivered', 'VietQR', 'Paid', true, 3, 1, 1, 11, 1, 'Online'),
      (112, 'NL-2611-0112', 'NL-2611-0112-S1', 'Delivered', 'COD', 'Paid', true, 4, 2, 1, 12, 2, 'Online'),
      (113, 'NL-2611-0113', 'NL-2611-0113-S1', 'Delivered', 'MoMo', 'Paid', true, 5, 5, 2, 1, 3, 'Livestream'),

      -- 6) ReturnRequested - Yêu cầu hoàn trả (Đang chờ shop xử lý, 1 đơn quá 48h sẽ trigger cảnh báo!)
      (114, 'NL-2611-0114', 'NL-2611-0114-S1', 'ReturnRequested', 'VietQR', 'Paid', true, 5, 4, 1, 2, 4, 'Online'),
      (115, 'NL-2611-0115', 'NL-2611-0115-S1', 'ReturnRequested', 'COD', 'Paid', true, 6, 8, 1, 3, 5, 'Online'),

      -- 7) Completed - Các đơn hoàn tất trong tháng này & các tháng trước để biểu đồ doanh thu đầy đủ
      (116, 'NL-2611-0116', 'NL-2611-0116-S1', 'Completed', 'VietQR', 'Paid', true, 7, 1, 2, 4, 6, 'Online'),
      (117, 'NL-2611-0117', 'NL-2611-0117-S1', 'Completed', 'MoMo', 'Paid', true, 9, 3, 1, 5, 7, 'Online'),
      (118, 'NL-2611-0118', 'NL-2611-0118-S1', 'Completed', 'COD', 'Paid', true, 12, 2, 3, 6, 8, 'Livestream'),
      (119, 'NL-2611-0119', 'NL-2611-0119-S1', 'Completed', 'VietQR', 'Paid', true, 15, 4, 1, 7, 9, 'Online'),
      (120, 'NL-2611-0120', 'NL-2611-0120-S1', 'Completed', 'MoMo', 'Paid', true, 18, 5, 2, 8, 10, 'Online'),
      (121, 'NL-2611-0121', 'NL-2611-0121-S1', 'Completed', 'VietQR', 'Paid', true, 21, 6, 1, 9, 11, 'Online'),
      (122, 'NL-2611-0122', 'NL-2611-0122-S1', 'Completed', 'COD', 'Paid', true, 25, 2, 2, 10, 12, 'Livestream'),
      (123, 'NL-2611-0123', 'NL-2611-0123-S1', 'Completed', 'VietQR', 'Paid', true, 28, 4, 1, 11, 13, 'Online'),

      -- Tháng 10
      (124, 'NL-2610-0124', 'NL-2610-0124-S1', 'Completed', 'VietQR', 'Paid', true, 40, 0, 2, 12, 14, 'Online'),
      (125, 'NL-2610-0125', 'NL-2610-0125-S1', 'Completed', 'MoMo', 'Paid', true, 45, 0, 1, 1, 15, 'Online'),
      (126, 'NL-2610-0126', 'NL-2610-0126-S1', 'Completed', 'COD', 'Paid', true, 50, 0, 2, 2, 16, 'Livestream'),

      -- Tháng 9
      (127, 'NL-2609-0127', 'NL-2609-0127-S1', 'Completed', 'VietQR', 'Paid', true, 70, 0, 1, 3, 17, 'Online'),
      (128, 'NL-2609-0128', 'NL-2609-0128-S1', 'Completed', 'MoMo', 'Paid', true, 75, 0, 2, 4, 18, 'Online'),

      -- Tháng 8
      (129, 'NL-2608-0129', 'NL-2608-0129-S1', 'Completed', 'VietQR', 'Paid', true, 100, 0, 1, 5, 19, 'Online'),
      (130, 'NL-2608-0130', 'NL-2608-0130-S1', 'Completed', 'COD', 'Paid', true, 105, 0, 3, 6, 20, 'Livestream'),

      -- Tháng 7
      (131, 'NL-2607-0131', 'NL-2607-0131-S1', 'Completed', 'VietQR', 'Paid', true, 130, 0, 2, 7, 21, 'Online'),

      -- Tháng 6
      (132, 'NL-2606-0132', 'NL-2606-0132-S1', 'Completed', 'MoMo', 'Paid', true, 160, 0, 1, 8, 22, 'Online'),

      -- Tháng 5
      (133, 'NL-2605-0133', 'NL-2605-0133-S1', 'Completed', 'VietQR', 'Paid', true, 190, 0, 2, 9, 23, 'Online'),

      -- Tháng 4
      (134, 'NL-2604-0134', 'NL-2604-0134-S1', 'Completed', 'COD', 'Paid', true, 220, 0, 1, 10, 24, 'Livestream'),

      -- Tháng 3
      (135, 'NL-2603-0135', 'NL-2603-0135-S1', 'Completed', 'VietQR', 'Paid', true, 250, 0, 2, 11, 25, 'Online'),

      -- Tháng 2
      (136, 'NL-2602-0136', 'NL-2602-0136-S1', 'Completed', 'MoMo', 'Paid', true, 275, 0, 1, 12, 26, 'Online'),

      -- Tháng 1
      (137, 'NL-2601-0137', 'NL-2601-0137-S1', 'Completed', 'VietQR', 'Paid', true, 305, 0, 2, 1, 27, 'Online')
  ) g(idx, order_code, sub_code, order_status, payment_method, payment_status, has_warehouse,
      created_days_ago, created_hours_ago, qty, sku_index, buyer_index, source)
)
SELECT
  d.idx,
  md5('pipeline-parent:' || d.idx)::uuid AS parent_id,
  md5('pipeline-sub:' || d.idx)::uuid AS sub_id,
  md5('pipeline-item:' || d.idx)::uuid AS item_id,
  md5('pipeline-payment:' || d.idx)::uuid AS payment_id,
  md5('pipeline-shipping:' || d.idx)::uuid AS shipping_id,
  'a1111111-1111-1111-1111-111111111111'::uuid AS shop_id,
  CASE WHEN d.has_warehouse THEN '081337c4-2c70-c376-1e5c-b2259cd1d270'::uuid ELSE NULL END AS warehouse_id,
  COALESCE(b.buyer_id, 'a0000000-0000-0000-0000-000000000002'::uuid) AS buyer_id,
  COALESCE(b.full_name, 'Nguyễn Văn Khách') AS recipient_name,
  COALESCE(b.phone, '0901234567') AS recipient_phone,
  s.sku_id,
  s.sku_code,
  s.sell_price,
  s.original_price,
  s.spu_id,
  s.spu_name,
  d.qty,
  (s.sell_price * d.qty)::numeric(18,2) AS item_amount,
  25000::numeric(18,2) AS shipping_fee,
  (s.sell_price * d.qty + 25000)::numeric(18,2) AS total_amount,
  ROUND((s.sell_price * d.qty + 25000) * 0.95, 2) AS seller_earnings,
  ROUND((s.sell_price * d.qty + 25000) * 0.05, 2) AS platform_fee,
  d.order_code,
  d.sub_code,
  d.order_status,
  d.payment_method,
  d.payment_status,
  d.source,
  (now() - (d.created_days_ago || ' days')::interval - (d.created_hours_ago || ' hours')::interval) AS created_at
FROM pipeline_def d
JOIN sku_list s ON s.row_num = ((d.sku_index - 1) % 24) + 1
LEFT JOIN buyers b ON b.b_num = d.buyer_index;

-- 5. Insert Parent Orders
INSERT INTO parent_orders (
  id, buyer_id, order_code, shipping_address_json, total_item_amount,
  total_shipping_fee, total_discount_amount, grand_total, currency_code,
  payment_status, note, created_at, updated_at
)
SELECT
  parent_id, buyer_id, order_code,
  jsonb_build_object(
    'recipientName', recipient_name,
    'phone', recipient_phone,
    'provinceName', 'Hồ Chí Minh',
    'districtName', 'Quận 1',
    'wardName', 'Phường Bến Nghé',
    'detailAddress', '123 Đường Lê Lợi'
  ),
  item_amount, shipping_fee, 0, total_amount, 'VND',
  payment_status, 'Đơn hàng trải nghiệm quy trình NovaLive', created_at, created_at
FROM _pipeline_orders
ON CONFLICT (id) DO UPDATE SET
  payment_status = EXCLUDED.payment_status,
  updated_at = EXCLUDED.updated_at;

-- 6. Insert Sub Orders
INSERT INTO sub_orders (
  id, parent_order_id, shop_id, sub_order_code, warehouse_address_id,
  order_source, item_amount, shipping_fee, shop_discount_amount,
  platform_discount_amount, shipping_discount_amount, sub_total,
  seller_earnings, status, confirmed_at, shipped_at, delivered_at,
  completed_at, cancelled_at, cancel_reason, created_at, updated_at
)
SELECT
  sub_id, parent_id, shop_id, sub_code, warehouse_id,
  source, item_amount, shipping_fee, 0, 0, 0, total_amount,
  seller_earnings, order_status,
  CASE WHEN order_status NOT IN ('PendingPayment', 'Cancelled') THEN created_at + interval '5 minutes' END,
  CASE WHEN order_status IN ('Shipping', 'Delivered', 'Completed', 'ReturnRequested') THEN created_at + interval '1 day' END,
  CASE WHEN order_status IN ('Delivered', 'Completed', 'ReturnRequested') THEN created_at + interval '2 days' END,
  CASE WHEN order_status = 'Completed' THEN created_at + interval '3 days' END,
  NULL, NULL,
  created_at, created_at + interval '1 hour'
FROM _pipeline_orders
ON CONFLICT (id) DO UPDATE SET
  warehouse_address_id = EXCLUDED.warehouse_address_id,
  status = EXCLUDED.status,
  seller_earnings = EXCLUDED.seller_earnings,
  delivered_at = EXCLUDED.delivered_at,
  completed_at = EXCLUDED.completed_at,
  updated_at = EXCLUDED.updated_at;

-- 7. Insert Order Items
INSERT INTO order_items (
  id, sub_order_id, sku_id, sku_snapshot_json, quantity,
  unit_price, original_price, discount_amount, line_total, status
)
SELECT
  item_id, sub_id, sku_id,
  jsonb_build_object(
    'spuName', spu_name,
    'skuCode', sku_code,
    'thumbnailUrl', (SELECT thumbnail_url FROM spus WHERE id = _pipeline_orders.spu_id),
    'attributesJson', '{"Màu sắc":"Đen"}'
  ),
  qty, sell_price, original_price, 0, item_amount, 'Active'
FROM _pipeline_orders
ON CONFLICT (id) DO NOTHING;

-- 8. Insert Payments
INSERT INTO payments (
  id, parent_order_id, method, amount, transaction_ref,
  gateway_response, status, paid_at, expired_at, created_at, updated_at
)
SELECT
  payment_id, parent_id, payment_method, total_amount,
  CASE WHEN payment_status = 'Paid' THEN 'NL-TXN-' || idx END,
  CASE WHEN payment_status = 'Paid' THEN jsonb_build_object('success', true, 'code', '00') END,
  CASE WHEN payment_status = 'Paid' THEN 'Success' ELSE 'Pending' END,
  CASE WHEN payment_status = 'Paid' THEN created_at + interval '5 minutes' END,
  CASE WHEN payment_status = 'Pending' THEN created_at + interval '24 hours' END,
  created_at, created_at
FROM _pipeline_orders
ON CONFLICT (id) DO UPDATE SET
  status = EXCLUDED.status,
  paid_at = EXCLUDED.paid_at;

-- 9. Insert Order Status Histories
INSERT INTO order_status_histories (id, sub_order_id, from_status, to_status, changed_by, changed_by_role, note, changed_at)
SELECT md5('pipeline-hist-init:' || idx)::uuid, sub_id, NULL, 'PendingPayment', buyer_id, 'Buyer', 'Đặt hàng thành công', created_at
FROM _pipeline_orders
ON CONFLICT (id) DO NOTHING;

INSERT INTO order_status_histories (id, sub_order_id, from_status, to_status, changed_by, changed_by_role, note, changed_at)
SELECT md5('pipeline-hist-conf:' || idx)::uuid, sub_id, 'PendingPayment', 'Confirmed', NULL, 'System', 'Xác nhận thanh toán', created_at + interval '5 minutes'
FROM _pipeline_orders
WHERE order_status NOT IN ('PendingPayment', 'Cancelled')
ON CONFLICT (id) DO NOTHING;

INSERT INTO order_status_histories (id, sub_order_id, from_status, to_status, changed_by, changed_by_role, note, changed_at)
SELECT md5('pipeline-hist-ship:' || idx)::uuid, sub_id, 'Confirmed', 'Shipping', NULL, 'Seller', 'Đã giao cho đơn vị vận chuyển GHN', created_at + interval '1 day'
FROM _pipeline_orders
WHERE order_status IN ('Shipping', 'Delivered', 'Completed', 'ReturnRequested')
ON CONFLICT (id) DO NOTHING;

INSERT INTO order_status_histories (id, sub_order_id, from_status, to_status, changed_by, changed_by_role, note, changed_at)
SELECT md5('pipeline-hist-deliv:' || idx)::uuid, sub_id, 'Shipping', 'Delivered', NULL, 'System', 'Đơn vị vận chuyển báo giao thành công', created_at + interval '2 days'
FROM _pipeline_orders
WHERE order_status IN ('Delivered', 'Completed', 'ReturnRequested')
ON CONFLICT (id) DO NOTHING;

INSERT INTO order_status_histories (id, sub_order_id, from_status, to_status, changed_by, changed_by_role, note, changed_at)
SELECT md5('pipeline-hist-comp:' || idx)::uuid, sub_id, 'Delivered', 'Completed', buyer_id, 'Buyer', 'Khách xác nhận đã nhận hàng hoàn chỉnh', created_at + interval '3 days'
FROM _pipeline_orders
WHERE order_status = 'Completed'
ON CONFLICT (id) DO NOTHING;

-- 10. Insert Shipping Orders for Shipping / Delivered / Completed
INSERT INTO shipping_orders (
  id, sub_order_id, provider, service_code, tracking_code, provider_order_id,
  pickup_address_json, delivery_address_json, cod_amount, shipping_fee, weight_gram,
  status, estimated_at, picked_at, delivered_at, created_at, updated_at
)
SELECT
  shipping_id, sub_id, 'GHN', 'ECO',
  'GHN-FLAGSHIP-' || lpad(idx::text, 4, '0'),
  'GHN-ID-' || idx,
  jsonb_build_object('contactName', 'NovaLive Flagship', 'phone', '0901234567', 'detailAddress', 'Kho chính NovaLive Flagship'),
  jsonb_build_object('recipientName', recipient_name, 'phone', recipient_phone, 'detailAddress', '123 Đường Lê Lợi, Q1, TP.HCM'),
  CASE WHEN payment_method = 'COD' THEN total_amount ELSE 0 END,
  shipping_fee, 500,
  CASE
    WHEN order_status = 'Shipping' THEN 'Delivering'
    WHEN order_status IN ('Delivered', 'Completed', 'ReturnRequested') THEN 'Delivered'
    ELSE 'ReadyToPick'
  END,
  created_at + interval '3 days',
  created_at + interval '1 day',
  CASE WHEN order_status IN ('Delivered', 'Completed', 'ReturnRequested') THEN created_at + interval '2 days' END,
  created_at + interval '1 day',
  created_at + interval '2 days'
FROM _pipeline_orders
WHERE order_status IN ('Shipping', 'Delivered', 'Completed', 'ReturnRequested')
ON CONFLICT (id) DO UPDATE SET
  status = EXCLUDED.status,
  delivered_at = EXCLUDED.delivered_at;

-- 11. Insert Order Returns for ReturnRequested
INSERT INTO order_returns (
  id, sub_order_id, buyer_id, reason, evidence_urls,
  status, refund_amount, seller_response, admin_note, created_at, updated_at
)
SELECT
  md5('pipeline-return:' || idx)::uuid, sub_id, buyer_id,
  'Defective',
  ARRAY['https://cdn.novalive.vn/demo/returns/defective-device.jpg'],
  'Pending',
  total_amount,
  NULL,
  NULL,
  -- Đơn 114: tạo cách đây 52 giờ (quá hạn 48 giờ -> Cần xử lý ngay!)
  -- Đơn 115: tạo cách đây 12 giờ
  CASE WHEN idx = 114 THEN now() - interval '52 hours' ELSE now() - interval '12 hours' END,
  now()
FROM _pipeline_orders
WHERE order_status = 'ReturnRequested'
ON CONFLICT (id) DO NOTHING;

INSERT INTO order_return_items (id, return_id, order_item_id, quantity, reason_detail)
SELECT
  md5('pipeline-return-item:' || idx)::uuid,
  md5('pipeline-return:' || idx)::uuid,
  item_id,
  qty,
  'Thiết bị bật không lên nguồn khi mở hộp, mong shop hỗ trợ hoàn hàng.'
FROM _pipeline_orders
WHERE order_status = 'ReturnRequested'
ON CONFLICT (id) DO NOTHING;

-- 12. Settle Wallet Ledger Transactions for Settled Orders (Delivered, Completed, ReturnRequested)
-- Ensure ShopWallet and ShopWalletTransactions accurately reflect OrderRevenue
WITH settled_orders AS (
  SELECT
    sub_id,
    order_code,
    sub_code,
    total_amount,
    created_at + interval '2 days' AS settled_at
  FROM _pipeline_orders
  WHERE order_status IN ('Delivered', 'Completed')
     OR (payment_method IN ('VietQR', 'MoMo') AND order_status IN ('Confirmed', 'Shipping', 'ReturnRequested'))
)
INSERT INTO shop_wallet_transactions (
  id, wallet_id, type, amount, balance_before, balance_after,
  ref_type, ref_id, description, created_at
)
SELECT
  md5('pipeline-wallet-rev:' || sub_id::text)::uuid,
  '466be702-6c67-b3fc-53b3-128097822d4c',
  'OrderRevenue',
  total_amount,
  0, -- running balance updated below
  total_amount,
  'SubOrder',
  sub_id,
  'Doanh thu từ đơn hàng ' || sub_code,
  settled_at
FROM settled_orders
ON CONFLICT (id) DO NOTHING;

-- 13. Recalculate and update wallet balance for Flagship Shop
DO $$
DECLARE
  v_flagship_wallet uuid := '466be702-6c67-b3fc-53b3-128097822d4c';
  v_net_balance numeric(18,2);
  v_rec RECORD;
  v_running numeric(18,2) := 0;
BEGIN
  -- Re-sequence running balances in chronological order
  FOR v_rec IN
    SELECT id, amount
    FROM shop_wallet_transactions
    WHERE wallet_id = v_flagship_wallet
    ORDER BY created_at ASC, id ASC
  LOOP
    UPDATE shop_wallet_transactions
    SET balance_before = v_running,
        balance_after = v_running + amount
    WHERE id = v_rec.id;
    v_running := v_running + v_rec.amount;
  END LOOP;

  -- Update wallet balance
  UPDATE shop_wallets
  SET balance = v_running,
      updated_at = now()
  WHERE id = v_flagship_wallet;
END $$;

COMMIT;
