<script setup lang="ts">
// The only layout in the app - picks between the two chrome variants based
// on auth state: SidebarShell (workspace nav) once logged in, on every page
// including public ones; PublicNavShell (top nav) for logged-out visitors.
// Auth state is only known client-side (hydrated from localStorage, see
// stores/auth.ts), so the choice is wrapped in ClientOnly with
// PublicNavShell as the SSR/pre-hydration fallback - the correct guess for
// crawlers and the common logged-out case.
const auth = useAuthStore()
</script>

<template>
  <ClientOnly>
    <SidebarShell v-if="auth.isAuthenticated">
      <slot />
    </SidebarShell>
    <PublicNavShell v-else>
      <slot />
    </PublicNavShell>

    <template #fallback>
      <PublicNavShell>
        <slot />
      </PublicNavShell>
    </template>
  </ClientOnly>
</template>
