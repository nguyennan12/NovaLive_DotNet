import { useEffect, useRef, useState } from 'react'
import AgoraRTC from 'agora-rtc-sdk-ng'
import { joinLiveAPI } from '~/common/apis/services/liveService'
import { env } from '~/common/configs/enviroment'

AgoraRTC.setLogLevel(4)

export const useLivePlayer = ({ liveId, userId }) => {
  const videoRef = useRef(null)
  const clientRef = useRef(null)
  const [status, setStatus] = useState('connecting')
  const [audioBlocked, setAudioBlocked] = useState(false)

  useEffect(() => {
    if (!liveId) return

    let isMounted = true
    const client = AgoraRTC.createClient({ mode: 'live', codec: 'vp8' })
    clientRef.current = client

    const joinLive = async () => {
      try {
        const guestIdentifier = `viewer_${userId || 'guest'}_${Math.random().toString(36).substring(2, 7)}`
        const res = await joinLiveAPI(liveId, { guestId: guestIdentifier })
        if (!isMounted) return

        const { token, channelName, account } = res
        const joinAccount = account || guestIdentifier

        await client.setClientRole('audience')

        client.on('user-published', async (remoteUser, mediaType) => {
          try {
            await client.subscribe(remoteUser, mediaType)
            if (mediaType === 'video' && videoRef.current) {
              remoteUser.videoTrack.play(videoRef.current, { fit: 'cover' })
              if (isMounted) setStatus('live')
            }
            if (mediaType === 'audio') {
              try {
                remoteUser.audioTrack.play()
                if (isMounted) setAudioBlocked(false)
              } catch {
                if (isMounted) setAudioBlocked(true)
              }
            }
          } catch (subErr) {
            console.error('[LivePlayer] Subscribe error:', subErr)
          }
        })

        client.on('user-unpublished', (_, mediaType) => {
          if (mediaType === 'video' && isMounted) setStatus('ended')
        })

        const agoraAppId = env.AGORA_APP_ID || import.meta.env.VITE_AGORA_APP_ID
        await client.join(agoraAppId, channelName, token, joinAccount)

      } catch (err) {
        console.error('[LivePlayer] joinLive error:', err)
        if (isMounted) setStatus('error')
      }
    }

    joinLive()

    AgoraRTC.onAutoplayFailed = () => {
      if (isMounted) setAudioBlocked(true)
    }

    return () => {
      isMounted = false
      AgoraRTC.onAutoplayFailed = undefined
      if (clientRef.current) {
        clientRef.current.leave().catch(() => {})
        clientRef.current = null
      }
    }
  }, [liveId, userId])

  const resumeAudio = async () => {
    const users = clientRef.current?.remoteUsers ?? []
    users.forEach(user => user.audioTrack?.play())
    setAudioBlocked(false)
  }

  return { videoRef, status, audioBlocked, resumeAudio }
}
