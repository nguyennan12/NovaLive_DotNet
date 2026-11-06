-- One-time, idempotent repair for paid orders missing their OrderRevenue ledger entry.
-- Run with psql against the intended database. Does not rerun or modify demo seed data.
BEGIN;

-- Serialize with payment/COD settlement handlers, which lock the parent order first.
SELECT parent.id
FROM parent_orders parent
WHERE parent.payment_status = 'Paid'
  AND EXISTS (
    SELECT 1
    FROM payments payment
    WHERE payment.parent_order_id = parent.id
      AND payment.status = 'Success'
      AND payment.method IN ('MoMo', 'VietQR', 'COD'))
  AND EXISTS (
    SELECT 1
    FROM sub_orders sub
    WHERE sub.parent_order_id = parent.id
      AND sub.status NOT IN ('Cancelled', 'PendingPayment')
      AND (sub.delivered_at IS NOT NULL OR EXISTS (
        SELECT 1 FROM payments online_payment
        WHERE online_payment.parent_order_id = parent.id
          AND online_payment.status = 'Success'
          AND online_payment.method IN ('MoMo', 'VietQR')))
      AND NOT EXISTS (
        SELECT 1 FROM shop_wallet_transactions tx
        WHERE tx.ref_type = 'SubOrder' AND tx.ref_id = sub.id
          AND tx.type = 'OrderRevenue'))
ORDER BY parent.id
FOR UPDATE OF parent;

CREATE TEMP TABLE _missing_order_revenue ON COMMIT DROP AS
SELECT DISTINCT ON (sub.id)
       sub.id AS sub_order_id,
       sub.shop_id,
       wallet.id AS wallet_id,
       sub.sub_total AS amount
FROM sub_orders sub
JOIN parent_orders parent ON parent.id = sub.parent_order_id
JOIN payments payment ON payment.parent_order_id = parent.id
  AND payment.status = 'Success'
  AND payment.method IN ('MoMo', 'VietQR', 'COD')
JOIN shop_wallets wallet ON wallet.shop_id = sub.shop_id
WHERE parent.payment_status = 'Paid'
  AND sub.status NOT IN ('Cancelled', 'PendingPayment')
  AND (payment.method <> 'COD' OR sub.delivered_at IS NOT NULL)
  AND sub.sub_total > 0
  AND NOT EXISTS (
    SELECT 1 FROM shop_wallet_transactions tx
    WHERE tx.ref_type = 'SubOrder' AND tx.ref_id = sub.id
      AND tx.type = 'OrderRevenue')
ORDER BY sub.id, payment.paid_at DESC NULLS LAST, payment.created_at DESC;

DO $$
BEGIN
  IF EXISTS (
    SELECT 1 FROM sub_orders sub
    JOIN parent_orders parent ON parent.id = sub.parent_order_id
    JOIN payments payment ON payment.parent_order_id = parent.id
    WHERE parent.payment_status = 'Paid'
      AND payment.status = 'Success'
      AND payment.method IN ('MoMo', 'VietQR', 'COD')
      AND sub.status NOT IN ('Cancelled', 'PendingPayment')
      AND (payment.method <> 'COD' OR sub.delivered_at IS NOT NULL)
      AND sub.sub_total > 0
      AND NOT EXISTS (
        SELECT 1 FROM shop_wallet_transactions tx
        WHERE tx.ref_type = 'SubOrder' AND tx.ref_id = sub.id
          AND tx.type = 'OrderRevenue')
      AND NOT EXISTS (
        SELECT 1 FROM shop_wallets wallet WHERE wallet.shop_id = sub.shop_id))
  THEN
    RAISE EXCEPTION 'A qualifying paid order has no shop wallet; no revenue was backfilled.';
  END IF;
END $$;

-- Hold the affected wallet rows until the credits and balances commit together.
SELECT wallet.id
FROM shop_wallets wallet
WHERE wallet.id IN (SELECT DISTINCT wallet_id FROM _missing_order_revenue)
ORDER BY wallet.id
FOR UPDATE OF wallet;

WITH sequenced AS (
  SELECT candidate.*,
         wallet.balance + COALESCE(SUM(candidate.amount) OVER (
           PARTITION BY candidate.wallet_id
           ORDER BY candidate.sub_order_id
           ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) AS balance_before,
         ROW_NUMBER() OVER (
           PARTITION BY candidate.wallet_id ORDER BY candidate.sub_order_id) AS sequence_no
  FROM _missing_order_revenue candidate
  JOIN shop_wallets wallet ON wallet.id = candidate.wallet_id
), inserted AS (
  INSERT INTO shop_wallet_transactions (
    id, wallet_id, type, amount, balance_before, balance_after,
    ref_type, ref_id, description, created_at)
  SELECT md5('novalive-order-revenue-backfill:' || sub_order_id::text)::uuid,
         wallet_id, 'OrderRevenue', amount, balance_before, balance_before + amount,
         'SubOrder', sub_order_id, 'Bù doanh thu đơn đã thanh toán; đối soát sổ ví.',
         now() + (sequence_no * interval '1 microsecond')
  FROM sequenced
  ON CONFLICT (id) DO NOTHING
  RETURNING wallet_id, amount
), credits AS (
  SELECT wallet_id, SUM(amount) AS amount
  FROM inserted
  GROUP BY wallet_id
), updated_wallets AS (
  UPDATE shop_wallets wallet
  SET balance = wallet.balance + credits.amount,
      updated_at = now()
  FROM credits
  WHERE wallet.id = credits.wallet_id
  RETURNING wallet.shop_id, credits.amount AS credited_amount
)
SELECT shop_id, credited_amount
FROM updated_wallets
ORDER BY shop_id;

COMMIT;
