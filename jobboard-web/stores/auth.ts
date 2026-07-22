import { defineStore } from 'pinia'
import type { AuthResult } from '~/types/auth'

const STORAGE_KEY = 'jobboard.auth'

interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  userId: string | null
  email: string | null
  role: AuthResult['role'] | null
}

// Direct cross-origin calls + client-side refresh-token rotation, not an
// httpOnly-cookie BFF proxy - the API's CORS policy is already scoped for
// this (see JobBoard.Api's Program.cs "NuxtFrontend" policy), and adding a
// server-route proxy layer wouldn't buy anything for a portfolio demo.
export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    accessToken: null,
    refreshToken: null,
    userId: null,
    email: null,
    role: null
  }),

  getters: {
    isAuthenticated: (state) => !!state.accessToken,
    isEmployer: (state) => state.role === 'Employer',
    isCandidate: (state) => state.role === 'Candidate'
  },

  actions: {
    setAuth(result: AuthResult) {
      this.accessToken = result.accessToken
      this.refreshToken = result.refreshToken
      this.userId = result.userId
      this.email = result.email
      this.role = result.role
      this.persist()
    },

    clearAuth() {
      this.accessToken = null
      this.refreshToken = null
      this.userId = null
      this.email = null
      this.role = null
      if (import.meta.client) {
        localStorage.removeItem(STORAGE_KEY)
      }
    },

    persist() {
      if (import.meta.client) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(this.$state))
      }
    },

    hydrateFromStorage() {
      if (!import.meta.client) return
      const raw = localStorage.getItem(STORAGE_KEY)
      if (!raw) return
      try {
        this.$patch(JSON.parse(raw))
      } catch {
        localStorage.removeItem(STORAGE_KEY)
      }
    }
  }
})
