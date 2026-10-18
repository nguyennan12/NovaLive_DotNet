import LocalOfferRoundedIcon from '@mui/icons-material/LocalOfferRounded'
import LocalShippingRoundedIcon from '@mui/icons-material/LocalShippingRounded'

export const toDateKey = (value) => {
  if (!value) return ''
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '' : date.toISOString().slice(0, 10)
}

export const normalizeDiscount = (discount) => {
  const startDateRaw = discount.validFrom ?? discount.discount_start_date ?? discount.startDate ?? ''
  const endDateRaw = discount.validTo ?? discount.discount_end_date ?? discount.endDate ?? ''
  const startDate = toDateKey(startDateRaw)
  const endDate = toDateKey(endDateRaw)
  const isExpired = startDateRaw && endDateRaw ? new Date(endDateRaw).getTime() < Date.now() : false

  return {
    id: discount.id ?? discount._id ?? discount.discount_code ?? `d${Date.now()}`,
    shopId: discount.shopId ?? discount.shop_id ?? null,
    name: discount.name ?? discount.discount_name ?? '',
    code: discount.code ?? discount.discount_code ?? null,
    category: discount.category ?? (['FreeShip', 'PercentShip'].includes(discount.discountType) ? 'freeship' : 'product'),
    type: discount.type ?? (['FixedCart', 'FreeShip'].includes(discount.discountType) ? 'fixed' : 'percentage'),
    value: discount.value ?? discount.discountValue ?? discount.discount_value ?? '',
    status: discount.status ?? (isExpired ? 'expired' : (discount.isActive ?? discount.discount_is_active) === false ? 'draft' : 'active'),
    startDate,
    endDate,
    usageLimit: discount.usageLimit ?? discount.maxUses ?? discount.discount_max_uses ?? '',
    usedCount: discount.usedCount ?? discount.discount_uses_count ?? 0,
    minOrder: discount.minOrder ?? discount.minOrderAmount ?? discount.discount_min_value ?? 0,
    maxValue: discount.maxValue ?? discount.maxDiscountAmount ?? discount.discount_max_value ?? 0,
    description: discount.description ?? discount.discount_description ?? ''
  }
}

export const fmtValue = (category, type, value) => {
  if (category === 'freeship' && type !== 'percentage') return 'FREE'
  return type === 'percentage' ? `${value}%` : `${(value / 1000).toFixed(0)}K`
}

export const fmtValueSub = (category, type, value) => {
  if (category === 'freeship') return type === 'percentage' ? 'GIẢM SHIP %' : `Ship −${(value / 1000).toFixed(0)}K`
  return type === 'percentage' ? 'GIẢM %' : 'GIẢM TIỀN'
}

export const CATEGORY_THEME = {
  product: {
    stubBg: 'rgba(29, 123, 255, 0.8)',
    stubAccent: '#ffffff',
    stubText: '#ffffff',
    dash: 'rgba(255, 255, 255, 0.3)',
    notchBg: '#e0eeff',
    icon: LocalOfferRoundedIcon,
    chipBg: '#dbeafe',
    chipText: 'secondary.main',
    chipLabel: 'Sản phẩm',
    barColor: 'secondary.main'
  },
  freeship: {
    stubBg: 'rgba(255, 172, 29, 0.8)',
    stubAccent: '#ffffff',
    stubText: '#ffffff',
    dash: 'rgba(255, 255, 255, 0.3)',
    notchBg: '#faf0d1ff',
    icon: LocalShippingRoundedIcon,
    chipBg: '#fef3c7',
    chipText: 'fourth.main',
    chipLabel: 'Free Ship',
    barColor: 'fourth.main'
  }
}
