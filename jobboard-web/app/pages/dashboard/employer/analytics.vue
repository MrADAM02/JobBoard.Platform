<script setup lang="ts">
import { ApplicationStatus, ApplicationStatusI18nKey } from '~/types/application'

definePageMeta({ layout: 'dashboard', middleware: 'auth', ssr: false })
useRequireRole('Employer')

const { getMyAnalytics } = useCompaniesApi()
const { data: analytics, status } = await useAsyncData('employer-analytics', () => getMyAnalytics())
const { t } = useI18n()
const localePath = useLocalePath()

// Zero-filled in the enum's own fixed order, matching how the categorical
// chart colors are assigned - a status with no applications still gets a row.
const statusOrder = [
  ApplicationStatus.Applied, ApplicationStatus.UnderReview, ApplicationStatus.InterviewScheduled,
  ApplicationStatus.Rejected, ApplicationStatus.Offered, ApplicationStatus.Withdrawn
] as const

const statusChartData = computed(() => {
  const counts = new Map(analytics.value?.applicationsByStatus.map((s) => [s.status, s.count]) ?? [])
  return statusOrder.map((status) => ({
    label: t(ApplicationStatusI18nKey[status]),
    count: counts.get(status) ?? 0
  }))
})

useSeoMeta({ title: () => t('dashboard.employer.analytics.seoTitle') })
</script>

<template>
  <div class="flex flex-col gap-6">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('dashboard.employer.analytics.title') }}</h1>

    <div v-if="status === 'pending'" class="text-slate-500 dark:text-slate-400">{{ t('dashboard.employer.overview.loading') }}</div>

    <Alert v-else-if="!analytics" variant="warning">
      {{ t('dashboard.employer.jobsNew.setupCompanyFirst') }}
      <NuxtLink :to="localePath('/dashboard/employer/company')" class="underline">{{ t('dashboard.employer.jobsNew.companyProfileLink') }}</NuxtLink>
    </Alert>

    <template v-else>
      <div class="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Card>
          <p class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ analytics.totalViews }}</p>
          <p class="text-sm text-slate-600 dark:text-slate-400">{{ t('dashboard.employer.analytics.totalViews') }}</p>
        </Card>
        <Card>
          <p class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ analytics.totalApplications }}</p>
          <p class="text-sm text-slate-600 dark:text-slate-400">{{ t('dashboard.employer.analytics.totalApplications') }}</p>
        </Card>
        <Card>
          <p class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ analytics.totalJobsPosted }}</p>
          <p class="text-sm text-slate-600 dark:text-slate-400">{{ t('dashboard.employer.analytics.totalJobsPosted') }}</p>
        </Card>
      </div>

      <Card>
        <h2 class="mb-4 text-sm font-semibold uppercase tracking-wide text-slate-500 dark:text-slate-400">
          {{ t('dashboard.employer.analytics.viewsChartTitle') }}
        </h2>
        <ViewsLineChart :data="analytics.viewsByDay" />
      </Card>

      <Card>
        <h2 class="mb-4 text-sm font-semibold uppercase tracking-wide text-slate-500 dark:text-slate-400">
          {{ t('dashboard.employer.analytics.statusChartTitle') }}
        </h2>
        <ApplicationStatusChart :data="statusChartData" />
      </Card>

      <Card v-if="analytics.topJobs.length">
        <h2 class="mb-4 text-sm font-semibold uppercase tracking-wide text-slate-500 dark:text-slate-400">
          {{ t('dashboard.employer.analytics.topJobsTitle') }}
        </h2>
        <ul class="flex flex-col gap-3">
          <li v-for="job in analytics.topJobs" :key="job.id" class="flex items-center justify-between gap-4 text-sm">
            <NuxtLink :to="localePath(`/jobs/${job.id}`)" class="text-primary-600 underline dark:text-primary-400">{{ job.title }}</NuxtLink>
            <span class="whitespace-nowrap text-slate-600 dark:text-slate-400">
              {{ job.viewCount }} {{ t('dashboard.employer.analytics.views') }} &middot; {{ job.applicationCount }} {{ t('admin.jobs.applicants') }}
            </span>
          </li>
        </ul>
      </Card>
    </template>
  </div>
</template>
