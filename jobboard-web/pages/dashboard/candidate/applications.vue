<script setup lang="ts">
import { ApplicationStatus, ApplicationStatusI18nKey } from '~/types/application'
import type { ApplicationStatusValue } from '~/types/application'

definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Candidate')

const { getMyApplications } = useApplicationsApi()
const { data: applications } = await useAsyncData('my-applications', () => getMyApplications())
const { t } = useI18n()
const localePath = useLocalePath()

function statusVariant(status: ApplicationStatusValue): 'success' | 'warning' | 'neutral' | 'info' | 'danger' {
  if (status === ApplicationStatus.Offered) return 'success'
  if (status === ApplicationStatus.InterviewScheduled) return 'warning'
  if (status === ApplicationStatus.UnderReview) return 'info'
  if (status === ApplicationStatus.Rejected) return 'danger'
  return 'neutral'
}

useSeoMeta({ title: () => t('dashboard.candidate.applications.seoTitle') })
</script>

<template>
  <div class="flex flex-col gap-6">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('dashboard.candidate.applications.title') }}</h1>

    <EmptyState v-if="!applications?.length">
      {{ t('dashboard.candidate.applications.empty') }}
      <template #action>
        <NuxtLink :to="localePath('/jobs')" class="text-sm text-primary-600 underline dark:text-primary-400">{{ t('dashboard.candidate.applications.browseOpenRoles') }}</NuxtLink>
      </template>
    </EmptyState>

    <ul v-else class="flex flex-col gap-3">
      <li v-for="app in applications" :key="app.id">
        <Card padding="sm">
          <div class="flex flex-col items-start gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <NuxtLink :to="localePath(`/jobs/${app.jobListingId}`)" class="font-semibold text-slate-900 hover:underline dark:text-slate-100">
                {{ app.jobTitle }}
              </NuxtLink>
              <p class="text-sm text-slate-600 dark:text-slate-400">{{ app.companyName }} &middot; {{ t('dashboard.candidate.applications.appliedOn', { date: new Date(app.appliedAt).toLocaleDateString() }) }}</p>
            </div>
            <Badge :variant="statusVariant(app.status)">{{ t(ApplicationStatusI18nKey[app.status]) }}</Badge>
          </div>
        </Card>
      </li>
    </ul>
  </div>
</template>
