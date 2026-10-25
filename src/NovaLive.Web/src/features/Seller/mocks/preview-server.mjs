// Isolated, loopback-only UI test server. Never imported by the application bundle.
import { createServer } from 'vite'

process.env.VITE_SELLER_API_MODE = 'mock'
process.env.VITE_SELLER_ORDER_HUB_AUTOJOIN = 'false'
const port = Number(process.env.SELLER_PREVIEW_PORT || 5177)
process.env.VITE_SIGNALR_URL = `http://127.0.0.1:${port}`
const vite = await createServer({
  server: { host: '127.0.0.1', port, strictPort: true },
  plugins: [{
    name: 'seller-isolated-preview',
    configureServer(server) {
      server.middlewares.use((req, res, next) => {
        if (req.url?.split('?')[0] === '/api/v1/auth/login' && req.method === 'POST') {
          let body = ''
          req.on('data', chunk => { body += chunk })
          req.on('end', () => {
            try {
              const input = JSON.parse(body)
              if (!['seller@qa.local', 'buyer@qa.local', 'admin@qa.local'].includes(input.emailOrPhone) || input.password !== 'SellerDemo123!') {
                res.writeHead(401, { 'Content-Type': 'application/json' }); res.end(JSON.stringify({ detail: 'Chỉ dùng tài khoản thử ghi trong tài liệu Seller.' })); return
              }
              const role = input.emailOrPhone.startsWith('seller') ? 'Seller' : input.emailOrPhone.startsWith('admin') ? 'Admin' : 'Buyer'
              res.writeHead(200, { 'Content-Type': 'application/json' })
              res.end(JSON.stringify({ success: true, data: { accessToken: 'seller-preview-only-not-a-jwt', user: { id: `qa-${role.toLowerCase()}`, email: input.emailOrPhone, fullName: 'Tài khoản kiểm thử Seller', roles: [role], shopId: role === 'Buyer' ? null : '10000000-0000-4000-8000-000000000005' } } }))
            } catch { res.writeHead(400); res.end() }
          })
          return
        }
        // No test request may reach a real API, upload service, or SignalR hub.
        if (req.url?.startsWith('/api/') || req.url?.startsWith('/hubs/')) {
          res.writeHead(501, { 'Content-Type': 'application/json' })
          res.end(JSON.stringify({ detail: 'Isolated Seller UI preview. No backend requests are forwarded.' }))
          return
        }
        next()
      })
    }
  }]
})
await vite.listen()
vite.printUrls()
process.stdout.write('Seller preview: seller@qa.local / SellerDemo123!\nBuyer/Admin guard tests: buyer@qa.local or admin@qa.local, same test password.\n')
for (const signal of ['SIGINT', 'SIGTERM']) process.on(signal, () => { void vite.close().then(() => process.exit(0)) })
