import { z } from 'zod'
import type { Category, CreateSkuRequest } from '../types/contracts'

export interface VariantAxis { name: string; values: string[] }
export function flattenCategories(categories: Category[], prefix = ''): { id: string; name: string }[] {
  return categories.flatMap(category => [{ id: category.id, name: `${prefix}${category.name}` }, ...flattenCategories(category.children || [], `${prefix}— `)])
}
export function buildSkuMatrix(axes: VariantAxis[], codePrefix: string): CreateSkuRequest[] {
  const normalized = axes.map(axis => ({ name: axis.name.trim(), values: [...new Set(axis.values.map(value => value.trim()).filter(Boolean))] }))
  if (normalized.some(axis => !axis.name || !axis.values.length)) throw new Error('Mỗi trục biến thể cần tên và ít nhất một giá trị.')
  if (new Set(normalized.map(axis => axis.name.toLowerCase())).size !== normalized.length) throw new Error('Tên các trục biến thể không được trùng nhau.')
  if (normalized.reduce((count, axis) => count * axis.values.length, 1) > 200) throw new Error('Tối đa 200 SKU mỗi lần tạo. Vui lòng giảm số biến thể.')
  const combinations = normalized.reduce<Record<string, string>[]>((rows, axis) => rows.flatMap(row => axis.values.map(value => ({ ...row, [axis.name]: value }))), [{}])
  return combinations.map((attributes, index) => ({ skuCode: `${codePrefix.trim() || 'NOVA'}-${String(index + 1).padStart(3, '0')}`, attributesJson: JSON.stringify(attributes), sellPrice: 0, originalPrice: 0, weightGram: 200, initialStock: 0, images: [] }))
}
export const skuSchema = z.object({
  skuCode: z.string().trim().min(1, 'Nhập mã SKU.').max(100, 'Mã SKU tối đa 100 ký tự.'), attributesJson: z.string(),
  originalPrice: z.number().min(0, 'Giá gốc không được âm.'), sellPrice: z.number().min(0, 'Giá bán không được âm.'),
  weightGram: z.number().int('Khối lượng phải là số nguyên.').positive('Khối lượng phải lớn hơn 0.'),
  initialStock: z.number().int('Tồn đầu phải là số nguyên.').min(0, 'Tồn đầu không được âm.'), images: z.array(z.string())
})
export const productSchema = z.object({
  name: z.string().trim().min(5, 'Tên sản phẩm cần ít nhất 5 ký tự.').max(300, 'Tên sản phẩm tối đa 300 ký tự.'), description: z.string().trim().min(1, 'Nhập mô tả sản phẩm.'), categoryId: z.string().min(1, 'Chọn danh mục.'), brand: z.string().nullable(), thumbnailUrl: z.string().min(1, 'Vui lòng tải ảnh đại diện sản phẩm.'), attributesConfigJson: z.string().nullable(), attributes: z.array(z.object({ name: z.string().trim().min(1, 'Nhập tên thuộc tính.'), value: z.string().trim().min(1, 'Nhập giá trị thuộc tính.') })),
  skus: z.array(skuSchema).min(1, 'Sản phẩm phải có ít nhất một SKU.').refine(skus => new Set(skus.map(sku => sku.skuCode.trim().toLowerCase())).size === skus.length, 'Mã SKU không được trùng nhau.')
})
