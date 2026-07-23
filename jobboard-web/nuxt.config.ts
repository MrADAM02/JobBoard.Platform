// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },

  modules: ['@nuxtjs/tailwindcss', '@pinia/nuxt', '@nuxtjs/color-mode'],

  css: ['~/assets/css/main.css'],

  typescript: {
    strict: true
  },

  runtimeConfig: {
    public: {
      // Points at JobBoard.Api's http profile (localhost:5000) in dev to avoid the
      // self-signed HTTPS dev cert tripping up server-side fetches during SSR.
      apiBase: process.env.NUXT_PUBLIC_API_BASE || 'http://localhost:5000/api'
    }
  },

  // Cookie-backed (not localStorage) so the correct theme renders in the very
  // first SSR response for /jobs and /jobs/[id] - no flash of the wrong theme,
  // matching the SSR/SEO discipline used for those pages.
  colorMode: {
    preference: 'system',
    fallback: 'light',
    classSuffix: '', // emits a bare "dark"/"light" class, pairing with tailwind.config.ts's darkMode: 'class'
    storage: 'cookie',
    storageKey: 'color-mode'
  }
})
