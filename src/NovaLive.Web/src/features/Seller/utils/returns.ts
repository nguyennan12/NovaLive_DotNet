import type { ReturnReason } from '../types/contracts'

export const returnReasons: Record<ReturnReason, string> = { WrongItem: 'Giao sai sản phẩm', Defective: 'Sản phẩm bị lỗi', DamagedInShipping: 'Hư hỏng khi vận chuyển', NotAsDescribed: 'Không đúng mô tả', ChangeOfMind: 'Thay đổi ý định' }
