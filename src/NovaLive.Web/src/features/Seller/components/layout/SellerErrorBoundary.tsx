import { Component } from 'react'
import type { ReactNode } from 'react'
import { Alert, Button } from '@mui/material'

export class SellerErrorBoundary extends Component<{ children: ReactNode }, { failed: boolean }> {
  state = { failed: false }
  static getDerivedStateFromError() { return { failed: true } }
  render() {
    if (this.state.failed) return <Alert severity='error' action={<Button color='inherit' onClick={() => window.location.reload()}>Tải lại trang</Button>}>Không thể hiển thị dữ liệu trang này. Vui lòng tải lại hoặc thử lại sau.</Alert>
    return this.props.children
  }
}
