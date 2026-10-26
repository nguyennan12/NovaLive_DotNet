import { Alert, Box, Button, Checkbox, FormControlLabel, Stack, TextField, Typography } from '@mui/material'
import { useState } from 'react'
import { useBankDetails } from '../../hooks/useShop'
import { useRequestPayout } from '../../hooks/useWallet'
import { QueryState } from '../shared/QueryState'
import { ConfirmDialog } from '../shared/ConfirmDialog'
import { MIN_PAYOUT } from '../../constants'
import { money } from '../../utils/format'
import { validatePayout } from '../../utils/business'

export function PayoutForm({ balance }: { balance: number }) {
  const bank = useBankDetails(); const request = useRequestPayout()
  const [amount, setAmount] = useState(''); const [confirmedBank, setConfirmedBank] = useState(false); const [confirm, setConfirm] = useState(false)
  const value = Number(amount)
  const valid = validatePayout(value, balance)
  return <Stack spacing={2}>
    <Alert severity='info'>Rút tối thiểu {money(MIN_PAYOUT)} và không vượt số dư khả dụng. Khoản tiền sẽ được khóa trong lúc xử lý.</Alert>
    <QueryState loading={bank.isLoading} error={bank.error} onRetry={bank.refetch}>{bank.data && <Stack spacing={1}>
      <TextField label='Tài khoản ngân hàng KYC' value={bank.data.bankAccount || 'Chưa có thông tin tài khoản'} slotProps={{ input: { readOnly: true } }} />
      <TextField label='Ngân hàng' value={bank.data.bankName || 'Chưa được cung cấp trong hồ sơ trả về'} slotProps={{ input: { readOnly: true } }} />
      <TextField label='Chủ tài khoản' value={bank.data.bankHolder || 'Theo hồ sơ KYC đã xác minh'} slotProps={{ input: { readOnly: true } }} />
      <TextField label='Chi nhánh' value={bank.data.bankBranch || 'Theo hồ sơ KYC'} slotProps={{ input: { readOnly: true } }} />
      <Typography variant='caption' color='text.secondary'>Thông tin nhận tiền lấy từ hồ sơ KYC phía máy chủ. Màn hình này không thay đổi tài khoản nhận tiền.</Typography>
      {!bank.data.bankAccount && <Alert severity='warning'>Cần hồ sơ KYC đã xác minh và tài khoản ngân hàng trước khi rút tiền.</Alert>}
    </Stack>}</QueryState>
    <TextField type='number' label='Số tiền muốn rút (₫)' value={amount} onChange={event => setAmount(event.target.value)} error={amount !== '' && !valid} helperText={amount !== '' && !valid ? `Nhập số nguyên từ ${money(MIN_PAYOUT)} đến ${money(balance)}.` : 'Số tiền được tính bằng Việt Nam đồng.'} />
    <FormControlLabel label='Tôi xác nhận sử dụng tài khoản trong hồ sơ KYC.' control={<Checkbox checked={confirmedBank} onChange={(_, checked) => setConfirmedBank(checked)} />} />
    <Box><Button variant='contained' color='secondary' disabled={!valid || !confirmedBank || !bank.data?.bankAccount || request.isPending || bank.isFetching} onClick={() => setConfirm(true)}>Xem lại & Yêu cầu rút tiền</Button></Box>
    <ConfirmDialog open={confirm} title='Xác nhận yêu cầu rút tiền?' description={`Rút ${money(value)} về tài khoản ${bank.data?.bankAccount || ''}${bank.data?.bankName ? ` tại ${bank.data.bankName}` : ' theo hồ sơ KYC'}. Số tiền được khóa khỏi ví khả dụng và chờ xử lý.`} danger pending={request.isPending} disabled={!valid || !confirmedBank} onClose={() => setConfirm(false)} onConfirm={() => request.mutate(value, { onSuccess: () => { setConfirm(false); setAmount(''); setConfirmedBank(false) } })} />
  </Stack>
}
