<script setup lang="ts">
const auth = useAuthStore()
const router = useRouter()
const route = useRoute()
const { t } = useI18n()
const localePath = useLocalePath()

const mobileMenuOpen = ref(false)

// Closing on route change covers both link clicks and any programmatic
// navigation (e.g. onLogout below), so the panel never stays open post-navigation.
watch(() => route.fullPath, () => { mobileMenuOpen.value = false })

function onLogout() {
  auth.clearAuth()
  router.push(localePath('/'))
}
</script>

<template>
  <div class="min-h-screen flex flex-col bg-slate-50 text-slate-900 dark:bg-slate-950 dark:text-slate-100">
    <header class="border-b border-slate-200 bg-white shadow-sm dark:border-slate-800 dark:bg-slate-900">
      <nav class="mx-auto max-w-5xl px-4 py-4">
        <div class="flex items-center justify-between gap-4">
          <NuxtLink :to="localePath('/')" class="text-lg font-bold tracking-tight text-primary-600 dark:text-primary-400">
            {{ t('common.brand') }}
          </NuxtLink>

          <!-- Desktop-only nav links: hidden below md, where the same links live in the collapsible panel instead. -->
          <div class="hidden flex-1 items-center gap-6 text-sm font-medium text-slate-600 dark:text-slate-400 md:flex">
            <NuxtLink :to="localePath('/jobs')" class="hover:text-slate-900 dark:hover:text-slate-100">{{ t('nav.browseJobs') }}</NuxtLink>
            <NuxtLink :to="localePath('/companies')" class="hover:text-slate-900 dark:hover:text-slate-100">{{ t('nav.companies') }}</NuxtLink>

            <ClientOnly>
              <template v-if="auth.isAuthenticated">
                <NuxtLink
                  :to="localePath(auth.isEmployer ? '/dashboard/employer' : '/dashboard/candidate')"
                  class="hover:text-slate-900 dark:hover:text-slate-100"
                >
                  {{ t('nav.dashboard') }}
                </NuxtLink>
              </template>
              <template v-else>
                <NuxtLink :to="localePath('/login')" class="hover:text-slate-900 dark:hover:text-slate-100">{{ t('nav.login') }}</NuxtLink>
              </template>
            </ClientOnly>
          </div>

          <!-- Always-visible controls, single instance regardless of viewport - avoids mounting NotificationBell
               twice (which would double its fetch and let its two local read-states drift apart). -->
          <div class="flex items-center gap-1 sm:gap-2">
            <LocaleSwitcher />
            <ThemeToggle />
            <ClientOnly>
              <NotificationBell v-if="auth.isAuthenticated" />
            </ClientOnly>
            <BaseButton v-if="!auth.isAuthenticated" :to="localePath('/register')" size="sm" class="hidden md:inline-flex">
              {{ t('nav.register') }}
            </BaseButton>
            <button
              v-if="auth.isAuthenticated"
              class="hidden text-sm font-medium text-slate-600 hover:text-slate-900 dark:text-slate-400 dark:hover:text-slate-100 md:inline-block"
              @click="onLogout"
            >
              {{ t('nav.logout') }}
            </button>

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

        <div v-if="mobileMenuOpen" class="mt-4 flex flex-col gap-4 border-t border-slate-200 pt-4 text-sm font-medium text-slate-600 dark:border-slate-800 dark:text-slate-400 md:hidden">
          <NuxtLink :to="localePath('/jobs')" class="hover:text-slate-900 dark:hover:text-slate-100">{{ t('nav.browseJobs') }}</NuxtLink>
          <NuxtLink :to="localePath('/companies')" class="hover:text-slate-900 dark:hover:text-slate-100">{{ t('nav.companies') }}</NuxtLink>

          <ClientOnly>
            <template v-if="auth.isAuthenticated">
              <NuxtLink
                :to="localePath(auth.isEmployer ? '/dashboard/employer' : '/dashboard/candidate')"
                class="hover:text-slate-900 dark:hover:text-slate-100"
              >
                {{ t('nav.dashboard') }}
              </NuxtLink>
              <button class="text-start hover:text-slate-900 dark:hover:text-slate-100" @click="onLogout">{{ t('nav.logout') }}</button>
            </template>
            <template v-else>
              <NuxtLink :to="localePath('/login')" class="hover:text-slate-900 dark:hover:text-slate-100">{{ t('nav.login') }}</NuxtLink>
              <BaseButton :to="localePath('/register')" class="justify-center">{{ t('nav.register') }}</BaseButton>
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
