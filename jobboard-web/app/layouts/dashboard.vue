<script setup lang="ts">
// Persistent left sidebar for the authenticated dashboard area, replacing
// the shared top-nav header (default.vue) for these pages only. Nav items
// are role-specific (candidate/employer/admin each have their own routes)
// but share the same shell/style, per the approved redesign plan.
//
// One <aside> and one <main>/<slot> only - on mobile it's translated
// off-canvas and slid in via mobileOpen, rather than duplicating the sidebar
// markup (and re-mounting NotificationBell / page content) once per
// breakpoint the way two separate "mobile" and "desktop" trees would.
const auth = useAuthStore()
const router = useRouter()
const route = useRoute()
const { t } = useI18n()
const localePath = useLocalePath()

const mobileOpen = ref(false)
watch(() => route.fullPath, () => { mobileOpen.value = false })

interface NavItem {
  label: string
  to: string
  icon: 'grid' | 'document' | 'bookmark' | 'building' | 'chart' | 'people'
}

const navItems = computed<NavItem[]>(() => {
  if (auth.isEmployer) {
    return [
      { label: t('sidebar.overview'), to: '/dashboard/employer', icon: 'grid' },
      { label: t('sidebar.jobListings'), to: '/dashboard/employer/jobs', icon: 'document' },
      { label: t('sidebar.analytics'), to: '/dashboard/employer/analytics', icon: 'chart' },
      { label: t('sidebar.company'), to: '/dashboard/employer/company', icon: 'building' }
    ]
  }
  if (auth.isAdmin) {
    return [
      { label: t('sidebar.overview'), to: '/dashboard/admin', icon: 'grid' },
      { label: t('sidebar.jobs'), to: '/dashboard/admin/jobs', icon: 'document' },
      { label: t('sidebar.users'), to: '/dashboard/admin/users', icon: 'people' }
    ]
  }
  return [
    { label: t('sidebar.overview'), to: '/dashboard/candidate', icon: 'grid' },
    { label: t('sidebar.applications'), to: '/dashboard/candidate/applications', icon: 'document' },
    { label: t('sidebar.savedJobs'), to: '/dashboard/candidate/saved-jobs', icon: 'bookmark' },
    { label: t('sidebar.companies'), to: '/companies', icon: 'building' }
  ]
})

function isActive(to: string) {
  const target = localePath(to)
  return route.path === target || route.path.startsWith(`${target}/`)
}

function initials(email: string | null) {
  return (email ?? '?').charAt(0).toUpperCase()
}

function onLogout() {
  auth.clearAuth()
  router.push(localePath('/'))
}
</script>

<template>
  <div class="min-h-screen bg-cream-100 text-slate-900 dark:bg-slate-950 dark:text-slate-100 md:flex">
    <div v-if="mobileOpen" class="fixed inset-0 z-40 bg-slate-900/40 md:hidden" @click="mobileOpen = false" />

    <aside
      class="fixed inset-y-0 start-0 z-50 flex w-72 max-w-[80vw] -translate-x-full flex-col gap-1 overflow-y-auto border-e border-slate-200/70 bg-white px-4 py-6 transition-transform rtl:translate-x-full dark:border-slate-800 dark:bg-slate-900 md:static md:z-auto md:w-64 md:max-w-none md:translate-x-0 md:rtl:translate-x-0"
      :class="mobileOpen ? 'translate-x-0 rtl:translate-x-0' : ''"
    >
      <NuxtLink :to="localePath('/')" class="flex items-center gap-2 px-2 font-display text-lg font-bold tracking-tight">
        <span class="flex h-8 w-8 items-center justify-center rounded-lg bg-accent-400 text-slate-900">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" class="h-4 w-4"><rect x="7" y="7" width="10" height="10" rx="2" fill="none" stroke="currentColor" stroke-width="2" /></svg>
        </span>
        {{ t('common.brand') }}
      </NuxtLink>

      <p class="mt-8 px-2 text-xs font-semibold uppercase tracking-wider text-primary-700 dark:text-primary-400">
        {{ t('sidebar.workspace') }}
      </p>
      <nav class="mt-2 flex flex-1 flex-col gap-1">
        <NuxtLink
          v-for="item in navItems"
          :key="item.to"
          :to="localePath(item.to)"
          class="flex items-center gap-3 rounded-xl px-3 py-2 text-sm font-medium transition-colors"
          :class="isActive(item.to)
            ? 'bg-primary-50 text-primary-800 dark:bg-primary-900/30 dark:text-primary-300'
            : 'text-slate-600 hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-slate-800'"
        >
          <NavIcon :name="item.icon" />
          {{ item.label }}
        </NuxtLink>
      </nav>

      <div class="flex flex-col gap-1 border-t border-slate-200/70 pt-4 dark:border-slate-800">
        <div class="flex items-center gap-1 px-2">
          <ClientOnly><NotificationBell /></ClientOnly>
          <ThemeToggle />
          <LocaleSwitcher />
        </div>
        <NuxtLink
          v-if="auth.isCandidate"
          :to="localePath('/dashboard/candidate/profile')"
          class="flex items-center gap-3 rounded-xl px-3 py-2 text-sm font-medium text-slate-600 hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-slate-800"
        >
          <NavIcon name="people" />
          {{ t('sidebar.profileSettings') }}
        </NuxtLink>
        <div class="mt-2 flex items-center gap-3 px-2 pt-2">
          <span class="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-primary-100 font-semibold text-primary-700 dark:bg-primary-900/40 dark:text-primary-300">
            {{ initials(auth.email) }}
          </span>
          <div class="min-w-0 flex-1">
            <p class="truncate text-sm font-medium text-slate-900 dark:text-slate-100">{{ auth.email }}</p>
          </div>
          <button type="button" class="rounded-lg p-1.5 text-slate-500 hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-slate-800" :aria-label="t('nav.logout')" @click="onLogout">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="h-5 w-5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9" />
            </svg>
          </button>
        </div>
      </div>
    </aside>

    <div class="flex flex-1 flex-col md:min-w-0">
      <div class="flex items-center justify-between border-b border-slate-200/70 bg-white px-4 py-3 dark:border-slate-800 dark:bg-slate-900 md:hidden">
        <NuxtLink :to="localePath('/')" class="flex items-center gap-2 font-display text-base font-bold tracking-tight">
          <span class="flex h-7 w-7 items-center justify-center rounded-lg bg-accent-400 text-slate-900">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" class="h-3.5 w-3.5"><rect x="7" y="7" width="10" height="10" rx="2" fill="none" stroke="currentColor" stroke-width="2" /></svg>
          </span>
          {{ t('common.brand') }}
        </NuxtLink>
        <button
          type="button"
          class="rounded-md p-1.5 text-slate-600 hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-slate-800"
          :aria-label="t('sidebar.menu')"
          :aria-expanded="mobileOpen"
          @click="mobileOpen = !mobileOpen"
        >
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="h-6 w-6">
            <path d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        </button>
      </div>

      <main class="flex-1 px-4 py-6 md:px-8 md:py-8">
        <slot />
      </main>
    </div>
  </div>
</template>
