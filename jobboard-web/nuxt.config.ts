// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },

  modules: ['@nuxtjs/tailwindcss'],

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
  }
})
