// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },

  modules: ['@nuxtjs/tailwindcss', '@pinia/nuxt', '@nuxtjs/color-mode', '@nuxtjs/i18n'],

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
  },

  // prefix_except_default gives /jobs (en) and /ar/jobs (ar) as separate,
  // independently indexable URLs with real hreflang alternates - required
  // for the SSR/SEO discipline already established for the public job pages.
  // redirectOn: 'root' stops browser-language guessing on deep pages like
  // /jobs/[id], so the SSR response for a given URL stays deterministic for crawlers.
  i18n: {
    // "language" (not "iso") is the field @nuxtjs/i18n's useLocaleHead actually
    // reads to populate <html lang> and hreflang alternate links - using "iso"
    // silently produces neither (confirmed by inspecting the module's head.js).
    locales: [
      { code: 'en', language: 'en-US', name: 'English', dir: 'ltr', file: 'en.json' },
      { code: 'ar', language: 'ar-SA', name: 'العربية', dir: 'rtl', file: 'ar.json' }
    ],
    defaultLocale: 'en',
    baseUrl: process.env.NUXT_PUBLIC_SITE_URL || 'http://localhost:3000',
    langDir: 'locales/',
    vueI18n: './i18n.config.ts',
    lazy: true,
    strategy: 'prefix_except_default',
    detectBrowserLanguage: {
      useCookie: true,
      cookieKey: 'i18n_redirected',
      redirectOn: 'root',
      alwaysRedirect: false
    }
  }
})
