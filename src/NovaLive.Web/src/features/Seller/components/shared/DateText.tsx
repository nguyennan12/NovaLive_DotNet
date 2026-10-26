import { dateText } from '../../utils/format'

export function DateText({ value, withTime = true }: { value: string | null | undefined; withTime?: boolean }) { return <time dateTime={value || undefined}>{dateText(value, withTime)}</time> }
