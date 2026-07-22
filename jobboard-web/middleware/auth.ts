// Auth state lives client-side only (localStorage, hydrated by
// plugins/auth.client.ts) - the server has no way to know if a request is
// authenticated, so this only enforces on the client, after hydration. These
// pages are all behind a login wall anyway, so there's no SSR/SEO reason to
// gate them server-side too.
export default defineNuxtRouteMiddleware((to) => {
  if (import.meta.server) return

  const auth = useAuthStore()
  if (!auth.isAuthenticated) {
    return navigateTo(`/login?redirect=${encodeURIComponent(to.fullPath)}`)
  }
})
