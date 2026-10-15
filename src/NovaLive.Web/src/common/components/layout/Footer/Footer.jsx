import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { Box, Divider, Typography } from '@mui/material'
import { useNavigate } from 'react-router-dom'
import { gradientText } from '~/theme'
import LinkedInIcon from '@mui/icons-material/LinkedIn'
import FacebookIcon from '@mui/icons-material/Facebook'
import GitHubIcon from '@mui/icons-material/GitHub'

const SOCIAL = [
  { label: 'fb', icon: <FacebookIcon />, href: 'https://www.facebook.com/share/1TJ3YeiaA6/?mibextid=wwXIfr' },
  { label: 'git', icon: <GitHubIcon />, href: 'https://github.com/nguyennan12' },
  { label: 'in', icon: <LinkedInIcon />, href: 'https://www.linkedin.com/in/d%C6%B0-nguy%C3%AAn-an-4a0965358/' }
]

const LINKS = [
  {
    title: 'KHÁM PHÁ',
    items: ['Trang chủ', 'Flash Sale', 'Best Seller', 'Live Stream']
  },
  {
    title: 'TÌM HIỂU THÊM',
    items: ['Về chúng tôi', 'Blog', 'Tin tức', 'Hợp tác kinh doanh']
  },
  {
    title: 'HỖ TRỢ',
    items: ['Điều khoản sử dụng', 'Chính sách bảo mật', 'Trung tâm hỗ trợ', 'FAQ']
  }
]

const FooterLink = ({ children }) => (
  <Typography
    component="span"
    sx={{
      display: 'block',
      fontSize: '0.82rem',
      color: '#757575ff',
      cursor: 'pointer',
      py: 0.35,
      transition: 'color 0.15s',
      '&:hover': { color: 'secondary.main' }
    }}
  >
    {children}
  </Typography>
)

const Footer = () => {
  const navigate = useNavigate()

  return (
    <Box
      component="footer"
      sx={{
        background: 'rgba(255, 255, 255, 0.08)',
        backdropFilter: 'blur(12px)',
        WebkitBackdropFilter: 'blur(12px)',
        borderTop: '1px solid rgba(255,255,255,0.18)',
        boxShadow: '0 -4px 30px rgba(0,0,0,0.04)',
        zIndex: 1,
        mt: { xs: 4, md: 6 },
        px: { xs: 2, sm: 4, md: 6, lg: 8 },
        pt: { xs: 4, md: 5 },
        pb: { xs: 3, md: 4 }
      }}
    >
      {/* Main grid */}
      <Box sx={{
        display: 'grid',
        gridTemplateColumns: {
          xs: '1fr 1fr',
          sm: '1fr 1fr 1fr 1fr'
        },
        gap: { xs: 3, sm: 4, md: 5 }
      }}>
        {/* Brand column */}
        <Box sx={{ gridColumn: { xs: '1 / -1', sm: 'auto' } }}>
          <Box
            sx={{ display: 'flex', alignItems: 'center', gap: 1.25, mb: 1.5, cursor: 'pointer' }}
            onClick={() => navigate('/', { replace: true })}
          >
            <Box sx={{ flexShrink: 0, userSelect: 'none', display: 'flex', alignItems: 'center', gap: 0.75 }}>
              <Box sx={{
                width: 28, height: 28, borderRadius: '8px',
                background: 'linear-gradient(90deg, #69bef7ff, #53e6eeff)',
                display: 'flex', alignItems: 'center', justifyContent: 'center',
                boxShadow: '0 2px 8px rgba(52,133,247,0.3)'
              }}>
                <PlayArrowIcon sx={{ color: '#fff', fontSize: '1.25rem' }} />
              </Box>
              <Typography variant="h6" sx={{
                fontWeight: 800,
                fontSize: { xs: '1.1rem', sm: '1.1rem' },
                ...gradientText,
                letterSpacing: '-0.4px',
                lineHeight: 1
              }}>
                NovaLive
              </Typography>
            </Box>
          </Box>

          <Typography sx={{
            fontSize: '0.78rem',
            color: 'text.secondary',
            lineHeight: 1.7,
            maxWidth: { xs: '100%', sm: 220 },
            mb: 2.5
          }}>
            Nền tảng mua sắm kết hợp livestream — trải nghiệm mua hàng trực tiếp, nhanh chóng và tin cậy.
          </Typography>

          {/* Social icons */}
          <Box sx={{ display: 'flex', gap: 1 }}>
            {SOCIAL.map(({ label, href, icon }) => (
              <Box
                key={label}
                component="a"
                href={href}
                target="_blank"
                rel="noopener noreferrer"
                sx={{
                  width: 34, height: 34, borderRadius: '50%',
                  background: 'linear-gradient(135deg, #3485f7 0%, #66b2ff 100%)',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  color: '#fff',
                  fontSize: label === 'in' ? '0.65rem' : '0.82rem',
                  fontWeight: 800,
                  textDecoration: 'none',
                  boxShadow: '0 2px 8px rgba(52,133,247,0.3)',
                  transition: 'transform 0.18s ease, box-shadow 0.18s ease',
                  '&:hover': {
                    transform: 'translateY(-2px)',
                    boxShadow: '0 4px 14px rgba(52,133,247,0.45)'
                  }
                }}
              >
                {icon}
              </Box>
            ))}
          </Box>
        </Box>

        {LINKS.map(({ title, items }, index) => (
          <Box
            key={title}
            sx={{ gridColumn: { xs: index === 2 ? '1 / -1' : 'auto', sm: 'auto' } }}
          >
            <Typography sx={{
              fontSize: '0.72rem',
              fontWeight: 800,
              letterSpacing: '0.08em',
              color: 'secondary.main',
              mb: 1.5,
              opacity: 0.85
            }}>
              {title}
            </Typography>
            {items.map(item => (
              <FooterLink key={item}>{item}</FooterLink>
            ))}
          </Box>
        ))}
      </Box>

      {/* Bottom bar */}
      <Divider sx={{ my: { xs: 2.5, md: 3 }, borderColor: 'rgba(0,0,0,0.07)' }} />

      <Box sx={{
        display: 'flex',
        flexDirection: { xs: 'column-reverse', sm: 'row' },
        justifyContent: { xs: 'center', sm: 'space-between' },
        alignItems: 'center',
        gap: { xs: 1.5, sm: 1 }
      }}>
        <Typography sx={{ fontSize: '0.72rem', color: 'text.secondary', textAlign: 'center' }}>
          © {new Date().getFullYear()} NovaLive. All rights reserved.
        </Typography>
        <Box sx={{ display: 'flex', gap: 2.5 }}>
          {['Điều khoản', 'Bảo mật'].map(label => (
            <Typography
              key={label}
              sx={{
                fontSize: '0.72rem', color: 'text.secondary', cursor: 'pointer',
                '&:hover': { color: 'secondary.main' }
              }}
            >
              {label}
            </Typography>
          ))}
        </Box>
      </Box>
    </Box>
  )
}

export default Footer