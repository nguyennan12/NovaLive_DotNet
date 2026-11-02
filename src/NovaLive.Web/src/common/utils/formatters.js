export const resolveMediaUrl = (url) => {
  if (!url || typeof url !== 'string') return ''
  if (url.startsWith('blob:') || url.startsWith('data:')) return url
  if (url.includes('cdn.novalive.vn') && typeof window !== 'undefined' && window.location.hostname === 'localhost') {
    return url.replace('https://cdn.novalive.vn', 'http://localhost:9000').replace('http://cdn.novalive.vn', 'http://localhost:9000')
  }
  return url
}

export const formatDuration = (seconds) => {
  const h = Math.floor(seconds / 3600)
  const m = Math.floor((seconds % 3600) / 60)
  const s = seconds % 60
  if (h > 0) return `${h}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
  return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '00')}`
}

export function formatValue(type, value) {
  if (type === 'percentage') return `${value}%`
  if (value >= 1000) return `${Math.round(value / 1000)}k ₫`
  return `${value} ₫`
}


export function formatDate(dateStr) {
  if (!dateStr) return '—'
  return new Date(dateStr).toLocaleDateString('vi-VN', {
    day: '2-digit',
    month: 'short',
    year: 'numeric'
  })
}

export const formatVND = (value) => {
  if (!value && value !== 0) return '0 ₫'

  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(value)
}

export const getStockStatus = (stock) => {
  if (stock === 0) return 'out'
  if (stock <= 10) return 'low'
  return 'in'
}

export const formatStringToSlug = (text) => {
  if (!text) return ''

  return text
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/đ/g, 'd')
    .replace(/[^a-z0-9\s-]/g, '')
    .trim()
    .replace(/\s+/g, '-')
    .replace(/-+/g, '-')
}

export const formatLiveDurationClock = (start, end) => {
  if (!start) return '—'

  const s = new Date(start)
  const e = end ? new Date(end) : new Date()

  let totalSeconds = Math.max(0, Math.floor((e - s) / 1000))

  const hours = Math.floor(totalSeconds / 3600)
  totalSeconds %= 3600

  const minutes = Math.floor(totalSeconds / 60)
  const seconds = totalSeconds % 60

  return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`
}


export const formatSold = (n) => {
  if (!n) return '0'
  if (n >= 1000) return `${(n / 1000).toFixed(n >= 10000 ? 0 : 1)}k`
  return `${n}`
}

// Compact currency for stats displays: 1.500.000đ → "1.5tr", 500.000đ → "500k"
export const formatCompactVND = (amount) => {
  if (!amount) return '0 ₫'
  if (amount >= 1_000_000) return `${(amount / 1_000_000).toFixed(1)}tr`
  if (amount >= 1_000) return `${Math.round(amount / 1_000)}k`
  return formatVND(amount)
}


export const getApiErrorMessage = (error, defaultMessage = 'Đã có lỗi xảy ra. Vui lòng thử lại.') => {
  if (!error) return defaultMessage
  if (typeof error === 'string') return error

  const data = error.response?.data
  if (typeof data === 'string') return data

  const errSource = data?.validationErrors || data?.errors
  if (errSource) {
    if (Array.isArray(errSource) && errSource.length > 0) {
      return typeof errSource[0] === 'string' ? errSource[0] : (errSource[0]?.message || defaultMessage)
    }
    if (typeof errSource === 'object') {
      const firstKey = Object.keys(errSource)[0]
      if (firstKey) {
        const val = errSource[firstKey]
        return Array.isArray(val) ? val[0] : String(val)
      }
    }
  }

  if (data?.detail) return data.detail
  if (data?.message && data.message !== 'A validation problem occurred.') return data.message
  if (data?.title && data.title !== 'One or more validation errors occurred.') return data.title
  if (error.message) return error.message

  return defaultMessage
}
