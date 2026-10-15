
export const FIELD_REQUIRED_MESSAGE = 'This field is required.'
export const EMAIL_RULE = /^\S+@\S+\.\S+$/
export const EMAIL_RULE_MESSAGE = 'Email is invalid. (example@gmail.com)'
export const PHONE_RULE = /^(0|\+84)[35789][0-9]{8}$/
export const PHONE_RULE_MESSAGE = 'Phone number is invalid. (e.g. 0912345678 or +84912345678)'
export const PASSWORD_RULE = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d\W_]{8,72}$/
export const PASSWORD_RULE_MESSAGE = 'Password must be at least 8 characters and include uppercase, lowercase, and a number.'
export const PASSWORD_CONFIRMATION_MESSAGE = 'Password Confirmation does not match!'



// Liên quan đến Validate File
export const LIMIT_COMMON_FILE_SIZE = 10485760 // byte = 10 MB
export const ALLOW_COMMON_FILE_TYPES = ['image/jpg', 'image/jpeg', 'image/png']
export const singleFileValidator = (file) => {
  if (!file || !file.name || !file.size || !file.type) {
    return 'File cannot be blank.'
  }
  if (file.size > LIMIT_COMMON_FILE_SIZE) {
    return 'Maximum file size exceeded. (10MB)'
  }
  if (!ALLOW_COMMON_FILE_TYPES.includes(file.type)) {
    return 'File type is invalid. Only accept jpg, jpeg and png'
  }
  return null
}

export function validateForm(form) {
  const errors = {}
  if (!form.name.trim()) errors.name = 'Tên không được để trống'
  if (!form.value || isNaN(+form.value) || +form.value <= 0)
    errors.value = 'Giá trị phải lớn hơn 0'
  if (form.type === 'percentage' && +form.value > 100)
    errors.value = 'Giá trị % không được vượt quá 100'
  if (!form.startDate) errors.startDate = 'Chọn ngày bắt đầu'
  if (!form.endDate) errors.endDate = 'Chọn ngày kết thúc'
  if (form.startDate && form.endDate && form.endDate < form.startDate)
    errors.endDate = 'Ngày kết thúc phải sau ngày bắt đầu'
  if (form.usageLimit && (isNaN(+form.usageLimit) || +form.usageLimit < 1))
    errors.usageLimit = 'Giới hạn phải là số nguyên dương'
  return errors
}
