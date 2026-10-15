import ExpandMoreRoundedIcon from '@mui/icons-material/ExpandMoreRounded'
import SubjectIcon from '@mui/icons-material/Subject'
import { Box, Button, Collapse, Typography } from '@mui/material'
import { useState } from 'react'
import { glassSx } from '~/theme'
import { SectionHeader } from '../shared/SectionHeader'

export const ProductDescription = ({ text }) => {
  const [expanded, setExpanded] = useState(false)
  if (!text) return null

  const paragraphs = text
    .split(/\n\n+/)
    .flatMap(chunk => chunk.split(/(?<=[.!?])\s{2,}/))
    .map(p => p.trim())
    .filter(Boolean)

  const PREVIEW_COUNT = 3

  return (
    <Box sx={{
      p: 2,
      bgcolor: 'primary.main',
      borderRadius: '12px',
      display: 'flex',
      flexDirection: 'column',
      ...glassSx
    }}>

      <SectionHeader title='Mô tả sản phẩm' icon={<SubjectIcon />} />

      <Box sx={{ position: 'relative', flexGrow: 1 }}>
        <Collapse in={expanded} collapsedSize={paragraphs.length > PREVIEW_COUNT ? 240 : 'auto'}>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
            {paragraphs.map((para, i) => {
              return <Typography
                key={i}
                sx={{
                  fontSize: '0.875rem',
                  color: '#444',
                  lineHeight: 1.85,
                  textAlign: 'justify',
                  letterSpacing: '0.01em'
                }}
              >
                {para}
              </Typography>
            })}
          </Box>
        </Collapse>
        {!expanded && paragraphs.length > PREVIEW_COUNT && (
          <Box sx={{
            position: 'absolute', bottom: 0, left: 0, right: 0, height: 64,
            background: 'linear-gradient(to bottom, rgba(248,250,255,0) 0%, rgba(248,250,255,0.85) 100%)',
            pointerEvents: 'none'
          }} />
        )}
      </Box>

      {paragraphs.length > PREVIEW_COUNT && (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 'auto', pt: 1 }}>
          <Button
            size="small"
            endIcon={
              <ExpandMoreRoundedIcon sx={{
                fontSize: 18,
                transition: 'transform 0.25s',
                transform: expanded ? 'rotate(180deg)' : 'rotate(0deg)'
              }} />
            }
            onClick={() => setExpanded(prev => !prev)}
            sx={{
              textTransform: 'none', fontWeight: 600, fontSize: '0.82rem',
              color: 'secondary.main', borderRadius: '8px',
              '&:hover': { bgcolor: 'rgba(52,133,247,0.06)' }
            }}
          >
            {expanded ? 'Thu gọn' : 'Xem thêm'}
          </Button>
        </Box>
      )}
    </Box>
  )
}