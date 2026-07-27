<script setup lang="ts">
import { ApplicationStatusI18nKey } from '~/types/application'

definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Employer')

const route = useRoute()
const jobId = route.params.id as string
const { t } = useI18n()
const localePath = useLocalePath()

const { getApplicationsForJob, updateApplicationStatus } = useApplicationsApi()
const { data: applicants, refresh } = await useAsyncData(`applicants-${jobId}`, () => getApplicationsForJob(jobId))
const toast = useToast()

const updating = ref<string | null>(null)

async function onStatusChange(applicationId: string, newStatus: number) {
  updating.value = applicationId
  try {
    await updateApplicationStatus(applicationId, newStatus as never)
    await refresh()
    toast.success(t('dashboard.employer.applicants.statusUpdated'))
  } catch {
    toast.error(t('dashboard.employer.applicants.statusError'))
  } finally {
    updating.value = null
  }
}

useSeoMeta({ title: () => t('dashboard.employer.applicants.seoTitle') })
</script>

<template>
  <div class="flex flex-col gap-6">
    <NuxtLink :to="localePath('/dashboard/employer/jobs')" class="text-sm text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200">
      <span class="inline-block rtl:-scale-x-100">&larr;</span> {{ t('dashboard.employer.applicants.backToListings') }}
    </NuxtLink>
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('dashboard.employer.applicants.title') }}</h1>

    <EmptyState v-if="!applicants?.length">{{ t('dashboard.employer.applicants.empty') }}</EmptyState>

    <ul v-else class="flex flex-col gap-3">
      <li v-for="app in applicants" :key="app.id">
        <Card padding="sm">
          <div class="flex flex-col items-start gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ app.candidateName }}</h2>
              <p class="text-sm text-slate-600 dark:text-slate-400">
                {{ t('dashboard.employer.applicants.appliedOn', { date: new Date(app.appliedAt).toLocaleDateString() }) }}
                <template v-if="app.resumeUrl">
                  &middot; <a :href="app.resumeUrl" target="_blank" rel="noopener" class="text-primary-600 underline dark:text-primary-400">{{ t('dashboard.employer.applicants.resume') }}</a>
                </template>
              </p>
            </div>
            <select
              :value="app.status"
              :disabled="updating === app.id"
              class="w-full rounded-lg border border-slate-300 px-2 py-1.5 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100 sm:w-auto"
              @change="onStatusChange(app.id, Number(($event.target as HTMLSelectElement).value))"
            >
              <option v-for="(i18nKey, value) in ApplicationStatusI18nKey" :key="value" :value="Number(value)">
                {{ t(i18nKey) }}
              </option>
            </select>
          </div>
        </Card>
      </li>
    </ul>
  </div>
</template>
