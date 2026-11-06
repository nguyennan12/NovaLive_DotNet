-- Seed realistic historical completed sales for all 108 products across 9 shops
-- Provides diverse, natural, multi-tiered purchase counts for the Home page, Product Detail, and Best Sellers
-- Numbers range from 25 up to 1,450+ purchases so HomeProductCard displays realistic '1.3k lượt mua', '850 lượt mua', etc.

BEGIN;

DO $$
DECLARE
  v_spu RECORD;
  v_sku RECORD;
  v_shop RECORD;
  v_buyer_id uuid := 'a0000000-0000-0000-0000-000000000002';
  v_parent_id uuid;
  v_sub_id uuid;
  v_item_id uuid;
  v_payment_id uuid;
  v_wallet_id uuid;
  v_target_qty int;
  v_item_total numeric(18,2);
  v_running_bal numeric(18,2);
  v_rec RECORD;
  v_idx int := 1;
BEGIN
  -- Iterate through all active SPUs
  FOR v_spu IN
    SELECT 
      s.id AS spu_id,
      s.shop_id,
      s.name,
      sh.shop_name,
      -- Extract shop index: 0 for Flagship, 1..8 for Demo shops
      CASE 
        WHEN s.shop_id = 'a1111111-1111-1111-1111-111111111111' THEN 0
        ELSE COALESCE(NULLIF(regexp_replace(sh.shop_name, '\D', '', 'g'), '')::int, 1)
      END AS shop_idx,
      -- Base tier by product category/type
      CASE 
        WHEN s.name ILIKE '%Tai nghe%' THEN 1150
        WHEN s.name ILIKE '%Túi đeo%' THEN 780
        WHEN s.name ILIKE '%Bình giữ nhiệt%' THEN 560
        WHEN s.name ILIKE '%Sạc nhanh%' THEN 430
        WHEN s.name ILIKE '%Chuột không dây%' THEN 350
        WHEN s.name ILIKE '%Đèn bàn%' THEN 270
        WHEN s.name ILIKE '%Bàn phím%' THEN 190
        WHEN s.name ILIKE '%Loa Bluetooth%' THEN 160
        WHEN s.name ILIKE '%Đồng hồ%' THEN 130
        WHEN s.name ILIKE '%Giá đỡ%' THEN 85
        WHEN s.name ILIKE '%Máy xay%' THEN 50
        ELSE 30
      END AS base_sales
    FROM spus s
    JOIN shops sh ON sh.id = s.shop_id
    WHERE s.status = 'Active' AND s.deleted_at IS NULL
    ORDER BY s.shop_id, s.name
  LOOP
    -- Calculate realistic target sales quantity with shop variance
    v_target_qty := v_spu.base_sales + ((v_spu.shop_idx * 37 + v_idx * 13) % 230) - 40;
    IF v_target_qty < 15 THEN v_target_qty := 15 + (v_idx % 20); END IF;
    -- Flagship store gets extra boost for top products
    IF v_spu.shop_idx = 0 THEN
      v_target_qty := v_target_qty + 180;
    END IF;

    -- Pick the primary/first active SKU of this SPU
    SELECT id, sell_price, original_price, sku_code
    INTO v_sku
    FROM skus
    WHERE spu_id = v_spu.spu_id AND is_active = true AND deleted_at IS NULL
    ORDER BY sku_code LIMIT 1;

    IF v_sku.id IS NOT NULL THEN
      v_parent_id := md5('hist-sales-parent:' || v_spu.spu_id)::uuid;
      v_sub_id := md5('hist-sales-sub:' || v_spu.spu_id)::uuid;
      v_item_id := md5('hist-sales-item:' || v_spu.spu_id)::uuid;
      v_payment_id := md5('hist-sales-pay:' || v_spu.spu_id)::uuid;
      v_item_total := (v_sku.sell_price * v_target_qty)::numeric(18,2);

      -- 1. Insert historical parent order
      INSERT INTO parent_orders (
        id, buyer_id, order_code, shipping_address_json, total_item_amount,
        total_shipping_fee, total_discount_amount, grand_total, currency_code,
        payment_status, note, created_at, updated_at
      )
      VALUES (
        v_parent_id, v_buyer_id, 'HIST-ORD-' || lpad(v_idx::text, 4, '0'),
        jsonb_build_object(
          'recipientName', 'Khách hàng thân thiết',
          'phone', '0901234567',
          'provinceName', 'Hồ Chí Minh',
          'districtName', 'Quận 1',
          'wardName', 'Phường Bến Nghé',
          'detailAddress', 'Tòa nhà Bitexco, Q1'
        ),
        v_item_total, 25000, 0, v_item_total + 25000, 'VND',
        'Paid', 'Đơn hàng tích luỹ lịch sử bán sàn NovaLive',
        now() - interval '90 days' - (v_idx * interval '1 hour'),
        now() - interval '87 days'
      )
      ON CONFLICT (id) DO UPDATE SET
        total_item_amount = EXCLUDED.total_item_amount,
        grand_total = EXCLUDED.grand_total;

      -- 2. Insert historical completed sub order
      INSERT INTO sub_orders (
        id, parent_order_id, shop_id, sub_order_code, warehouse_address_id,
        order_source, item_amount, shipping_fee, shop_discount_amount,
        platform_discount_amount, shipping_discount_amount, sub_total,
        seller_earnings, status, confirmed_at, shipped_at, delivered_at,
        completed_at, created_at, updated_at
      )
      VALUES (
        v_sub_id, v_parent_id, v_spu.shop_id, 'HIST-SUB-' || lpad(v_idx::text, 4, '0'),
        (SELECT id FROM shop_addresses WHERE shop_id = v_spu.shop_id LIMIT 1),
        'Online', v_item_total, 25000, 0, 0, 0, v_item_total + 25000,
        ROUND((v_item_total + 25000) * 0.95, 2), 'Completed',
        now() - interval '90 days',
        now() - interval '89 days',
        now() - interval '88 days',
        now() - interval '87 days',
        now() - interval '90 days',
        now() - interval '87 days'
      )
      ON CONFLICT (id) DO UPDATE SET
        item_amount = EXCLUDED.item_amount,
        sub_total = EXCLUDED.sub_total,
        seller_earnings = EXCLUDED.seller_earnings;

      -- 3. Insert historical order item with realistic sold quantity
      INSERT INTO order_items (
        id, sub_order_id, sku_id, sku_snapshot_json, quantity,
        unit_price, original_price, discount_amount, line_total, status
      )
      VALUES (
        v_item_id, v_sub_id, v_sku.id,
        jsonb_build_object(
          'spuName', v_spu.name,
          'skuCode', v_sku.sku_code,
          'attributesJson', '{"Màu sắc":"Mặc định"}'
        ),
        v_target_qty, v_sku.sell_price, v_sku.original_price, 0, v_item_total, 'Active'
      )
      ON CONFLICT (id) DO UPDATE SET
        quantity = EXCLUDED.quantity,
        line_total = EXCLUDED.line_total;

      -- 4. Insert completed payment
      INSERT INTO payments (
        id, parent_order_id, method, amount, transaction_ref,
        gateway_response, status, paid_at, expired_at, created_at, updated_at
      )
      VALUES (
        v_payment_id, v_parent_id, 'VietQR', v_item_total + 25000,
        'HIST-TXN-' || lpad(v_idx::text, 4, '0'),
        jsonb_build_object('success', true),
        'Success', now() - interval '90 days', NULL,
        now() - interval '90 days', now() - interval '90 days'
      )
      ON CONFLICT (id) DO UPDATE SET
        amount = EXCLUDED.amount;

      v_idx := v_idx + 1;
    END IF;
  END LOOP;
END $$;

COMMIT;
