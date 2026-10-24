import type { AxiosRequestConfig } from 'axios'
import axios from 'axios'
import client from '~/common/apis/custom/authorizeAxios'
import { MOCK_FALLBACK, MOCK_ONLY, MOCK_SCENARIO } from '../constants'
import { isMissingSellerEndpoint } from '../utils/business'

const fallbackModules = new Set<string>()
const listeners = new Set<() => void>()
let moduleSnapshot = ''
export const subscribeMock = (listener: () => void) => { listeners.add(listener); return () => { listeners.delete(listener) } }
export const getMockSnapshot = () => moduleSnapshot
function markMock(module: string) {
  if (fallbackModules.has(module)) return
  fallbackModules.add(module)
  moduleSnapshot = [...fallbackModules].join(', ')
  listeners.forEach(listener => listener())
}
async function runMock<T>(module: string, mock: () => T | Promise<T>): Promise<T> {
  markMock(module)
  await new Promise(resolve => setTimeout(resolve, 180))
  if (MOCK_SCENARIO === 'error') throw new Error('Tình huống thử nghiệm: máy chủ tạm thời không phản hồi. Chuyển VITE_SELLER_MOCK_SCENARIO=success để thử lại.')
  return mock()
}
// Existing interceptor already unwraps ApiResponse<T>. Do not unwrap a second time.
export async function request<T>(module: string, config: AxiosRequestConfig, mock: () => T | Promise<T>): Promise<T> {
  if (MOCK_ONLY) return runMock(module, mock)
  try {
    const result: unknown = await client.request({ timeout: 20_000, ...config })
    return result as T
  } catch (error: unknown) {
    if (MOCK_FALLBACK && axios.isAxiosError(error) && isMissingSellerEndpoint(error.response?.status)) return runMock(module, mock)
    throw error
  }
}
