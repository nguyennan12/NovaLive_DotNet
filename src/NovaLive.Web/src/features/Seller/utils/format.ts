import { SELLER_TIMEZONE, RETURN_RESPONSE_MS } from '../constants'
import type { Tracking } from '../types/contracts'

export const money = (amount: number) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 }).format(Object.is(amount, -0) ? 0 : amount)
export function dateText(value: string | null | undefined, withTime = true) {
  if (!value) return '—'
  const normalized = /(?:Z|[+-]\d{2}:?\d{2})$/i.test(value) ? value : `${value}Z`
  const date = new Date(normalized)
  if (!Number.isFinite(date.getTime())) return '—'
  const parts = new Intl.DateTimeFormat('vi-VN', {
    timeZone: SELLER_TIMEZONE, day: '2-digit', month: '2-digit', year: 'numeric',
    ...(withTime ? { hour: '2-digit', minute: '2-digit', hourCycle: 'h23' as const } : {})
  }).formatToParts(date)
  const part = (type: Intl.DateTimeFormatPartTypes) => parts.find(item => item.type === type)?.value || ''
  return `${part('day')}/${part('month')}/${part('year')}${withTime ? ` ${part('hour')}:${part('minute')}` : ''}`
}
export function utcDate(value: string) {
  return new Date(/(?:Z|[+-]\d{2}:?\d{2})$/i.test(value) ? value : `${value}Z`)
}
export const returnDeadline = (createdAt: string) => utcDate(createdAt).getTime() + RETURN_RESPONSE_MS
export function localInput(value: string) {
  const parts = new Intl.DateTimeFormat('sv-SE', { timeZone: SELLER_TIMEZONE, year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', hourCycle: 'h23' }).format(utcDate(value))
  return parts.replace(' ', 'T')
}
export const inputToUtc = (value: string) => new Date(`${value}+07:00`).toISOString()
export const dayToUtc = (value: string, end = false) => value ? new Date(`${value}T${end ? '23:59:59.999' : '00:00:00'}+07:00`).toISOString() : undefined
export const todayInput = () => new Intl.DateTimeFormat('sv-SE', { timeZone: SELLER_TIMEZONE }).format(new Date())
export function normalizeSellerTracking(result: Tracking | Tracking[] | null | undefined): Tracking {
  // The shared backend query returns a list; each seller sub-order has one shipment.
  const tracking = Array.isArray(result) ? result[0] : result
  if (!tracking) throw new Error('Đơn hàng chưa có thông tin vận chuyển.')
  return tracking
}
export function snapshot(json: string): { name: string; variant: string; image: string } {
  try {
    const value: unknown = JSON.parse(json)
    if (!value || typeof value !== 'object') throw new Error('Invalid snapshot')
    const obj = value as Record<string, unknown>
    const text = (...keys: string[]) => keys.map(key => obj[key]).find((value): value is string => typeof value === 'string') || ''
    const code = text('skuCode', 'SkuCode')
    let variant = ''
    try {
      const raw: unknown = obj.attributesJson ?? obj.AttributesJson
      const attributes: unknown = typeof raw === 'string' ? JSON.parse(raw) : raw
      if (attributes && typeof attributes === 'object') variant = Object.entries(attributes).map(([name, value]) => `${name}: ${String(value)}`).join(', ')
    } catch { variant = text('attributesJson', 'AttributesJson') }
    return { name: text('spuName', 'SpuName', 'productName', 'name', 'Name') || 'Sản phẩm', variant: [code, variant].filter(Boolean).join(' · '), image: text('thumbnailUrl', 'ThumbnailUrl', 'imageUrl') }
  } catch { return { name: 'Sản phẩm', variant: '', image: '' } }
}
