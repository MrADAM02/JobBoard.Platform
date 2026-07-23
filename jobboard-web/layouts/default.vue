<script setup lang="ts">
const auth = useAuthStore()
const router = useRouter()

function onLogout() {
  auth.clearAuth()
  router.push('/')
}
</script>

<template>
  <div class="min-h-screen flex flex-col bg-slate-50 text-slate-900 dark:bg-slate-950 dark:text-slate-100">
    <header class="border-b border-slate-200 bg-white dark:border-slate-800 dark:bg-slate-900">
      <nav class="mx-auto flex max-w-5xl items-center justify-between px-4 py-4">
        <NuxtLink to="/" class="text-lg font-semibold tracking-tight">
          JobBoard
        </NuxtLink>
        <div class="flex items-center gap-6 text-sm font-medium text-slate-600 dark:text-slate-400">
          <NuxtLink to="/jobs" class="hover:text-slate-900 dark:hover:text-slate-100">Browse Jobs</NuxtLink>

          <ThemeToggle />

          <ClientOnly>
            <template v-if="auth.isAuthenticated">
              <NuxtLink
                :to="auth.isEmployer ? '/dashboard/employer' : '/dashboard/candidate'"
                class="hover:text-slate-900 dark:hover:text-slate-100"
              >
                Dashboard
              </NuxtLink>
              <button class="hover:text-slate-900 dark:hover:text-slate-100" @click="onLogout">Log out</button>
            </template>
            <template v-else>
              <NuxtLink to="/login" class="hover:text-slate-900 dark:hover:text-slate-100">Log in</NuxtLink>
              <NuxtLink
                to="/register"
                class="rounded-md bg-slate-900 px-3 py-1.5 text-white hover:bg-slate-700 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-300"
              >
                Register
              </NuxtLink>
            </template>
          </ClientOnly>
        </div>
      </nav>
    </header>

    <main class="mx-auto w-full max-w-5xl flex-1 px-4 py-8">
      <slot />
    </main>

    <footer class="border-t border-slate-200 bg-white py-6 text-center text-sm text-slate-500 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-400">
      JobBoard &mdash; built with ASP.NET Core &amp; Nuxt
    </footer>
  </div>
</template>
