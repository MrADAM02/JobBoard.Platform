<script setup lang="ts">
definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Candidate')
const { t } = useI18n()

useSeoMeta({ title: () => t('dashboard.candidate.overview.seoTitle') })
</script>

<template>
  <div class="flex flex-col gap-4">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('dashboard.candidate.overview.title') }}</h1>

    <NuxtLink
      to="/dashboard/candidate/applications"
      class="rounded-lg border border-slate-200 bg-white p-6 hover:border-slate-400 dark:border-slate-700 dark:bg-slate-900 dark:hover:border-slate-500"
    >
      <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ t('dashboard.candidate.overview.applicationsTitle') }}</h2>
      <p class="text-sm text-slate-600 dark:text-slate-400">{{ t('dashboard.candidate.overview.applicationsDesc') }}</p>
    </NuxtLink>

    <NuxtLink
      to="/dashboard/candidate/profile"
      class="rounded-lg border border-slate-200 bg-white p-6 hover:border-slate-400 dark:border-slate-700 dark:bg-slate-900 dark:hover:border-slate-500"
    >
      <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ t('dashboard.candidate.overview.profileTitle') }}</h2>
      <p class="text-sm text-slate-600 dark:text-slate-400">{{ t('dashboard.candidate.overview.profileDesc') }}</p>
    </NuxtLink>

    <NuxtLink
      to="/jobs"
      class="rounded-lg border border-slate-200 bg-white p-6 hover:border-slate-400 dark:border-slate-700 dark:bg-slate-900 dark:hover:border-slate-500"
    >
      <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ t('dashboard.candidate.overview.browseJobsTitle') }}</h2>
      <p class="text-sm text-slate-600 dark:text-slate-400">{{ t('dashboard.candidate.overview.browseJobsDesc') }}</p>
    </NuxtLink>
  </div>
</template>
