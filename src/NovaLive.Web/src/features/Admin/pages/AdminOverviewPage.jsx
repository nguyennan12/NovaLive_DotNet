import { useState, useEffect } from 'react'
import {
  Alert,
  Box,
  Button,
  Grid,
  Stack,
  Typography,
  Chip
} from '@mui/material'
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import AccountBalanceWalletRoundedIcon from '@mui/icons-material/AccountBalanceWalletRounded'
import { Link } from 'react-router-dom'
import {
  getAdminDashboardReportAPI,
  getAdminShopsAPI,
  getAdminPayoutsAPI
} from '~/common/apis/services/adminService'
import { SellerCard } from '~/features/Seller/components/shared/SellerCard'
import { PageHeader } from '~/features/Seller/components/shared/PageHeader'
import { QueryState } from '~/features/Seller/components/shared/QueryState'
import { MoneyText } from '~/features/Seller/components/shared/MoneyText'
import { DataTable } from '~/features/Seller/components/shared/DataTable'

export const AdminOverviewPage = () => {
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [report, setReport] = useState(null)
  const [pendingShopsCount, setPendingShopsCount] = useState(0)
  const [pendingPayoutsCount, setPendingPayoutsCount] = useState(0)

  const loadData = async () => {
    setLoading(true)
    setError(null)
    try {
      const [reportRes, shopsRes, payoutsRes] = await Promise.allSettled([
        getAdminDashboardReportAPI(),
        getAdminShopsAPI({ status: 'Pending', size: 1 }),
        getAdminPayoutsAPI({ status: 'Pending', size: 1 })
      ])

      if (reportRes.status === 'fulfilled' && reportRes.value) {
        setReport(reportRes.value)
      } else if (reportRes.status === 'rejected') {
        setError(reportRes.reason)
      }

      if (shopsRes.status === 'fulfilled' && shopsRes.value) {
        setPendingShopsCount(shopsRes.value.totalCount ?? shopsRes.value.total ?? shopsRes.value.items?.length ?? 0)
      }

      if (payoutsRes.status === 'fulfilled' && payoutsRes.value) {
        setPendingPayoutsCount(payoutsRes.value.totalCount ?? payoutsRes.value.total ?? payoutsRes.value.items?.length ?? 0)
      }
    } catch (err) {
      setError(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadData()
  }, [])

  const pendingPayoutsList = report?.pendingPayouts || []

  return (
    <>
      <PageHeader
        title="Tổng quan sàn giao dịch"
        description="Số liệu tài chính, hiệu suất sàn và các tác vụ cần phê duyệt."
        action={
          <Button
            variant="outlined"
            size="small"
            startIcon={<RefreshRoundedIcon fontSize="small" />}
            onClick={loadData}
            disabled={loading}
            sx={{
              borderColor: 'divider',
              color: 'text.secondary',
              textTransform: 'none',
              borderRadius: 2,
              '&:hover': { bgcolor: 'rgba(0,0,0,0.04)' }
            }}
          >
            Làm mới
          </Button>
        }
      />

      <QueryState loading={loading} error={error} onRetry={loadData}>
        {report && (
          <Stack spacing={2.5}>
            {/* Quick Actions Alert Banner */}
            {(pendingShopsCount > 0 || pendingPayoutsCount > 0) && (
              <SellerCard title="Cần xử lý ngay">
                <Grid container spacing={2}>
                  {pendingShopsCount > 0 && (
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <Button
                        component={Link}
                        to="/admin/shops"
                        fullWidth
                        variant="outlined"
                        color="secondary"
                        sx={{
                          justifyContent: 'space-between',
                          textAlign: 'left',
                          py: 1.2,
                          px: 2,
                          borderRadius: 2
                        }}
                      >
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <StorefrontRoundedIcon fontSize="small" />
                          <span>Hồ sơ Shop chờ duyệt KYC</span>
                        </Box>
                        <Chip label={pendingShopsCount} size="small" color="warning" sx={{ fontWeight: 700 }} />
                      </Button>
                    </Grid>
                  )}
                  {pendingPayoutsCount > 0 && (
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <Button
                        component={Link}
                        to="/admin/payouts"
                        fullWidth
                        variant="outlined"
                        color="secondary"
                        sx={{
                          justifyContent: 'space-between',
                          textAlign: 'left',
                          py: 1.2,
                          px: 2,
                          borderRadius: 2
                        }}
                      >
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <AccountBalanceWalletRoundedIcon fontSize="small" />
                          <span>Yêu cầu giải ngân ví chờ duyệt</span>
                        </Box>
                        <Chip label={pendingPayoutsCount} size="small" color="warning" sx={{ fontWeight: 700 }} />
                      </Button>
                    </Grid>
                  )}
                </Grid>
              </SellerCard>
            )}

            {/* Core Financial Stat Cards */}
            <Grid container spacing={2}>
              {[
                {
                  label: 'Tổng GMV toàn sàn',
                  value: <MoneyText value={report.totalGmv || 0} />
                },
                {
                  label: 'Doanh thu phí sàn (5%)',
                  value: <MoneyText value={report.totalPlatformFeeRevenue || 0} />
                },
                {
                  label: 'Tổng số đơn hàng',
                  value: Number(report.totalOrdersCount || 0).toLocaleString('vi-VN')
                },
                {
                  label: 'Gian hàng hoạt động',
                  value: Number(report.totalActiveShopsCount || 0).toLocaleString('vi-VN')
                }
              ].map((item) => (
                <Grid key={item.label} size={{ xs: 12, sm: 6, xl: 3 }}>
                  <SellerCard>
                    <Typography color="text.secondary" sx={{ fontSize: '0.85rem', fontWeight: 500 }}>
                      {item.label}
                    </Typography>
                    <Typography component="div" variant="h5" sx={{ fontWeight: 800, mt: 1, color: '#0f172a' }}>
                      {item.value}
                    </Typography>
                  </SellerCard>
                </Grid>
              ))}
            </Grid>

            {/* Secondary Growth & Quality Metrics */}
            <Grid container spacing={2}>
              {[
                {
                  label: 'Người mua mới',
                  value: Number(report.newBuyerCount || 0).toLocaleString('vi-VN')
                },
                {
                  label: 'Người bán mới',
                  value: Number(report.newSellerCount || 0).toLocaleString('vi-VN')
                },
                {
                  label: 'Tỷ lệ trả hàng',
                  value: `${Number(report.returnRatePercent || 0).toFixed(1)}%`
                },
                {
                  label: 'Tỷ lệ tranh chấp',
                  value: `${Number(report.disputeRatePercent || 0).toFixed(1)}%`
                }
              ].map((item) => (
                <Grid key={item.label} size={{ xs: 12, sm: 6, xl: 3 }}>
                  <SellerCard>
                    <Typography color="text.secondary" sx={{ fontSize: '0.82rem', fontWeight: 500 }}>
                      {item.label}
                    </Typography>
                    <Typography component="div" variant="h6" sx={{ fontWeight: 700, mt: 0.5, color: '#334155' }}>
                      {item.value}
                    </Typography>
                  </SellerCard>
                </Grid>
              ))}
            </Grid>

            {/* Pending Payouts Table */}
            <SellerCard
              title="Yêu cầu giải ngân ví định kỳ"
              action={
                <Button component={Link} to="/admin/payouts" size="small" variant="text" sx={{ textTransform: 'none' }}>
                  Xem tất cả lệnh ví
                </Button>
              }
            >
              <DataTable
                rows={pendingPayoutsList}
                rowKey={(row) => row.id}
                loading={false}
                error={null}
                onRetry={loadData}
                emptyText="Hiện không có lệnh giải ngân nào đang chờ xử lý."
                columns={[
                  {
                    key: 'id',
                    label: 'Mã yêu cầu',
                    render: (row) => (
                      <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 600 }}>
                        {String(row.id).slice(0, 8)}...
                      </Typography>
                    )
                  },
                  {
                    key: 'amount',
                    label: 'Số tiền rút',
                    render: (row) => (
                      <Typography variant="body2" sx={{ fontWeight: 700, color: 'secondary.main' }}>
                        <MoneyText value={row.amount} />
                      </Typography>
                    )
                  },
                  {
                    key: 'bank',
                    label: 'Ngân hàng thụ hưởng',
                    render: (row) => (
                      <Box>
                        <Typography variant="body2" sx={{ fontWeight: 600 }}>
                          {row.bankName || 'Ngân hàng'}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {row.bankAccount}
                        </Typography>
                      </Box>
                    )
                  },
                  {
                    key: 'scheduledAt',
                    label: 'Thời gian hẹn',
                    render: (row) => (
                      <Typography variant="body2" color="text.secondary">
                        {row.scheduledAt ? new Date(row.scheduledAt).toLocaleString('vi-VN') : '—'}
                      </Typography>
                    )
                  }
                ]}
              />
            </SellerCard>
          </Stack>
        )}
      </QueryState>
    </>
  )
}

export default AdminOverviewPage
