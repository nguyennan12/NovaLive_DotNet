

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
        searchParams.set(key, String(safeLimit))
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
  discount_start_date: toDateInputValue(discount.discount_start_date ?? discount.startDate),
  discount_end_date: toDateInputValue(discount.discount_end_date ?? discount.endDate),
  discount_is_active: typeof discount.discount_is_active === 'boolean'
    ? discount.discount_is_active
    : (discount.status ? discount.status === 'active' : true),
  discount_target: discount.discount_target ?? (discount.category === 'freeship' ? 'shipping' : 'product'),
  discount_type: discount.discount_type
    ?? (discount.type === 'fixed' || discount.type === 'amount' ? 'fixed_amount' : 'percentage'),
  discount_applies_to: discount.discount_applies_to ?? 'all',
  discount_scope: discount.discount_scope ?? 'global',
  discount_shopId: discount.discount_shopId ?? null,
  discount_min_value: discount.discount_min_value ?? discount.minOrder ?? '',
  discount_value: discount.discount_value ?? discount.value ?? '',
  discount_max_value: discount.discount_max_value ?? discount.value ?? '',
  discount_max_uses: discount.discount_max_uses ?? discount.usageLimit ?? '',
  discount_uses_count: discount.discount_uses_count ?? discount.usedCount ?? 0,
  discount_users_used: discount.discount_users_used ?? [],
  discount_max_uses_per_user: discount.discount_max_uses_per_user ?? 1,
  discount_product_ids: discount.discount_product_ids ?? []
})

export const toApiPayloadDiscount = (data) => ({
  discount_name: data.discount_name.trim(),
  discount_description: data.discount_description.trim(),
  discount_code: data.discount_code?.trim().toUpperCase() || null,
  discount_start_date: data.discount_start_date ? new Date(`${data.discount_start_date}T00:00:00.000Z`).toISOString() : null,
  discount_end_date: data.discount_end_date ? new Date(`${data.discount_end_date}T23:59:59.999Z`).toISOString() : null,
  discount_is_active: Boolean(data.discount_is_active),
  discount_target: data.discount_target,
  discount_type: data.discount_type,
  discount_applies_to: data.discount_applies_to ?? 'all',
  discount_scope: data.discount_scope ?? 'global',
  discount_shopId: data.discount_shopId || null,
  discount_min_value: Number(data.discount_min_value),
  discount_value: Number(data.discount_value),
  discount_max_value: Number(data.discount_max_value),
  discount_max_uses: Number(data.discount_max_uses),
  discount_uses_count: Number(data.discount_uses_count ?? 0),
  discount_users_used: Array.isArray(data.discount_users_used) ? data.discount_users_used : [],
  discount_max_uses_per_user: Number(data.discount_max_uses_per_user ?? 1),
  discount_product_ids: Array.isArray(data.discount_product_ids) ? data.discount_product_ids : []
})

export const slugCateToNameCate = (categoriesSlug = [], categories = []) =>
  categoriesSlug.map((catId) => {
    const found = categories.find((c) => c.cat_slug === catId)
    return { id: catId, label: found?.cat_name ?? catId.slice(-6) }
  })

export const toArrayProducts = (data) => {
  if (Array.isArray(data)) return data
  if (data && Array.isArray(data.products)) return data.products
  return []
}


