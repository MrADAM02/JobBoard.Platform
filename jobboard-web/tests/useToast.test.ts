import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

// useToast's queue is module-level (a real shared global, not per-component -
// see the composable's own comment), so each test re-imports the module after
// vi.resetModules() to get an isolated queue instead of leaking state between tests.
describe('useToast', () => {
  beforeEach(() => {
    vi.resetModules()
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('pushes a success toast onto the queue', async () => {
    const { useToast } = await import('../composables/useToast')
    const { toasts, success } = useToast()

    success('Job posted.')

    expect(toasts.value).toHaveLength(1)
    expect(toasts.value[0]).toMatchObject({ message: 'Job posted.', variant: 'success' })
  })

  it('pushes an error toast', async () => {
    const { useToast } = await import('../composables/useToast')
    const { toasts, error } = useToast()

    error('Something went wrong.')

    expect(toasts.value[0]).toMatchObject({ message: 'Something went wrong.', variant: 'error' })
  })

  it('auto-dismisses a toast after 4 seconds', async () => {
    const { useToast } = await import('../composables/useToast')
    const { toasts, success } = useToast()

    success('Temporary')
    expect(toasts.value).toHaveLength(1)

    vi.advanceTimersByTime(4000)

    expect(toasts.value).toHaveLength(0)
  })

  it('does not dismiss early, just before the 4-second mark', async () => {
    const { useToast } = await import('../composables/useToast')
    const { toasts, success } = useToast()

    success('Still here')
    vi.advanceTimersByTime(3999)

    expect(toasts.value).toHaveLength(1)
  })
})
