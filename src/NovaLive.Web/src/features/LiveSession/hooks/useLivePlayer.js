import { useEffect, useRef, useState } from 'react'
import AgoraRTC from 'agora-rtc-sdk-ng'
import { joinLiveAPI } from '~/common/apis/services/liveService'
import { env } from '~/common/configs/enviroment'

AgoraRTC.setLogLevel(4)

export const useLivePlayer = ({ liveId, userId }) => {
  const videoRef = useRef(null)
  const clientRef = useRef(null)
  const [status, setStatus] = useState('connecting')

  useEffect(() => {
    if (!userId || !liveId) return

    let isMounted = true

    const joinLive = async () => {
      try {
        const { token, channelName } = await joinLiveAPI(liveId)
        if (!isMounted) return

        const client = AgoraRTC.createClient({ mode: 'live', codec: 'vp8' })
        await client.setClientRole('audience')
        clientRef.current = client

        client.on('user-published', async (remoteUser, mediaType) => {
          await client.subscribe(remoteUser, mediaType)
          if (mediaType === 'video') {
            remoteUser.videoTrack.play(videoRef.current)
            if (isMounted) setStatus('live')
          }
          if (mediaType === 'audio') remoteUser.audioTrack.play()
        })
        client.on('user-unpublished', (_, mediaType) => {
          if (mediaType === 'video' && isMounted) setStatus('ended')
        })
        await client.join(env.AGORA_APP_ID, channelName, token, userId)

      } catch {
        if (isMounted) setStatus('error')
      }
    }

    joinLive()

    return () => {
      isMounted = false
      clientRef.current?.leave()
      clientRef.current = null
    }
  }, [liveId, userId])

  return { videoRef, status }
}