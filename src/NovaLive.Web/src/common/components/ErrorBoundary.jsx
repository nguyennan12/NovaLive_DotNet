import React from 'react'
import { Box, Typography, Button, Paper } from '@mui/material'
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded'
import HomeRoundedIcon from '@mui/icons-material/HomeRounded'
import ErrorOutlineRoundedIcon from '@mui/icons-material/ErrorOutlineRounded'

export class ErrorBoundary extends React.Component {
  constructor(props) {
    super(props)
    this.state = { hasError: false, error: null, errorInfo: null }
  }

  static getDerivedStateFromError(error) {
    return { hasError: true, error }
  }

  componentDidCatch(error, errorInfo) {
    console.error('ErrorBoundary caught an unhandled runtime error:', error, errorInfo)
    this.setState({ error, errorInfo })
  }

  handleReload = () => {
    window.location.reload()
  }

  handleGoHome = () => {
    window.location.href = '/'
  }

  render() {
    if (this.state.hasError) {
      return (
        <Box
          sx={{
            minHeight: '100vh',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            bgcolor: '#f8fafc',
            p: 3
          }}
        >
          <Paper
            elevation={0}
            sx={{
              maxWidth: 540,
              width: '100%',
              p: 4,
              borderRadius: '20px',
              border: '1px solid #e2e8f0',
              textAlign: 'center',
              boxShadow: '0 10px 30px rgba(0,0,0,0.04)'
            }}
          >
            <Box
              sx={{
                width: 64,
                height: 64,
                borderRadius: '16px',
                bgcolor: '#fef2f2',
                color: '#ef4444',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                mx: 'auto',
                mb: 2.5
              }}
            >
              <ErrorOutlineRoundedIcon sx={{ fontSize: 36 }} />
            </Box>

            <Typography variant="h5" sx={{ fontWeight: 800, color: '#0f172a', mb: 1 }}>
              Đã xảy ra sự cố hiển thị
            </Typography>

            <Typography variant="body2" sx={{ color: '#64748b', mb: 3 }}>
              Hệ thống ghi nhận sự cố bất ngờ trong quá trình kết xuất giao diện. Bạn có thể thử tải lại trang hoặc quay về trang chủ.
            </Typography>

            {this.state.error && (
              <Box
                sx={{
                  bgcolor: '#f1f5f9',
                  p: 2,
                  borderRadius: '10px',
                  textAlign: 'left',
                  mb: 3,
                  maxHeight: 140,
                  overflowY: 'auto'
                }}
              >
                <Typography variant="caption" sx={{ fontFamily: 'monospace', color: '#dc2626', wordBreak: 'break-word', fontWeight: 600 }}>
                  {this.state.error.toString()}
                </Typography>
              </Box>
            )}

            <Box sx={{ display: 'flex', gap: 1.5, justifyContent: 'center' }}>
              <Button
                variant="contained"
                startIcon={<RefreshRoundedIcon />}
                onClick={this.handleReload}
                sx={{
                  textTransform: 'none',
                  fontWeight: 700,
                  borderRadius: '10px',
                  background: 'linear-gradient(90deg, #0095ff 0%, #14c3eb 100%)',
                  boxShadow: '0 4px 14px rgba(0, 149, 255, 0.25)'
                }}
              >
                Tải lại trang
              </Button>
              <Button
                variant="outlined"
                startIcon={<HomeRoundedIcon />}
                onClick={this.handleGoHome}
                sx={{
                  textTransform: 'none',
                  fontWeight: 700,
                  borderRadius: '10px',
                  borderColor: '#cbd5e1',
                  color: '#475569',
                  '&:hover': {
                    borderColor: '#94a3b8',
                    bgcolor: '#f1f5f9'
                  }
                }}
              >
                Về trang chủ
              </Button>
            </Box>
          </Paper>
        </Box>
      )
    }

    return this.props.children
  }
}

export default ErrorBoundary
