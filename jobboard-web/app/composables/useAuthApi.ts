import type { AuthResult, LoginPayload, RegisterPayload } from '~/types/auth'

// Unauthenticated calls (no token to attach yet) - plain $fetch, not useAuthFetch.
export function useAuthApi() {
  const config = useRuntimeConfig()
  const apiBase = config.public.apiBase as string

  function register(payload: RegisterPayload) {
    return $fetch<AuthResult>(`${apiBase}/auth/register`, { method: 'POST', body: payload })
  }

  function login(payload: LoginPayload) {
    return $fetch<AuthResult>(`${apiBase}/auth/login`, { method: 'POST', body: payload })
  }

  return { register, login }
}
