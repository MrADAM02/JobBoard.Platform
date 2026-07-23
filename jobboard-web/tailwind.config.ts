import type { Config } from 'tailwindcss'

// darkMode: 'class' is required to pair with @nuxtjs/color-mode's class-based
// toggling (classSuffix: '' in nuxt.config.ts makes it emit a bare "dark"/"light"
// class on <html>). Tailwind's default "media" strategy would ignore the
// cookie-persisted user choice and follow only OS prefers-color-scheme.
export default {
  darkMode: 'class',
  // Introducing this config file turns off @nuxtjs/tailwindcss's zero-config
  // content auto-detection, so every directory containing class strings must
  // be listed explicitly here.
  content: [
    './components/**/*.{vue,js,ts}',
    './layouts/**/*.vue',
    './pages/**/*.vue',
    './composables/**/*.{js,ts}',
    './app.vue',
    './error.vue'
  ],
  theme: {
    extend: {}
  },
  plugins: []
} satisfies Config
