import { defineVitestConfig } from '@nuxt/test-utils/config'

// @nuxt/test-utils' config wires up Nuxt's auto-imports (ref, useI18n, ...)
// inside tests without hand-rolling a separate Vite/alias config.
export default defineVitestConfig({
  test: {
    environment: 'happy-dom'
  }
})
