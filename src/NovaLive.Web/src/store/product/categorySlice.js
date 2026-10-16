import { createSlice, createAsyncThunk } from '@reduxjs/toolkit'
import { toast } from 'react-toastify'

const initialState = {
  categories: [],
  attributesBySlug: [],
  loading: false,
  error: null
}

export const fetchCategoriesAPI = createAsyncThunk(
  'category/fetchCategoriesAPI',
  async (params = {}, { rejectWithValue }) => {
    try {
      const { getAllCategoryAPI } = await import('~/common/apis/services/categoryService')
      const response = await getAllCategoryAPI(params)
      return response
    } catch (err) {
      return rejectWithValue(err?.response?.data || err?.message)
    }
  }
)

export const fetchAttributesBySlugAPI = createAsyncThunk(
  'category/fetchAttributesBySlugAPI',
  async (slugArr = [], { rejectWithValue }) => {
    try {
      const { getAttributeByCategorySlugAPI } = await import('~/common/apis/services/categoryService')
      const response = await getAttributeByCategorySlugAPI(slugArr)
      return response
    } catch (err) {
      return rejectWithValue(err?.response?.data || err?.message)
    }
  }
)

export const categorySlice = createSlice({
  name: 'category',
  initialState,
  reducers: {
    setCategories: (state, action) => { state.categories = action.payload || [] },
    setAttributesBySlug: (state, action) => { state.attributesBySlug = action.payload || [] },
    clearCategoryError: (state) => { state.error = null }
  },
  extraReducers: (builder) => {
    // fetchCategoriesAPI
    builder.addCase(fetchCategoriesAPI.pending, (state) => {
      state.loading = true
      state.error = null
    })
    builder.addCase(fetchCategoriesAPI.fulfilled, (state, action) => {
      state.loading = false
      state.categories = action.payload || []
    })
    builder.addCase(fetchCategoriesAPI.rejected, (state, action) => {
      state.loading = false
      state.error = action.payload || action.error?.message
      toast.error(action?.payload?.message || 'Fetch categories failed!')
    })

    // fetchAttributesBySlugAPI
    builder.addCase(fetchAttributesBySlugAPI.pending, (state) => {
      state.loading = true
      state.error = null
    })
    builder.addCase(fetchAttributesBySlugAPI.fulfilled, (state, action) => {
      state.loading = false
      state.attributesBySlug = action.payload || []
    })
    builder.addCase(fetchAttributesBySlugAPI.rejected, (state, action) => {
      state.loading = false
      state.error = action.payload || action.error?.message
      toast.error(action?.payload?.message || 'Fetch attributes failed!')
    })
  }
})

export const {
  setCategories,
  setAttributesBySlug,
  clearCategoryError
} = categorySlice.actions

export const selectCategories = (state) => state.category.categories
export const selectAttributesBySlug = (state) => state.category.attributesBySlug
export const selectCategoryLoading = (state) => state.category.loading
export const selectCategoryError = (state) => state.category.error

export const CategoryReducer = categorySlice.reducer