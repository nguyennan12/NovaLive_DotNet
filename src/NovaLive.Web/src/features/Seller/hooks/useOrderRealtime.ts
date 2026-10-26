import { useEffect, useRef, useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { toast } from 'react-toastify'
import { createHubConnection } from '~/common/services/signalr/signalrService'
import { MOCK_ONLY, SELLER_QUERY_KEY } from '../constants'
import type { OrderPlacedEvent } from '../types/viewModels'
import { money } from '../utils/format'

export function useOrderRealtime() {
  const queryClient = useQueryClient()
  const [sound, setSound] = useState(() => localStorage.getItem('novalive:seller:order-sound') === 'true')
  const [state, setState] = useState('Chưa bật')
  const soundRef = useRef(sound)
  const audioRef = useRef<AudioContext | null>(null)
  const autoJoin = import.meta.env.VITE_SELLER_ORDER_HUB_AUTOJOIN === 'true'
  const play = () => {
    try {
      const audio = audioRef.current ?? new AudioContext()
      audioRef.current = audio
      void audio.resume()
      const oscillator = audio.createOscillator(); const gain = audio.createGain()
      oscillator.connect(gain); gain.connect(audio.destination); oscillator.frequency.value = 740
      gain.gain.setValueAtTime(0.08, audio.currentTime); gain.gain.exponentialRampToValueAtTime(0.001, audio.currentTime + 0.3)
      oscillator.start(); oscillator.stop(audio.currentTime + 0.3)
    } catch { /* A browser may require a user gesture before allowing audio. */ }
  }
  useEffect(() => { soundRef.current = sound; localStorage.setItem('novalive:seller:order-sound', String(sound)) }, [sound])
  useEffect(() => {
    if (MOCK_ONLY || !autoJoin) return
    // Backend must join the JWT-owned shop automatically. Never invoke JoinShop(shopId).
    const connection = createHubConnection('/hubs/order')
    let disposed = false
    let timer: ReturnType<typeof setTimeout> | undefined
    connection.on('OrderPlaced', (event: OrderPlacedEvent) => {
      toast.info(`Đơn hàng mới • ${event.itemCount} món • ${money(event.totalAmount)}`)
      if (soundRef.current) play()
      void queryClient.invalidateQueries({ queryKey: SELLER_QUERY_KEY })
    })
    connection.onreconnecting(() => { if (!disposed) setState('Đang kết nối lại') })
    connection.onreconnected(() => { if (!disposed) { setState('Đã kết nối'); void queryClient.invalidateQueries({ queryKey: SELLER_QUERY_KEY }) } })
    const start = async () => {
      try { await connection.start(); if (!disposed) setState('Đã kết nối') }
      catch { if (!disposed) { setState('Chưa kết nối'); timer = setTimeout(() => void start(), 15_000) } }
    }
    connection.onclose(() => { if (!disposed) { setState('Chưa kết nối'); timer = setTimeout(() => void start(), 15_000) } })
    void start()
    return () => { disposed = true; clearTimeout(timer); connection.off('OrderPlaced'); void connection.stop() }
  }, [queryClient, autoJoin])
  useEffect(() => () => { void audioRef.current?.close() }, [])
  return { sound, toggleSound: () => { if (!sound) play(); setSound(!sound) }, state, enabled: autoJoin && !MOCK_ONLY }
}
