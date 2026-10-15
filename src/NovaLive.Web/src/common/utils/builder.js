/* eslint-disable indent */
import { formatStringToSlug } from './formatters'

export const buildQueryParamsProducts = (filters) => {
    const params = {}
    const categories = Array.isArray(filters.category)
        ? filters.category
        : filters.category
            ? [filters.category]
            : []

    const validCategories = categories.filter(c => c && c !== 'all')
    if (validCategories.length > 0) {
        params.category = validCategories.map(formatStringToSlug)
    }

    if (filters.keyword && filters.keyword.trim()) { params.keyword = filters.keyword.trim() }
    if (filters.status && filters.status !== 'all') { params.status = filters.status }
    if (filters.stock && filters.stock !== 'all') { params.stock = filters.stock }
    if (filters.shopId !== undefined) { params.shopId = filters.shopId }
    if (filters.minPrice !== undefined && filters.minPrice > 0) { params.minPrice = filters.minPrice }
    if (filters.maxPrice !== undefined) { params.maxPrice = filters.maxPrice }

    switch (filters.sort) {
        case 'price_asc':
            params.sortBy = 'price'
            params.sortOrder = 'asc'
            break
        case 'price_desc':
            params.sortBy = 'price'
            params.sortOrder = 'desc'
            break
        case 'name_az':
            params.sortBy = 'name'
            params.sortOrder = 'desc'
            break
        case 'newest':
        default:
            params.sortBy = 'time'
            params.sortOrder = 'desc'
            break
    }

    return params
}

export const buildFlattenedSkus = (skus) => {
    const list = []
    skus.forEach(spu => {
        if (spu.has_variations && spu.variation_stocks?.length > 0) {
            spu.variation_stocks.forEach(sku => {
                list.push({
                    sku_id: sku.sku_id,
                    spu_id: spu._id,
                    spu_name: spu.spu_name,
                    spu_code: spu.spu_code,
                    sku_name: sku.sku_name || Object.values(sku.attributes || {}).join(' - ') || 'Variation',
                    sku_code: sku.sku_code,
                    stock: sku.stock,
                    attributes: sku.attributes
                })
            })
        } else {
            list.push({
                spu_id: spu._id,
                spu_name: spu.spu_name,
                spu_code: spu.spu_code,
                sku_name: 'No variation',
                sku_code: 'N/A',
                stock: spu.total_stock,
                attributes: []
            })
        }
    })
    return list
}

export const buildQueryParamsDiscounts = (filters) => {
    const params = { limit: filters.limit, page: filters.page || 1 }

    if (filters.shopId !== undefined) params.shopId = filters.shopId
    if (filters.search && filters.search.trim()) params.search = filters.search.trim()
    if (filters.status && filters.status !== 'all') params.status = filters.status
    if (filters.scope && filters.scope !== 'all') params.scope = filters.scope
    if (filters.type && filters.type !== 'all') params.type = filters.type === 'fixed' ? 'fixed_amount' : filters.type
    if (filters.category && filters.category !== 'all') params.target = filters.category === 'freeship' ? 'shipping' : filters.category

    return params
}

export const buildShopOrderIds = (selectedItems, shopDiscountMap = {}) => {
    const shopMap = {}

    selectedItems.forEach(item => {
        const { shopId, productId, skuId, quantity } = item
        if (!shopMap[shopId]) {
            shopMap[shopId] = []
        }
        shopMap[shopId].push({ productId, skuId, quantity })
    })
    return Object.entries(shopMap).map(([shopId, item_products]) => {
        const discount = shopDiscountMap[shopId] || []
        const result = { shopId, item_products }
        if (discount && discount.code) {
            result.shop_discount = [
                { id: discount.id, code: discount.code, target: discount.category }
            ]
        }
        return result
    })
}