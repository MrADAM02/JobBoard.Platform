import { describe, expect, it } from 'vitest'
import { arabicPluralRule } from '../i18n.config'

// CLDR Arabic plural categories: zero/one/two/few/many/other (indices 0-5),
// matching the 6 pipe-separated forms every pluralized key in ar.json provides.
describe('arabicPluralRule', () => {
  it('maps 0 to the zero form', () => {
    expect(arabicPluralRule(0)).toBe(0)
  })

  it('maps 1 to the one form', () => {
    expect(arabicPluralRule(1)).toBe(1)
  })

  it('maps 2 to the two form', () => {
    expect(arabicPluralRule(2)).toBe(2)
  })

  it('maps 3-10 to the few form', () => {
    expect(arabicPluralRule(3)).toBe(3)
    expect(arabicPluralRule(10)).toBe(3)
  })

  it('maps 11-99 to the many form', () => {
    expect(arabicPluralRule(11)).toBe(4)
    expect(arabicPluralRule(99)).toBe(4)
  })

  it('maps 100+ to the other form', () => {
    expect(arabicPluralRule(100)).toBe(5)
  })

  it('re-applies the mod-100 few/many bands past 100 (e.g. 103, 111)', () => {
    expect(arabicPluralRule(103)).toBe(3) // 103 % 100 = 3 -> few
    expect(arabicPluralRule(111)).toBe(4) // 111 % 100 = 11 -> many
  })
})
