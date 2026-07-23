<script setup lang="ts">
const auth = useAuthStore()
const router = useRouter()
const { t } = useI18n()

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
          {{ t('common.brand') }}
        </NuxtLink>
        <div class="flex items-center gap-6 text-sm font-medium text-slate-600 dark:text-slate-400">
          <NuxtLink to="/jobs" class="hover:text-slate-900 dark:hover:text-slate-100">{{ t('nav.browseJobs') }}</NuxtLink>

          <LocaleSwitcher />
          <ThemeToggle />

          <ClientOnly>
            <template v-if="auth.isAuthenticated">
              <NuxtLink
                :to="auth.isEmployer ? '/dashboard/employer' : '/dashboard/candidate'"
                class="hover:text-slate-900 dark:hover:text-slate-100"
              >
                {{ t('nav.dashboard') }}
              </NuxtLink>
              <button class="hover:text-slate-900 dark:hover:text-slate-100" @click="onLogout">{{ t('nav.logout') }}</button>
            </template>
            <template v-else>
              <NuxtLink to="/login" class="hover:text-slate-900 dark:hover:text-slate-100">{{ t('nav.login') }}</NuxtLink>
              <NuxtLink
                to="/register"
                class="rounded-md bg-slate-900 px-3 py-1.5 text-white hover:bg-slate-700 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-300"
              >
                {{ t('nav.register') }}
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
      {{ t('footer.tagline') }}
    </footer>
  </div>
</template>
