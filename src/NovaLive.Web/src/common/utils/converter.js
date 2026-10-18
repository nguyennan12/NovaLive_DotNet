

export const toDateInputValue = (value) => {
  if (!value) return ''
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? '' : date.toISOString().slice(0, 10)
}


export const toQueryString = (params = {}) => {
  const searchParams = new URLSearchParams()

  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') return

    if (key === 'limit') {
      const numericLimit = Number(value)
      if (Number.isFinite(numericLimit)) {
        const safeLimit = Math.max(1, Math.min(100, Math.trunc(numericLimit)))
        searchParams.set('size', String(safeLimit))
        searchParams.set('limit', String(safeLimit))
        return
      }
    }

    searchParams.set(key, String(value))
  })

  return searchParams.toString()
}


export const toDefaultValuesDiscount = (discount = {}) => ({
  discount_name: discount.discount_name ?? discount.name ?? '',
  discount_description: discount.discount_description ?? discount.description ?? '',
  discount_code: discount.discount_code ?? discount.code ?? '',
  discount_start_date: toDateInputValue(discount.discount_start_date ?? discount.validFrom ?? discount.startDate),
  discount_end_date: toDateInputValue(discount.discount_end_date ?? discount.validTo ?? discount.endDate),
  discount_is_active: typeof discount.discount_is_active === 'boolean'
    ? discount.discount_is_active
    : (typeof discount.isActive === 'boolean' ? discount.isActive : (discount.status ? discount.status === 'active' : true)),
  discount_is_public: discount.discount_is_public ?? discount.isPublic ?? true,
  discount_target: discount.discount_target ?? (discount.category === 'freeship' || ['FreeShip', 'PercentShip'].includes(discount.discountType) ? 'shipping' : 'product'),
  discount_type: discount.discount_type
    ?? (discount.discountType ? (['FixedCart', 'FreeShip'].includes(discount.discountType) ? 'fixed_amount' : 'percentage') : (discount.type === 'fixed' || discount.type === 'amount' ? 'fixed_amount' : 'percentage')),
  discount_applies_to: discount.discount_applies_to ?? (discount.appliesTo === 'SpecificSpus' ? 'specific' : 'all'),
  discount_scope: discount.discount_scope ?? 'global',
  discount_shopId: discount.discount_shopId ?? discount.shopId ?? null,
  discount_min_value: discount.discount_min_value ?? discount.minOrderAmount ?? discount.minOrder ?? '',
  discount_value: discount.discount_value ?? discount.discountValue ?? discount.value ?? '',
  discount_max_value: discount.discount_max_value ?? discount.maxDiscountAmount ?? discount.maxValue ?? '',
  discount_max_uses: discount.discount_max_uses ?? discount.maxUses ?? discount.usageLimit ?? '',
  discount_uses_count: discount.discount_uses_count ?? discount.usedCount ?? 0,
  discount_users_used: discount.discount_users_used ?? [],
  discount_max_uses_per_user: discount.discount_max_uses_per_user ?? discount.perUserLimit ?? 1,
  discount_product_ids: discount.discount_product_ids ?? discount.targetIds ?? []
})

export const toApiPayloadDiscount = (data) => ({
  code: data.discount_code.trim().toUpperCase(),
  name: data.discount_name.trim(),
  discountType: data.discount_type === 'fixed_amount' ? 'FixedCart' : 'PercentCart',
  discountValue: Number(data.discount_value),
  minOrderAmount: Number(data.discount_min_value),
  maxDiscountAmount: data.discount_type === 'fixed_amount' ? null : Number(data.discount_max_value),
  maxUses: data.discount_max_uses ? Number(data.discount_max_uses) : null,
  perUserLimit: Number(data.discount_max_uses_per_user ?? 1),
  appliesTo: data.discount_applies_to === 'specific' ? 'SpecificSpus' : 'AllProducts',
  targetIds: data.discount_applies_to === 'specific' && Array.isArray(data.discount_product_ids) ? data.discount_product_ids : null,
  validFrom: data.discount_start_date ? new Date(`${data.discount_start_date}T00:00:00.000Z`).toISOString() : null,
  validTo: data.discount_end_date ? new Date(`${data.discount_end_date}T23:59:59.999Z`).toISOString() : null,
  isPublic: Boolean(data.discount_is_public)
})

export const toApiPayloadDiscountUpdate = (data) => ({
  name: data.discount_name.trim(),
  maxUses: data.discount_max_uses ? Number(data.discount_max_uses) : null,
  validTo: data.discount_end_date ? new Date(`${data.discount_end_date}T23:59:59.999Z`).toISOString() : null,
  isActive: Boolean(data.discount_is_active)
})

export const slugCateToNameCate = (categoriesSlug = [], categories = []) =>
  categoriesSlug.map((catId) => {
    const found = categories.find((c) => c.cat_slug === catId)
    return { id: catId, label: found?.cat_name ?? catId.slice(-6) }
  })

export const toArrayProducts = (data) => {
  if (Array.isArray(data)) return data
  if (data && Array.isArray(data.items)) return data.items
  if (data && Array.isArray(data.products)) return data.products
  return []
}


