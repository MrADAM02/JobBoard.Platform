// @vitest-environment nuxt
//
// Needs the full Nuxt test environment (not the default happy-dom one) since
// Pagination.vue calls useI18n() and auto-imports BaseButton - mountSuspended
// resolves both, unlike a plain @vue/test-utils mount would.
import { describe, expect, it } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import Pagination from '../components/Pagination.vue'

function renderedPageLabels(text: string) {
  // Strip the prev/next arrow buttons' (empty) text and the whitespace
  // between elements, leaving just the page-number/ellipsis sequence.
  return text.replace(/\s+/g, ' ').trim()
}

describe('Pagination', () => {
  it('shows every page when the total is small', async () => {
    const wrapper = await mountSuspended(Pagination, {
      props: { pageNumber: 2, totalPages: 4, hasPreviousPage: true, hasNextPage: true }
    })

    const buttons = wrapper.findAll('button').filter((b) => /^\d+$/.test(b.text()))
    expect(buttons.map((b) => b.text())).toEqual(['1', '2', '3', '4'])
  })

  it('collapses a large page count into an ellipsis around the current page', async () => {
    const wrapper = await mountSuspended(Pagination, {
      props: { pageNumber: 10, totalPages: 20, hasPreviousPage: true, hasNextPage: true }
    })

    const text = renderedPageLabels(wrapper.text())
    // Always page 1, a window around 10 (9/10/11), and the last page, with
    // single ellipses filling the gaps - never every page 1..20.
    expect(text).toContain('1')
    expect(text).toContain('9')
    expect(text).toContain('10')
    expect(text).toContain('11')
    expect(text).toContain('20')
    expect(wrapper.findAll('button').some((b) => b.text() === '2')).toBe(false)
  })

  it('marks the current page with aria-current', async () => {
    const wrapper = await mountSuspended(Pagination, {
      props: { pageNumber: 3, totalPages: 5, hasPreviousPage: true, hasNextPage: true }
    })

    const current = wrapper.find('[aria-current="page"]')
    expect(current.exists()).toBe(true)
    expect(current.text()).toBe('3')
  })

  it('emits change with the target page number when a page button is clicked', async () => {
    const wrapper = await mountSuspended(Pagination, {
      props: { pageNumber: 1, totalPages: 5, hasPreviousPage: false, hasNextPage: true }
    })

    // With pageNumber 1, the near-current window only covers pages 1-2 - page
    // 2 is the first page guaranteed to render alongside the edges (1 and 5).
    const buttons = wrapper.findAll('button').filter((b) => /^\d+$/.test(b.text()))
    const pageTwo = buttons.find((b) => b.text() === '2')!
    await pageTwo.trigger('click')

    expect(wrapper.emitted('change')).toEqual([[2]])
  })
})
