<script setup lang="ts">
import { ApplicationStatusI18nKey } from '~/types/application'

definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Candidate')

const { getMyApplications } = useApplicationsApi()
const { data: applications } = await useAsyncData('my-applications', () => getMyApplications())
const { t } = useI18n()

useSeoMeta({ title: () => t('dashboard.candidate.applications.seoTitle') })
</script>

<template>
  <div class="flex flex-col gap-6">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('dashboard.candidate.applications.title') }}</h1>

    <div v-if="!applications?.length" class="rounded-lg border border-slate-200 bg-white p-10 text-center text-slate-500 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-400">
      {{ t('dashboard.candidate.applications.empty') }}
      <NuxtLink to="/jobs" class="block underline">{{ t('dashboard.candidate.applications.browseOpenRoles') }}</NuxtLink>
    </div>

    <ul v-else class="flex flex-col gap-3">
      <li v-for="app in applications" :key="app.id" class="rounded-lg border border-slate-200 bg-white p-5 dark:border-slate-700 dark:bg-slate-900">
        <div class="flex items-center justify-between gap-4">
          <div>
            <NuxtLink :to="`/jobs/${app.jobListingId}`" class="font-semibold text-slate-900 hover:underline dark:text-slate-100">
              {{ app.jobTitle }}
            </NuxtLink>
            <p class="text-sm text-slate-600 dark:text-slate-400">{{ app.companyName }} &middot; {{ t('dashboard.candidate.applications.appliedOn', { date: new Date(app.appliedAt).toLocaleDateString() }) }}</p>
          </div>
          <span class="whitespace-nowrap rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-700 dark:bg-slate-800 dark:text-slate-300">
            {{ t(ApplicationStatusI18nKey[app.status]) }}
          </span>
        </div>
      </li>
    </ul>
  </div>
</template>
