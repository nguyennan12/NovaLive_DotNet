// Auth + read-only Seller API checks. Does not create business data or print cookies.
const base = new URL(process.env.SELLER_LIVE_URL || 'http://127.0.0.1:5173/api/v1/')
if (!['localhost', '127.0.0.1', '[::1]'].includes(base.hostname)) throw new Error('Local loopback only')
if (!process.env.SELLER_DEMO_PASSWORD) throw new Error('Set SELLER_DEMO_PASSWORD for the local demo Seller')
const cookies = new Map()
const pending = []
let failures = 0
async function call(path, { method = 'GET', data, csrf = true, expected = 200 } = {}) {
  const response = await fetch(new URL(path, base), {
    method, headers: {
      Cookie: [...cookies].map(([key, value]) => `${key}=${value}`).join('; '),
      ...(csrf ? { 'X-Requested-With': 'XMLHttpRequest' } : {}),
      ...(data ? { 'Content-Type': 'application/json' } : {})
    }, ...(data ? { body: JSON.stringify(data) } : {}), signal: AbortSignal.timeout(20_000)
  })
  for (const cookie of response.headers.getSetCookie()) {
    const [key, value] = cookie.split(';')[0].split('=')
    if (value) cookies.set(key, value)
    else cookies.delete(key)
  }
  const text = await response.text()
  const body = text ? JSON.parse(text) : null
  console.log(`${method} ${path}: ${response.status}`)
  if (response.status !== expected) {
    failures++
    console.error(body?.detail || body?.error?.message || 'Unexpected status')
  }
  return body?.data
}
await call('auth/me', { expected: 401 })
await call('auth/login', { method: 'POST', data: { emailOrPhone: process.env.SELLER_DEMO_EMAIL || 'seller@novalive.local', password: process.env.SELLER_DEMO_PASSWORD } })
try {
  const user = await call('auth/me')
  if (!user?.roles?.includes('Seller')) throw new Error('The authenticated demo account must have Seller role')
  await call('auth/refresh', { method: 'POST', csrf: false, expected: 403 })
  const oldRefresh = cookies.get('nova_rt')
  await call('auth/refresh', { method: 'POST' })
  if (!oldRefresh || oldRefresh === cookies.get('nova_rt')) throw new Error('Refresh cookie did not rotate')
  const products = await call('seller/products?page=1&size=2')
  if (products?.items?.length) {
    await call(`seller/products/${products.items[0].id}`)
    await call(`products/${products.items[0].id}/reviews?page=1&size=50`)
  }
  for (const path of [
    'seller/shop/me', 'seller/shop/addresses', 'categories',
    'seller/products?page=2&size=2&keyword=Apple', 'seller/inventory?page=1&size=2',
    'seller/inventory?lowStock=true&page=1&size=2', 'seller/inventory/histories?page=1&size=2',
    'seller/orders?page=1&size=2', 'seller/orders?page=2&size=2', 'seller/orders?status=Confirmed&page=1&size=2',
    'seller/returns?page=1&size=2', 'seller/returns?status=Completed&page=1&size=2',
    'seller/discounts?page=1&size=2', 'seller/discounts?status=active&page=1&size=2',
    'flash-sales/active', 'seller/flash-sales/registrations?page=1&size=2',
    'seller/wallet', 'seller/wallet/transactions?type=OrderRefund&page=1&size=2'
  ]) await call(path)
  for (const path of ['seller/reports/dashboard', 'notifications?page=1&size=2', 'seller/livestreams?page=1&size=2', 'seller/reviews?page=1&size=2', 'seller/wallet/payouts?page=1&size=2']) {
    await call(path, { expected: 404 })
    pending.push(path)
  }
  await call('auth/logout', { method: 'POST', csrf: false, expected: 403 })
} finally {
  await call('auth/logout', { method: 'POST' })
}
await call('auth/me', { expected: 401 })
await call('auth/refresh', { method: 'POST', expected: 401 })
console.log(`Pending backend endpoints (404 expected, not functional): ${pending.length}`)
process.exitCode = failures ? 1 : 0
