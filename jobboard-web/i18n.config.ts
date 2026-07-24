// CLDR Arabic has 6 plural categories (zero/one/two/few/many/other), not the
// 2-3 forms English needs - vue-i18n's default plural selector only handles
// simple positional indices, so ar.json's pluralized keys (resultCount,
// applicantCount) provide 6 pipe-separated forms in this exact order, and this
// custom rule picks the right one per the standard CLDR Arabic algorithm.
function arabicPluralRule(choice: number): number {
  if (choice === 0) return 0 // zero
  if (choice === 1) return 1 // one
  if (choice === 2) return 2 // two
  const mod100 = choice % 100
  if (mod100 >= 3 && mod100 <= 10) return 3 // few
  if (mod100 >= 11 && mod100 <= 99) return 4 // many
  return 5 // other
}

export default defineI18nConfig(() => ({
  legacy: false,
  // Gaps in ar.json (translation still in progress) fall back to en.json
  // instead of rendering a raw missing-key string.
  fallbackLocale: 'en',
  pluralRules: {
    ar: arabicPluralRule
  }
}))
