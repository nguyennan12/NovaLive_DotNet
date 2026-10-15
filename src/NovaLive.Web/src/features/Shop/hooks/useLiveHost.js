/* eslint-disable no-console */
import { useCallback, useEffect, useRef, useState } from 'react'
import AgoraRTC from 'agora-rtc-sdk-ng'
import { startLiveAPI, endLiveAPI } from '~/common/apis/services/liveService'
import { formatDuration } from '~/common/utils/formatters'

const AGORA_APP_ID = import.meta.env.VITE_AGORA_APP_ID

export const useLiveHost = ({ liveId, userId }) => {
  const [status, setStatus] = useState('idle') // idle | starting | live | ending | ended | error
  const [isMicOn, setIsMicOn] = useState(true)
  const [isCamOn, setIsCamOn] = useState(true)
  const [duration, setDuration] = useState(0)

  const clientRef = useRef(null)
  const localVideoTrackRef = useRef(null)
  const localAudioTrackRef = useRef(null)
  const videoContainerRef = useRef(null)
  const timerRef = useRef(null)

  // turn off khi out live
  const stopAllTracks = useCallback(() => {
    if (localVideoTrackRef.current) {
      localVideoTrackRef.current.stop()
      localVideoTrackRef.current.close()
      localVideoTrackRef.current = null
    }
    if (localAudioTrackRef.current) {
      localAudioTrackRef.current.stop()
      localAudioTrackRef.current.close()
      localAudioTrackRef.current = null
    }
    if (clientRef.current) {
      clientRef.current.leave()
    }
  }, [])

  useEffect(() => {
    return () => {
      stopAllTracks()
      if (timerRef.current) clearInterval(timerRef.current)
    }
  }, [stopAllTracks])

  const startLive = useCallback(async () => {
    if (!liveId || !userId) return
    setStatus('starting')

    try {
      // Lấy Token từ Server
      const { token } = await startLiveAPI(liveId)
      const channelName = liveId.toString()

      // Khởi tạo Agora Client
      const client = AgoraRTC.createClient({ mode: 'live', codec: 'vp8' })
      await client.setClientRole('host')
      clientRef.current = client

      // Join vào Channel
      await client.join(AGORA_APP_ID, channelName, token, userId)

      //  Khởi tạo Track (video và audio)
      let audioTrack = null
      let videoTrack = null

      // Thử lấy Micro
      try {
        audioTrack = await AgoraRTC.createMicrophoneAudioTrack()
        setIsMicOn(true)
      } catch (e) {
        console.warn('Not found device Microphone:', e)
        setIsMicOn(false)
      }

      // Thử lấy Camera
      try {
        videoTrack = await AgoraRTC.createCameraVideoTrack()
        setIsCamOn(true)
      } catch (e) {
        console.warn('Not found device Camera:', e)
        setIsCamOn(false)
      }

      if (!audioTrack && !videoTrack) {
        throw new Error('DEVICE_NOT_FOUND: do not have any audio/video device')
      }

      localAudioTrackRef.current = audioTrack
      localVideoTrackRef.current = videoTrack

      //hiển thị video sau khi connect thàng công
      if (videoTrack) {
        if (videoContainerRef.current) {
          videoTrack.play(videoContainerRef.current)
        } else {
          console.warn('dont have video container ref to play local video track')
        }
      }

      //đẩy cái track lên agora để user thấy
      const publishTracks = []
      if (audioTrack) publishTracks.push(audioTrack)
      if (videoTrack) publishTracks.push(videoTrack)

      await client.publish(publishTracks)

      setStatus('live')

      //đếm thời gian
      if (timerRef.current) clearInterval(timerRef.current)
      timerRef.current = setInterval(() => {
        setDuration(prev => prev + 1)
      }, 1000)

    } catch (err) {
      console.error('startLive error:', err)
      setStatus('error')
      stopAllTracks()
    }
  }, [liveId, userId, stopAllTracks])

  const endLive = useCallback(async () => {
    setStatus('ending')
    try {
      if (timerRef.current) clearInterval(timerRef.current)
      stopAllTracks()
      await endLiveAPI(liveId)
      setStatus('ended')
    } catch (error) {
      console.error('endLive error:', error)
      setStatus('error')
    }
  }, [liveId, stopAllTracks])

  const toggleMic = useCallback(async () => {
    if (!localAudioTrackRef.current) return
    const newState = !isMicOn
    try {
      await localAudioTrackRef.current.setEnabled(newState)
      setIsMicOn(newState)
    } catch (e) {
      console.error('error changing mic state:', e)
    }
  }, [isMicOn])

  const toggleCam = useCallback(async () => {
    if (!localVideoTrackRef.current) return
    const newState = !isCamOn
    try {
      await localVideoTrackRef.current.setEnabled(newState)
      setIsCamOn(newState)
    } catch (e) {
      console.error('error changing camera state:', e)
    }
  }, [isCamOn])

  return {
    status,
    isMicOn,
    isCamOn,
    videoContainerRef,
    startLive,
    endLive,
    toggleMic,
    toggleCam,
    duration: formatDuration(duration)
  }
}