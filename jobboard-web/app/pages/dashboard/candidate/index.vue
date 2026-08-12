<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: 'auth', ssr: false })
useRequireRole('Candidate')
const { t } = useI18n()
const localePath = useLocalePath()

useSeoMeta({ title: () => t('dashboard.candidate.overview.seoTitle') })
</script>

<template>
  <div class="flex flex-col gap-4">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('dashboard.candidate.overview.title') }}</h1>

    <Card :to="localePath('/dashboard/candidate/applications')" hover>
      <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ t('dashboard.candidate.overview.applicationsTitle') }}</h2>
      <p class="text-sm text-slate-600 dark:text-slate-400">{{ t('dashboard.candidate.overview.applicationsDesc') }}</p>
    </Card>

    <Card :to="localePath('/dashboard/candidate/saved-jobs')" hover>
      <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ t('dashboard.candidate.overview.savedJobsTitle') }}</h2>
      <p class="text-sm text-slate-600 dark:text-slate-400">{{ t('dashboard.candidate.overview.savedJobsDesc') }}</p>
    </Card>

    <Card :to="localePath('/dashboard/candidate/profile')" hover>
      <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ t('dashboard.candidate.overview.profileTitle') }}</h2>
      <p class="text-sm text-slate-600 dark:text-slate-400">{{ t('dashboard.candidate.overview.profileDesc') }}</p>
    </Card>

    <Card :to="localePath('/jobs')" hover>
      <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ t('dashboard.candidate.overview.browseJobsTitle') }}</h2>
      <p class="text-sm text-slate-600 dark:text-slate-400">{{ t('dashboard.candidate.overview.browseJobsDesc') }}</p>
    </Card>
  </div>
</template>
