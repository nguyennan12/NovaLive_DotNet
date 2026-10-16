import ImageIcon from '@mui/icons-material/Image'
import { Box, CircularProgress, FormHelperText, Paper, Typography } from '@mui/material'
import { useMemo, useRef, useState } from 'react'
import { Controller, useFormContext, useWatch } from 'react-hook-form'
import { toast } from 'react-toastify'
import { uploadSingleImageAPI } from '~/common/apis/services/uploadService'

const ThumbnailUpload = () => {
  const { control, setValue } = useFormContext()
  const [uploading, setUploading] = useState(false)


  const spuThumb = useWatch({ control, name: 'spu_thumb' })

  const preview = useMemo(() => {
    if (!spuThumb) return null
    if (typeof spuThumb === 'string') return spuThumb
    return spuThumb.thumb_url || spuThumb.image_url || null
  }, [spuThumb])

  const inputRef = useRef(null)

  const uploadFile = async (file) => {
    if (!file) return
    try {
      setUploading(true)
      const uploaded = await uploadSingleImageAPI(file)
      setValue('spu_thumb', uploaded.thumb_url, { shouldDirty: true, shouldValidate: true })
      toast.success('Uploaded thumbnail!')
    } catch (e) {
      toast.error(e?.message || 'Upload thumbnail failed!')
    } finally {
      setUploading(false)
    }
  }

  const onDrop = (e) => {
    e.preventDefault()
    const file = e.dataTransfer?.files?.[0]
    if (file) uploadFile(file)
  }

  return (
    <Controller
      name="spu_thumb"
      control={control}
      // rules={{ required: 'Product thumbnail is required' }}
      render={({ fieldState: { error } }) => (
        <Box>
          <Paper
            variant="outlined"
            onClick={() => inputRef.current?.click()}
            onDragOver={(e) => e.preventDefault()}
            onDrop={onDrop}
            sx={{
              p: 2,
              borderRadius: 3,
              border: '1.5px dashed',
              borderColor: error ? 'error.main' : 'secondary.main',
              background: error ? 'error.50' : 'rgba(52,101,200,0.04)',
              cursor: uploading ? 'not-allowed' : 'pointer',
              transition: 'all 0.2s',
              textAlign: 'center',
              minHeight: 200,
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              justifyContent: 'center',
              gap: 1,
              position: 'relative',
              overflow: 'hidden',
              opacity: uploading ? 0.8 : 1
            }}
          >
            <input
              ref={inputRef}
              type="file"
              accept=".png,.jpg,.jpeg"
              style={{ display: 'none' }}
              disabled={uploading}
              onChange={(e) => {
                const file = e.target.files?.[0]
                if (file) uploadFile(file)
                e.target.value = ''
              }}
            />

            {preview ? (
              <Box
                component="img"
                src={preview}
                alt="Preview"
                sx={{ maxWidth: '100%', maxHeight: 180, objectFit: 'contain', borderRadius: 2 }}
              />
            ) : (
              <>
                <Box
                  sx={{
                    width: 56,
                    height: 56,
                    borderRadius: '50%',
                    background: error ? 'transparent' : 'linear-gradient(135deg, #eef2ff, #dbeafe)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    mb: 1
                  }}
                >
                  <ImageIcon
                    sx={{
                      color: error ? 'error.main' : 'primary.main',
                      fontSize: 28
                    }}
                  />
                </Box>

                <Typography variant="body2" fontWeight={600} color={error ? 'error.main' : 'primary.contrastText'}>
                  {uploading ? 'Uploading...' : 'Click or drop image here'}
                </Typography>
                <Typography variant="caption" color={error ? 'error.main' : 'primary.contrastText'}>
                  Only *.png, *.jpg and *.jpeg accepted
                </Typography>
              </>
            )}

            {uploading && (
              <Box sx={{ position: 'absolute', inset: 0, display: 'grid', placeItems: 'center', bgcolor: 'rgba(0,0,0,0.25)' }}>
                <CircularProgress size={28} />
              </Box>
            )}
          </Paper>

          {error && (
            <FormHelperText error sx={{ mt: 1, fontWeight: 500 }}>
              {error.message}
            </FormHelperText>
          )}
        </Box>
      )}
    />
  )
}

export default ThumbnailUpload