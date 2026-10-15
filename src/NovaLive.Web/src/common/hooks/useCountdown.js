/* eslint-disable react-hooks/exhaustive-deps */
import { useState, useEffect } from 'react'

const useCountdown = (endTime) => {
  const calcTimeLeft = () => {
    const diff = Math.max(0, new Date(endTime) - new Date())
    return {
      d: Math.floor(diff / 86400000),
      h: Math.floor((diff % 86400000) / 3600000),
      m: Math.floor((diff % 3600000) / 60000),
      s: Math.floor((diff % 60000) / 1000)
    }
  }

  const [time, setTime] = useState(calcTimeLeft)

  useEffect(() => {
    if (!endTime || new Date(endTime) - new Date() <= 0) return

    const timer = setInterval(() => setTime(calcTimeLeft()), 1000)
    return () => clearInterval(timer)
  }, [endTime])

  return time
}

export default useCountdown