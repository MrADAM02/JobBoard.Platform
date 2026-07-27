<script setup lang="ts">
import { JobStatus, JobStatusI18nKey } from '~/types/job'

definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Employer')

const { getMyJobListings, closeJobListing, publishJobListing, deleteJobListing } = useJobsApi()
const { t } = useI18n()
const localePath = useLocalePath()
const toast = useToast()

const { data, refresh } = await useAsyncData('my-jobs', () => getMyJobListings())

// Each action used to just re-fetch the list with no confirmation - a slow or
// dropped request looked identical to success. Now every action reports back.
async function onClose(id: string) {
  try {
    await closeJobListing(id)
    await refresh()
    toast.success(t('dashboard.employer.jobsList.closeSuccess'))
  } catch {
    toast.error(t('dashboard.employer.jobsList.actionError'))
  }
}

async function onPublish(id: string) {
  try {
    await publishJobListing(id)
    await refresh()
    toast.success(t('dashboard.employer.jobsList.publishSuccess'))
  } catch {
    toast.error(t('dashboard.employer.jobsList.actionError'))
  }
}

async function onDelete(id: string) {
  if (!confirm(t('dashboard.employer.jobsList.deleteConfirm'))) return
  try {
    await deleteJobListing(id)
    await refresh()
    toast.success(t('dashboard.employer.jobsList.deleteSuccess'))
  } catch {
    toast.error(t('dashboard.employer.jobsList.actionError'))
  }
}

function statusVariant(status: number): 'success' | 'neutral' | 'warning' {
  if (status === JobStatus.Published) return 'success'
  if (status === JobStatus.Closed) return 'neutral'
  return 'warning'
}

useSeoMeta({ title: () => t('dashboard.employer.jobsList.seoTitle') })
</script>

<template>
  <div class="flex flex-col gap-6">
    <div class="flex items-center justify-between">
      <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('dashboard.employer.jobsList.title') }}</h1>
      <BaseButton :to="localePath('/dashboard/employer/jobs/new')">
        {{ t('dashboard.employer.jobsList.postJob') }}
      </BaseButton>
    </div>

    <EmptyState v-if="!data?.items.length">{{ t('dashboard.employer.jobsList.empty') }}</EmptyState>

    <ul v-else class="flex flex-col gap-3">
      <li v-for="job in data.items" :key="job.id">
        <Card padding="sm">
          <div class="flex items-start justify-between gap-4">
            <div>
              <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ job.title }}</h2>
              <p class="text-sm text-slate-600 dark:text-slate-400">
                {{ t('dashboard.employer.jobsList.jobStats', {
                  location: job.location,
                  applicants: t('dashboard.employer.jobsList.applicantCount', { count: job.applicationCount }, job.applicationCount),
                  views: job.viewCount
                }) }}
              </p>
            </div>
            <Badge :variant="statusVariant(job.status)">{{ t(JobStatusI18nKey[job.status]) }}</Badge>
          </div>
          <div class="mt-4 flex flex-wrap gap-3 text-sm">
            <NuxtLink :to="localePath(`/dashboard/employer/jobs/${job.id}/applicants`)" class="text-primary-600 underline dark:text-primary-400">
              {{ t('dashboard.employer.jobsList.viewApplicants') }}
            </NuxtLink>
            <NuxtLink :to="localePath(`/dashboard/employer/jobs/${job.id}/edit`)" class="text-primary-600 underline dark:text-primary-400">
              {{ t('dashboard.employer.jobsList.edit') }}
            </NuxtLink>
            <button
              v-if="job.status === JobStatus.Draft"
              class="text-emerald-700 underline dark:text-emerald-400"
              @click="onPublish(job.id)"
            >
              {{ t('dashboard.employer.jobsList.publish') }}
            </button>
            <button
              v-if="job.status === JobStatus.Published"
              class="text-slate-700 underline dark:text-slate-300"
              @click="onClose(job.id)"
            >
              {{ t('dashboard.employer.jobsList.close') }}
            </button>
            <button class="text-red-600 underline dark:text-red-400" @click="onDelete(job.id)">
              {{ t('dashboard.employer.jobsList.delete') }}
            </button>
          </div>
        </Card>
      </li>
    </ul>
  </div>
</template>
