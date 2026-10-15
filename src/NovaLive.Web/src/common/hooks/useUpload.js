import { useRef } from 'react'
import { toast } from 'react-toastify'

export const useUpload = ({ api, afterUpload, pendingMsg, successMsg, errorMsg }) => {
  const inputRef = useRef()

  const openFileDialog = () => inputRef.current?.click()

  const onFileChange = async (e) => {
    const file = e.target.files[0]
    if (!file) return
    await toast.promise(
      api(file).then(() => {
        afterUpload && afterUpload()
      }),
      {
        pending: pendingMsg || 'Đang upload...',
        success: successMsg || 'Tải lên thành công!',
        error: {
          render({ data }) {
            return data?.message || errorMsg || 'Tải lên thất bại!'
          }
        }
      }
    )
    e.target.value = ''
  }

  return {
    inputRef,
    openFileDialog,
    fileInputProps: {
      ref: inputRef,
      type: 'file',
      accept: '.png,.jpg,.jpeg',
      style: { display: 'none' },
      onChange: onFileChange
    }
  }
}