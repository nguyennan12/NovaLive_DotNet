import { useState, useEffect } from 'react'

export const useOtpTimer = (initialCooldown, isOpen) => {
  const [resendCooldown, setResendCooldown] = useState(initialCooldown)
  const [prevIsOpen, setPrevIsOpen] = useState(isOpen)

  // Reset cooldown khi dialog đóng — set state during render tránh cascade
  if (prevIsOpen !== isOpen) {
    setPrevIsOpen(isOpen)
    if (!isOpen) setResendCooldown(initialCooldown)
  }

  // Chỉ giữ countdown timer (side effect thật sự) trong useEffect
  useEffect(() => {
    if (isOpen && resendCooldown > 0) {
      const timer = setTimeout(() => setResendCooldown(prev => prev - 1), 1000)
      return () => clearTimeout(timer)
    }
  }, [resendCooldown, isOpen])

  return { resendCooldown, setResendCooldown, canResend: resendCooldown <= 0 }
}