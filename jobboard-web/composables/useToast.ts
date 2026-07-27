export interface Toast {
  id: number
  message: string
  variant: 'success' | 'error'
}

// Module-level (not inside the composable function) so every caller shares
// the same queue and <ToastContainer> - a real global toast, not a
// per-component one. Only ever mutated from client-side @click handlers, so
// there's no cross-request SSR leakage risk despite the shared module state.
const toasts = ref<Toast[]>([])
let nextId = 0

export function useToast() {
  function push(message: string, variant: Toast['variant']) {
    const id = nextId++
    toasts.value.push({ id, message, variant })
    setTimeout(() => {
      toasts.value = toasts.value.filter((t) => t.id !== id)
    }, 4000)
  }

  return {
    toasts,
    success: (message: string) => push(message, 'success'),
    error: (message: string) => push(message, 'error')
  }
}
