-- Explicitly approved, disposable LOCAL QA fixtures. Never run against a shared database.
-- Idempotent: once the marker Buyer exists, a repeat does not reset mutations.
-- No schema changes, external calls, credentials, or identity documents.
\set ON_ERROR_STOP on
BEGIN;
DO $qa$
DECLARE
  seller uuid := 'a0000000-0000-0000-0000-000000000001';
  shop uuid := 'a1111111-1111-1111-1111-111111111111';
  buyer uuid := 'b6100000-0000-0000-0000-000000000001';
  warehouse uuid := 'b6100000-0000-0000-0000-000000000002';
  wallet uuid := 'b6100000-0000-0000-0000-000000000003';
  product record; inv record; n integer; parent uuid; sub uuid; line uuid;
  state text; address jsonb; snapshot jsonb; stamp timestamptz := now();
BEGIN
  IF current_database() <> 'novalive_db' THEN RAISE EXCEPTION 'Expected local novalive_db'; END IF;
  IF NOT EXISTS (SELECT 1 FROM shops s JOIN users u ON u.id=s.owner_id
      WHERE s.id=shop AND u.id=seller AND u.email='seller@novalive.local' AND s.status='Active')
  THEN RAISE EXCEPTION 'Expected active demo Seller/shop'; END IF;
  PERFORM pg_advisory_xact_lock(610024);
  IF EXISTS (SELECT 1 FROM users WHERE id=buyer) THEN
    RAISE NOTICE 'T61-QA fixtures already exist; preserving mutations'; RETURN;
  END IF;
  IF EXISTS (SELECT 1 FROM shop_wallets WHERE shop_id=shop)
     OR EXISTS (SELECT 1 FROM shop_addresses WHERE shop_id=shop AND (is_primary OR is_return))
  THEN RAISE EXCEPTION 'Demo wallet/default warehouse already exists; inspect before seeding'; END IF;

  INSERT INTO users(id,email,phone,password_hash,full_name,is_seller,account_status,created_at,updated_at)
  SELECT buyer,'t61-qa-buyer@novalive.local','0900000061',password_hash,'T61-QA Người mua local',false,'Active',stamp,stamp
  FROM users WHERE id=seller;
  INSERT INTO user_roles(id,user_id,role_id,granted_at)
  VALUES(gen_random_uuid(),buyer,'00000000-0000-0000-0000-000000000003',stamp);
  INSERT INTO shop_addresses(id,shop_id,warehouse_name,contact_name,contact_phone,
    province_id,province_name,district_id,district_name,ward_code,ward_name,detail_address,is_primary,is_return,created_at)
  VALUES(warehouse,shop,'T61-QA Kho local','T61-QA Seller','0900000061',202,'Hồ Chí Minh',1442,'Quận 1','20101','Phường Bến Nghé','61 Đường kiểm thử local',true,true,stamp);
  INSERT INTO shop_wallets(id,shop_id,balance,locked_balance,currency,updated_at)
  VALUES(wallet,shop,20000000,0,'VND',stamp);
  INSERT INTO shop_wallet_transactions(id,wallet_id,type,amount,balance_before,balance_after,ref_type,description,created_at)
  VALUES('b6100000-0000-0000-0000-000000000004',wallet,'ManualAdjustment',20000000,0,20000000,'T61-QA','T61-QA số dư thử nghiệm, không phải tiền thật',stamp);

  SELECT k.id AS sku_id,k.sku_code,k.sell_price,k.original_price,k.attributes_json,p.name,p.thumbnail_url
  INTO STRICT product FROM skus k JOIN spus p ON p.id=k.spu_id
  WHERE k.shop_id=shop AND k.is_active AND k.deleted_at IS NULL ORDER BY k.sell_price,k.id LIMIT 1;
  SELECT * INTO STRICT inv FROM inventories WHERE sku_id=product.sku_id FOR UPDATE;
  IF inv.qty_on_hand-inv.reserved_qty < 2 THEN RAISE EXCEPTION 'Need two available units for QA reservations'; END IF;
  address := jsonb_build_object('recipientName','T61-QA Người nhận','phone','0900000061','provinceId',202,
    'provinceName','Hồ Chí Minh','districtId',1442,'districtName','Quận 1','wardCode','20101','wardName','Phường Bến Nghé','detailAddress','61 Đường kiểm thử local');
  snapshot := jsonb_build_object('spuName',product.name,'skuCode',product.sku_code,'attributesJson',product.attributes_json,'thumbnailUrl',product.thumbnail_url);
  FOR n IN 1..7 LOOP
    parent := ('b6100001-0000-0000-0000-'||lpad(n::text,12,'0'))::uuid;
    sub := ('b6100002-0000-0000-0000-'||lpad(n::text,12,'0'))::uuid;
    line := ('b6100003-0000-0000-0000-'||lpad(n::text,12,'0'))::uuid;
    state := (ARRAY['PendingPayment','Confirmed','Completed','ReturnRequested','ReturnRequested','Delivered','Cancelled'])[n];
    INSERT INTO parent_orders(id,buyer_id,order_code,shipping_address_json,total_item_amount,total_shipping_fee,
      total_discount_amount,grand_total,currency_code,payment_status,note,created_at,updated_at)
    VALUES(parent,buyer,'T61-QA-P'||n,address,product.sell_price,30000,0,product.sell_price+30000,'VND',
      CASE WHEN n IN(1,2) THEN 'Pending' WHEN n=7 THEN 'Cancelled' ELSE 'Paid' END,'T61-QA local only',stamp,stamp);
    INSERT INTO sub_orders(id,parent_order_id,shop_id,sub_order_code,warehouse_address_id,order_source,
      item_amount,shipping_fee,shop_discount_amount,platform_discount_amount,sub_total,seller_earnings,status,
      confirmed_at,delivered_at,completed_at,cancelled_at,created_at,updated_at)
    VALUES(sub,parent,shop,'T61-QA-S'||n,CASE WHEN n=2 THEN NULL ELSE warehouse END,'Online',product.sell_price,30000,0,0,
      product.sell_price+30000,CASE WHEN n IN(3,4,5,6) THEN product.sell_price ELSE 0 END,state,
      CASE WHEN n<>1 THEN stamp END,CASE WHEN n IN(3,4,5,6) THEN stamp END,CASE WHEN n=3 THEN stamp END,
      CASE WHEN n=7 THEN stamp END,stamp,stamp);
    INSERT INTO order_items(id,sub_order_id,sku_id,sku_snapshot_json,quantity,unit_price,original_price,discount_amount,line_total,status)
    VALUES(line,sub,product.sku_id,snapshot,1,product.sell_price,product.original_price,0,product.sell_price,'Active');
    INSERT INTO payments(id,parent_order_id,method,amount,status,expired_at,created_at,updated_at)
    VALUES(gen_random_uuid(),parent,CASE WHEN n=1 THEN 'MoMo' ELSE 'COD' END,product.sell_price+30000,
      CASE WHEN n IN(3,4,5,6) THEN 'Success' WHEN n=7 THEN 'Failed' ELSE 'Pending' END,
      CASE WHEN n=1 THEN stamp+interval '1 day' END,stamp,stamp);
    INSERT INTO order_status_histories(id,sub_order_id,to_status,changed_by_role,note,changed_at)
    VALUES(gen_random_uuid(),sub,state,'System','T61-QA seed snapshot',stamp);
    IF n IN(4,5) THEN
      INSERT INTO order_returns(id,sub_order_id,buyer_id,reason,evidence_urls,status,refund_amount,created_at,updated_at)
      VALUES(('b6100004-0000-0000-0000-'||lpad(n::text,12,'0'))::uuid,sub,buyer,'NotAsDescribed',ARRAY[]::text[],
        CASE WHEN n=4 THEN 'Pending' ELSE 'AdminDispute' END,product.sell_price,stamp-interval '2 days 2 hours',stamp);
      INSERT INTO order_return_items(id,return_id,order_item_id,quantity,reason_detail)
      VALUES(gen_random_uuid(),('b6100004-0000-0000-0000-'||lpad(n::text,12,'0'))::uuid,line,1,'T61-QA thử hoàn hàng local');
    END IF;
    IF n=3 THEN
      INSERT INTO reviews(id,order_item_id,sku_id,shop_id,buyer_id,rating,title,content,edit_count,is_visible,created_at,updated_at)
      VALUES('b6100005-0000-0000-0000-000000000001',line,product.sku_id,shop,buyer,4,'T61-QA','T61-QA đánh giá để thử phản hồi một lần',0,true,stamp,stamp);
    END IF;
  END LOOP;
  INSERT INTO inventory_histories(id,inventory_id,sku_id,operation_id,change_type,qty_before,qty_change,
    reserved_before,reserved_change,qty_after,ref_type,note,created_by,created_at)
  VALUES(gen_random_uuid(),inv.id,product.sku_id,gen_random_uuid(),'ReserveAdd',inv.qty_on_hand,0,inv.reserved_qty,2,
    inv.qty_on_hand,'T61-QA','T61-QA reserve PendingPayment + Confirmed',seller,stamp);
  UPDATE inventories SET reserved_qty=reserved_qty+2,version=version+1,last_updated=stamp WHERE id=inv.id;
  INSERT INTO discounts(id,shop_id,code,name,discount_type,discount_value,min_order_amount,max_discount_amount,
    max_uses,used_count,per_user_limit,applies_to,valid_from,valid_to,is_public,is_active,created_at,updated_at)
  VALUES('b6100006-0000-0000-0000-000000000001',shop,'T61QASEED','T61-QA voucher local','PercentCart',10,100000,50000,
    100,0,1,'AllProducts',stamp-interval '1 hour',stamp+interval '30 days',true,true,stamp,stamp);
  INSERT INTO flash_sale_campaigns(id,name,start_at,end_at,status,created_at,updated_at)
  VALUES('b6100007-0000-0000-0000-000000000001','T61-QA chiến dịch local',stamp-interval '1 hour',stamp+interval '7 days','Active',stamp,stamp);
  INSERT INTO flash_sale_campaigns(id,name,start_at,end_at,status,created_at,updated_at)
  VALUES('b6100007-0000-0000-0000-000000000002','T61-QA chiến dịch sắp tới',stamp+interval '2 days',stamp+interval '3 days','Scheduled',stamp,stamp);
END $qa$;
COMMIT;
