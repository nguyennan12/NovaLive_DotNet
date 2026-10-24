import axios from 'axios'
import { ZodError } from 'zod'

function messages(value: unknown): string[] {
  if (typeof value === 'string') return [value]
  if (Array.isArray(value)) return value.flatMap(messages)
  if (value && typeof value === 'object') return Object.values(value).flatMap(messages)
  return []
}
export function errorMessage(error: unknown): string {
  if (error instanceof ZodError) return error.issues.map(issue => issue.message).join('\n')
  if (axios.isAxiosError(error)) {
    const body: unknown = error.response?.data
    if (body && typeof body === 'object') {
      const problem = body as Record<string, unknown>
      const apiError = problem.error && typeof problem.error === 'object' ? problem.error as Record<string, unknown> : {}
      const validation = messages(problem.errors ?? apiError.validationErrors)
      if (validation.length) return validation.join('\n')
      const detail = problem.detail ?? apiError.message ?? problem.message ?? problem.title
      if (typeof detail === 'string') return detail
    }
    if (!error.response) return 'Không kết nối được máy chủ. Vui lòng kiểm tra kết nối và thử lại.'
    if (error.response.status === 403) return 'Bạn không có quyền thực hiện thao tác này.'
    if (error.response.status === 404 || error.response.status === 501) return 'Chức năng chưa được backend cung cấp. Vui lòng thử lại sau.'
  }
  return error instanceof Error ? error.message : 'Đã có lỗi xảy ra. Vui lòng thử lại.'
}
