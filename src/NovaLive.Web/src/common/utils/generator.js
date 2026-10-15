export const generateCombinations = (variations) => {
  const validVars = variations.filter(v => v.name?.trim() && v.options?.length > 0)
  if (validVars.length === 0) return []

  let results = [{ options: [], indices: [] }]

  validVars.forEach((v) => {
    const nextResults = []
    results.forEach(res => {
      v.options.forEach((opt, oIdx) => {
        nextResults.push({
          options: [...res.options, opt], // (Đỏ, S)
          indices: [...res.indices, oIdx] // ( 0, 1)
        })
      })
    })
    results = nextResults
  })
  return results
}