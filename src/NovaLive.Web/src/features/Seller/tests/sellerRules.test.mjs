import assert from 'node:assert/strict'
import { after, before, test } from 'node:test'
import { createServer } from 'vite'

let server, product, marketing, format, business
before(async () => {
  server = await createServer({ server: { middlewareMode: true, hmr: false }, appType: 'custom', logLevel: 'error' })
  ;[product, marketing, format, business] = await Promise.all([
    server.ssrLoadModule('/src/features/Seller/utils/product.ts'),
    server.ssrLoadModule('/src/features/Seller/utils/marketing.ts'),
    server.ssrLoadModule('/src/features/Seller/utils/format.ts'),
    server.ssrLoadModule('/src/features/Seller/utils/business.ts')
  ])
})
after(async () => { await server?.close() })

test('A product without axes still gets exactly one default SKU', () => {
  const rows = product.buildSkuMatrix([], 'NOVA')
  assert.equal(rows.length, 1)
  assert.equal(rows[0].attributesJson, '{}')
})
test('Variant matrix is the Cartesian product with unique SKU codes', () => {
  const rows = product.buildSkuMatrix([{ name: 'Màu', values: ['Trắng', 'Đen'] }, { name: 'Size', values: ['S', 'M', 'L'] }], 'TEE')
  assert.equal(rows.length, 6)
  assert.equal(new Set(rows.map(row => row.skuCode)).size, 6)
  assert.equal(new Set(rows.map(row => row.attributesJson)).size, 6)
  assert.deepEqual(JSON.parse(rows[5].attributesJson), { Màu: 'Đen', Size: 'L' })
})
test('Empty variant values and duplicate axis names are rejected', () => {
  assert.throws(() => product.buildSkuMatrix([{ name: 'Size', values: [] }], 'TEE'))
  assert.throws(() => product.buildSkuMatrix([{ name: 'Size', values: ['S'] }, { name: ' size ', values: ['M'] }], 'TEE'))
  assert.equal(product.buildSkuMatrix([{ name: 'Màu', values: ['Đen', ' Đen ', ''] }], 'TEE').length, 1)
})
test('Matrix size guard prevents accidental huge forms', () => {
  assert.throws(() => product.buildSkuMatrix([{ name: 'A', values: Array.from({ length: 201 }, (_, i) => String(i)) }], 'TEE'))
})
test('SKU constraints accept zero prices but require positive integer weight', () => {
  const sku = { skuCode: 'NOVA-1', attributesJson: '{}', sellPrice: 0, originalPrice: 0, weightGram: 1, initialStock: 0, images: [] }
  assert.equal(product.skuSchema.safeParse(sku).success, true)
  for (const patch of [{ sellPrice: -1 }, { originalPrice: -1 }, { weightGram: 0 }, { weightGram: 0.5 }, { initialStock: -1 }]) assert.equal(product.skuSchema.safeParse({ ...sku, ...patch }).success, false)
})
test('A product must have at least one SKU and unique case-insensitive codes', () => {
  const base = { name: 'Áo Nova', description: 'Áo cotton', categoryId: 'category', brand: null, thumbnailUrl: '/fixture.png', attributesConfigJson: null, attributes: [] }
  const sku = product.buildSkuMatrix([], 'TEE')[0]
  assert.equal(product.productSchema.safeParse({ ...base, skus: [] }).success, false)
  assert.equal(product.productSchema.safeParse({ ...base, skus: [sku, { ...sku, skuCode: sku.skuCode.toLowerCase() }] }).success, false)
})
const voucher = { code: 'NOVA10', name: 'Ưu đãi', discountType: 'PercentCart', discountValue: 10, minOrderAmount: 0, maxDiscountAmount: 50000, maxUses: null, perUserLimit: 1, validFrom: '2026-10-23T00:00:00.000Z', validTo: '2026-10-24T00:00:00.000Z', isPublic: true, appliesTo: 'AllProducts', targetIds: null }
test('Percentage vouchers require a positive cap and cannot exceed 100%', () => {
  assert.equal(marketing.discountSchema.safeParse(voucher).success, true)
  for (const patch of [{ discountValue: 101 }, { discountValue: 0 }, { maxDiscountAmount: null }, { maxDiscountAmount: 0 }]) assert.equal(marketing.discountSchema.safeParse({ ...voucher, ...patch }).success, false)
})
test('Voucher expiration must be strictly after its start', () => {
  assert.equal(marketing.discountSchema.safeParse({ ...voucher, validTo: voucher.validFrom }).success, false)
  assert.equal(marketing.discountSchema.safeParse({ ...voucher, validTo: '2026-10-22T23:59:59.000Z' }).success, false)
  assert.equal(marketing.discountSchema.safeParse({ ...voucher, discountType: 'FixedCart', maxDiscountAmount: null }).success, true)
})
test('Flash sale accepts an exact 10% discount and rejects a 9.9% discount', () => {
  assert.equal(marketing.validateFlashRegistration(900, 1000, 10, 10, 1), null)
  assert.notEqual(marketing.validateFlashRegistration(901, 1000, 10, 10, 1), null)
})
test('Flash registration closes at the start and rejects Active campaigns', () => {
  const now = Date.parse('2026-10-24T01:00:00Z')
  const upcoming = { status: 'Scheduled', startAt: '2026-10-24T01:00:01Z' }
  assert.equal(marketing.canRegisterCampaign(upcoming, now), true)
  assert.equal(marketing.canRegisterCampaign({ ...upcoming, startAt: '2026-10-24T01:00:00Z' }, now), false)
  assert.equal(marketing.canRegisterCampaign({ ...upcoming, status: 'Active' }, now), false)
  assert.equal(marketing.canRegisterCampaign({ ...upcoming, status: 'Cancelled' }, now), false)
})
test('Flash sale validates quantity against available stock and per-user limit', () => {
  assert.notEqual(marketing.validateFlashRegistration(900, 1000, 11, 10, 1), null)
  assert.notEqual(marketing.validateFlashRegistration(900, 1000, 10, 10, 11), null)
  assert.notEqual(marketing.validateFlashRegistration(0, 1000, 10, 10, 1), null)
})
test('Stock cannot fall below reserved; equality is valid', () => {
  assert.equal(business.validateStockAdjustment(10, 7, -3, 'ManualAdjust'), null)
  assert.notEqual(business.validateStockAdjustment(10, 7, -4, 'ManualAdjust'), null)
  assert.notEqual(business.validateStockAdjustment(10, 7, -1, 'Import'), null)
  assert.notEqual(business.validateStockAdjustment(10, 7, 0, 'ManualAdjust'), null)
})
test('Payout enforces 50,000 VND minimum, available balance, and whole currency units', () => {
  assert.equal(business.validatePayout(50000, 50000), true)
  for (const [amount, balance] of [[49999, 50000], [50001, 50000], [50000.5, 60000], [NaN, 100000]]) assert.equal(business.validatePayout(amount, balance), false)
})
test('Seller role is mandatory; Buyer and Admin alone cannot enter', () => {
  assert.equal(business.hasSellerRole(['Seller']), true)
  assert.equal(business.hasSellerRole(['seller']), true)
  assert.equal(business.hasSellerRole(undefined, 'Seller'), true)
  assert.equal(business.hasSellerRole(['Admin']), false)
  assert.equal(business.hasSellerRole(['Buyer']), false)
  assert.equal(business.hasSellerRole([]), false)
})
test('Mock fallback accepts only missing/unimplemented endpoints', () => {
  for (const status of [404, 501]) assert.equal(business.isMissingSellerEndpoint(status), true)
  for (const status of [undefined, 400, 401, 403, 409, 422, 500, 502]) assert.equal(business.isMissingSellerEndpoint(status), false)
})
test('UTC and timezone-less backend dates display in Vietnam, including midnight rollover', () => {
  assert.equal(format.dateText('2026-10-23T18:05:00Z'), '24/10/2026 01:05')
  assert.equal(format.dateText('2026-10-23T18:05:00'), '24/10/2026 01:05')
  assert.equal(format.dateText('invalid'), '—')
})
test('Seller return deadline is exactly three UTC days after creation', () => {
  assert.equal(format.returnDeadline('2026-10-23T23:00:00Z'), Date.parse('2026-10-26T23:00:00Z'))
})
test('Datetime-local inputs round-trip with UTC+7, independent of host timezone', () => {
  assert.equal(format.inputToUtc('2026-10-24T01:05'), '2026-10-23T18:05:00.000Z')
  assert.equal(format.localInput('2026-10-23T18:05:00Z'), '2026-10-24T01:05')
  assert.equal(format.dayToUtc('2026-10-24'), '2026-10-23T17:00:00.000Z')
  assert.equal(format.dayToUtc('2026-10-24', true), '2026-10-24T16:59:59.999Z')
})
