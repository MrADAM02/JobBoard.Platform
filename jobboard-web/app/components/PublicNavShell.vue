<script setup lang="ts">
// The logged-out chrome: top nav + footer. Used by default.vue only when
// !auth.isAuthenticated (and as the SSR/pre-hydration best guess, since
// auth state is client-only) - see SidebarShell.vue for the authenticated
// chrome shown on every page once logged in. Because this only ever renders
// for logged-out visitors, it doesn't need its own auth branching (no
// Dashboard link, no logout button, no NotificationBell) the way the old
// combined default.vue did.
const { t } = useI18n()
const localePath = useLocalePath()

const mobileMenuOpen = ref(false)
const route = useRoute()
watch(() => route.fullPath, () => { mobileMenuOpen.value = false })
</script>

<template>
  <div class="min-h-screen flex flex-col bg-cream-100 text-slate-900 dark:bg-slate-950 dark:text-slate-100">
    <header class="border-b border-slate-200/70 bg-cream-100 dark:border-slate-800 dark:bg-slate-950">
      <nav class="mx-auto max-w-5xl px-4 py-4">
        <div class="flex items-center justify-between gap-4">
          <NuxtLink :to="localePath('/')" class="flex items-center gap-2 font-display text-lg font-bold tracking-tight text-slate-900 dark:text-slate-100">
            <span class="flex h-8 w-8 items-center justify-center rounded-lg bg-accent-400 text-slate-900">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" class="h-4 w-4"><rect x="7" y="7" width="10" height="10" rx="2" fill="none" stroke="currentColor" stroke-width="2" /></svg>
            </span>
            {{ t('common.brand') }}
          </NuxtLink>

          <!-- Desktop-only nav links: hidden below md, where the same links live in the collapsible panel instead. -->
          <div class="hidden flex-1 items-center gap-6 text-sm font-medium text-slate-600 dark:text-slate-400 md:flex">
            <NuxtLink :to="localePath('/jobs')" class="hover:text-slate-900 dark:hover:text-slate-100">{{ t('nav.browseJobs') }}</NuxtLink>
            <NuxtLink :to="localePath('/companies')" class="hover:text-slate-900 dark:hover:text-slate-100">{{ t('nav.companies') }}</NuxtLink>
          </div>

          <div class="flex items-center gap-1 sm:gap-2">
            <LocaleSwitcher />
            <ThemeToggle />

            <div class="hidden items-center gap-2 md:flex">
              <BaseButton :to="localePath('/login')" variant="secondary" size="sm">{{ t('nav.login') }}</BaseButton>
              <BaseButton :to="localePath('/register')" size="sm">{{ t('nav.register') }}</BaseButton>
            </div>

            <button
              type="button"
              class="rounded-md p-1.5 text-slate-600 hover:bg-slate-100 hover:text-slate-900 dark:text-slate-400 dark:hover:bg-slate-800 dark:hover:text-slate-100 md:hidden"
              :aria-label="t('nav.menu')"
              :aria-expanded="mobileMenuOpen"
              @click="mobileMenuOpen = !mobileMenuOpen"
            >
              <svg v-if="!mobileMenuOpen" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="h-6 w-6">
                <path d="M4 6h16M4 12h16M4 18h16" />
              </svg>
              <svg v-else xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="h-6 w-6">
                <path d="M6 6l12 12M18 6L6 18" />
              </svg>
            </button>
          </div>
        </div>

        <div v-if="mobileMenuOpen" class="mt-4 flex flex-col gap-4 border-t border-slate-200/70 pt-4 text-sm font-medium text-slate-600 dark:border-slate-800 dark:text-slate-400 md:hidden">
          <NuxtLink :to="localePath('/jobs')" class="hover:text-slate-900 dark:hover:text-slate-100">{{ t('nav.browseJobs') }}</NuxtLink>
          <NuxtLink :to="localePath('/companies')" class="hover:text-slate-900 dark:hover:text-slate-100">{{ t('nav.companies') }}</NuxtLink>
          <div class="flex gap-2">
            <BaseButton :to="localePath('/login')" variant="secondary" class="flex-1 justify-center">{{ t('nav.login') }}</BaseButton>
            <BaseButton :to="localePath('/register')" class="flex-1 justify-center">{{ t('nav.register') }}</BaseButton>
          </div>
        </div>
      </nav>
    </header>

    <main class="mx-auto w-full max-w-5xl flex-1 px-4 py-8">
      <slot />
    </main>

    <footer class="border-t border-slate-200/70 bg-cream-100 py-6 text-center text-sm text-slate-500 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-400">
      {{ t('footer.tagline') }}
    </footer>
  </div>
</template>
