import type { AuthResult } from '~/types/auth'

// Called from a page's <script setup>, after the `auth` route middleware has
// already guaranteed the user is logged in - this only enforces the *role*,
// bouncing an employer away from candidate-only pages and vice versa.
export function useRequireRole(role: AuthResult['role']) {
  const auth = useAuthStore()
  if (import.meta.client && auth.role !== role) {
    navigateTo(auth.isEmployer ? '/dashboard/employer' : '/dashboard/candidate')
  }
}
