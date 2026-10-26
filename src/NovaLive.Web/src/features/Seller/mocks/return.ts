import { db, paginate, required, saveMock } from './database'
import { returnDeadline } from '../utils/format'
import type { OrderReturn, PagedResult, PageRequest } from '../types/contracts'
import type { ReturnDecision } from '../services/returnService'

export function mockGetReturns(params: PageRequest & { status?: string }): PagedResult<OrderReturn> | Promise<PagedResult<OrderReturn>> { return paginate(db().returns.filter(item => !params.status || item.status === params.status), params) }

export function mockDecideReturn(id: string, decision: ReturnDecision): unknown | Promise<unknown> {
  const item = required(db().returns.find(item => item.id === id), 'Yêu cầu hoàn hàng')
  if (decision.action === 'received') {
    if (!['SellerApproved', 'AdminApproved'].includes(item.status)) throw new Error('Chưa thể xác nhận nhận hàng ở trạng thái này.')
    if (db().wallet.balance < item.requestedRefundAmount) throw new Error('Số dư ví khả dụng không đủ để hoàn tiền.')
    item.status = 'Completed'; item.approvedRefundAmount = item.requestedRefundAmount
    const before = db().wallet.balance
    db().wallet.balance -= item.requestedRefundAmount
    db().transactions.unshift({ id: crypto.randomUUID(), walletId: 'mock-wallet', type: 'OrderRefund', amount: -item.requestedRefundAmount, balanceAfter: before - item.requestedRefundAmount, description: `Hoàn tiền đơn ${item.subOrderCode}`, createdAt: new Date().toISOString() })
  } else {
    if (item.status !== 'Pending' || returnDeadline(item.createdAt) <= Date.now()) throw new Error('Yêu cầu đã quá hạn phản hồi hoặc đang được xử lý.')
    if (decision.action === 'approve') { item.returnAddress = required(db().warehouses.find(item => item.isReturn), 'Kho nhận hàng hoàn'); item.status = 'SellerApproved'; item.sellerResponse = decision.note }
    else { if (!decision.reason.trim() || !decision.evidenceUrls.length) throw new Error('Vui lòng nhập lý do và tải bằng chứng đối chứng.'); item.status = 'SellerRejected'; item.sellerResponse = decision.reason; item.sellerEvidenceUrls = decision.evidenceUrls }
  }
  saveMock(); return structuredClone(item)
}
