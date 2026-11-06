-- Enrich flash_sale_items with realistic, varied quantity and sold_qty for campaigns
BEGIN;

-- For active campaign: realistic sold numbers (some close to sellout, some half, some started)
WITH numbered_active AS (
    SELECT id, ROW_NUMBER() OVER (ORDER BY id) AS rn
    FROM flash_sale_items
    WHERE campaign_id = '21af9474-aadf-4e60-2c6d-f9b2e4955e83'
)
UPDATE flash_sale_items fsi
SET 
    quantity = CASE 
        WHEN na.rn % 4 = 1 THEN 30
        WHEN na.rn % 4 = 2 THEN 25
        WHEN na.rn % 4 = 3 THEN 20
        ELSE 35
    END,
    sold_qty = CASE 
        WHEN na.rn % 4 = 1 THEN CASE WHEN na.rn % 2 = 1 THEN 28 ELSE 22 END -- out of 30
        WHEN na.rn % 4 = 2 THEN CASE WHEN na.rn % 2 = 1 THEN 23 ELSE 17 END -- out of 25
        WHEN na.rn % 4 = 3 THEN CASE WHEN na.rn % 2 = 1 THEN 18 ELSE 12 END -- out of 20
        ELSE CASE WHEN na.rn % 2 = 1 THEN 29 ELSE 19 END                     -- out of 35
    END
FROM numbered_active na
WHERE fsi.id = na.id;

-- For ended campaign: high sold numbers
WITH numbered_ended AS (
    SELECT id, ROW_NUMBER() OVER (ORDER BY id) AS rn
    FROM flash_sale_items
    WHERE campaign_id = 'a6a6ce27-224e-f082-71b2-f19786caf0b9'
)
UPDATE flash_sale_items fsi
SET 
    quantity = 30,
    sold_qty = CASE 
        WHEN ne.rn % 3 = 1 THEN 30 -- Sold out
        WHEN ne.rn % 3 = 2 THEN 28
        ELSE 26
    END
FROM numbered_ended ne
WHERE fsi.id = ne.id;

COMMIT;
