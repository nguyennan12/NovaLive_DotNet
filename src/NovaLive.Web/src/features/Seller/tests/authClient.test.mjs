import assert from 'node:assert/strict'
import { after, before, test } from 'node:test'
import { createServer } from 'vite'

let server, client, store
before(async () => {
  server = await createServer({ server: { middlewareMode: true, hmr: false }, ssr: { noExternal: ['redux-persist'] }, appType: 'custom', logLevel: 'error' })
  client = (await server.ssrLoadModule('/src/common/apis/custom/authorizeAxios.js')).default
  store = (await server.ssrLoadModule('/src/store/store.js')).store
})
after(async () => { await server?.close() })

async function expiredRequests(refreshData) {
  store.dispatch({ type: 'user/setCurrentUser', payload: { id: 'demo', roles: ['Seller'], accessToken: 'expired-legacy' } })
  let refreshes = 0
  const retryHeaders = []
  client.defaults.adapter = async config => {
    assert.equal(config.headers['X-Requested-With'], 'XMLHttpRequest')
    assert.equal(config.withCredentials, true)
    if (config.url === 'auth/refresh') {
      refreshes++
      await new Promise(resolve => setTimeout(resolve, 10))
      return { status: 200, data: { data: refreshData }, headers: {}, config }
    }
    if (!config._retry) throw Object.assign(new Error('Expired'), { config, response: { status: 401 } })
    retryHeaders.push(config.headers.Authorization)
    return { status: 200, data: { data: { ok: true } }, headers: {}, config }
  }
  const values = await Promise.all([client.get('seller/shop/me'), client.get('seller/wallet')])
  assert.deepEqual(values, [{ ok: true }, { ok: true }])
  assert.equal(refreshes, 1)
  return retryHeaders
}

test('Cookie refresh removes stale Bearer headers on every queued retry', async () => {
  assert.deepEqual(await expiredRequests({ user: { id: 'demo', roles: ['Seller'] }, expiresAt: '2030-01-01' }), [undefined, undefined])
  assert.equal(store.getState().user.currentUser.accessToken, undefined)
})
test('Legacy Bearer refresh still uses the newly returned token', async () => {
  assert.deepEqual(await expiredRequests({ accessToken: 'renewed' }), ['Bearer renewed', 'Bearer renewed'])
  assert.equal(store.getState().user.currentUser.accessToken, 'renewed')
})
