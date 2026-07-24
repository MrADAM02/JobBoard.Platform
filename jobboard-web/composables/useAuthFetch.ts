import type { AuthResult } from '~/types/auth'

// Authenticated $fetch: attaches the current access token, and on a single 401
// tries one refresh-token rotation before retrying the original request. If the
// refresh itself fails, the session is dead - clear it and bounce to /login.
export function useAuthFetch() {
  const config = useRuntimeConfig()
  const apiBase = config.public.apiBase as string
  const auth = useAuthStore()

  let refreshPromise: Promise<boolean> | null = null

  async function doRefresh(): Promise<boolean> {
    if (!auth.refreshToken) return false
    try {
      const result = await $fetch<AuthResult>(`${apiBase}/auth/refresh`, {
        method: 'POST',
        body: { refreshToken: auth.refreshToken }
      })
      auth.setAuth(result)
      return true
    } catch {
      return false
    }
  }

  async function authFetch<T>(path: string, options: Parameters<typeof $fetch>[1] = {}): Promise<T> {
    const run = () =>
      $fetch<T>(`${apiBase}${path}`, {
        ...options,
        headers: {
          ...(options.headers as Record<string, string> | undefined),
          ...(auth.accessToken ? { Authorization: `Bearer ${auth.accessToken}` } : {})
        }
      })

    try {
      return await run()
    } catch (err) {
      const status = (err as { response?: { status?: number } })?.response?.status
      if (status !== 401) throw err

      refreshPromise ??= doRefresh().finally(() => {
        refreshPromise = null
      })
      const refreshed = await refreshPromise

      if (!refreshed) {
        auth.clearAuth()
        const localePath = useLocalePath()
        await navigateTo(localePath('/login'))
        throw err
      }

      return await run()
    }
  }

  return { authFetch }
}
