<script setup lang="ts">
definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Admin')

const { getStats } = useAdminApi()
const { data: stats, status } = await useAsyncData('admin-stats', () => getStats())
const { t } = useI18n()
const localePath = useLocalePath()

const tiles = [
  'totalUsers', 'totalCandidates', 'totalEmployers',
  'totalCompanies', 'totalJobListings', 'totalApplications'
] as const

useSeoMeta({ title: () => t('admin.overview.seoTitle') })
</script>

<template>
  <div class="flex flex-col gap-6">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('admin.overview.title') }}</h1>

    <div v-if="status === 'pending'" class="text-slate-500 dark:text-slate-400">{{ t('dashboard.employer.overview.loading') }}</div>

    <div v-else-if="stats" class="grid grid-cols-2 gap-4 sm:grid-cols-3">
      <Card v-for="tile in tiles" :key="tile">
        <p class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ stats[tile] }}</p>
        <p class="text-sm text-slate-600 dark:text-slate-400">{{ t(`admin.overview.${tile}`) }}</p>
      </Card>
    </div>

    <div class="flex flex-col gap-4">
      <Card :to="localePath('/dashboard/admin/users')" hover>
        <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ t('admin.overview.manageUsers') }}</h2>
        <p class="text-sm text-slate-600 dark:text-slate-400">{{ t('admin.overview.manageUsersDesc') }}</p>
      </Card>

      <Card :to="localePath('/dashboard/admin/jobs')" hover>
        <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ t('admin.overview.manageJobs') }}</h2>
        <p class="text-sm text-slate-600 dark:text-slate-400">{{ t('admin.overview.manageJobsDesc') }}</p>
      </Card>
    </div>
  </div>
</template>
