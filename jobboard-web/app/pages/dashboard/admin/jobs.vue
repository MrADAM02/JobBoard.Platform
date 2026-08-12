<script setup lang="ts">
import { JobStatus, JobStatusI18nKey } from '~/types/job'
import type { JobStatusValue } from '~/types/job'

definePageMeta({ layout: 'dashboard', middleware: 'auth', ssr: false })
useRequireRole('Admin')

const route = useRoute()
const router = useRouter()
const { getJobs, deleteJob } = useAdminApi()
const { t } = useI18n()
const localePath = useLocalePath()
const toast = useToast()

const statusFilter = ref<string>((route.query.status as string) || '')

const { data, refresh } = await useAsyncData(
  () => `admin-jobs-${JSON.stringify(route.query)}`,
  () => getJobs(
    route.query.status ? (Number(route.query.status) as JobStatusValue) : undefined,
    route.query.page ? Number(route.query.page) : 1
  ),
  { watch: [() => route.query] }
)

function applyFilter() {
  router.push({ path: localePath('/dashboard/admin/jobs'), query: { status: statusFilter.value || undefined } })
}

function goToPage(page: number) {
  router.push({ path: localePath('/dashboard/admin/jobs'), query: { ...route.query, page: String(page) } })
}

async function onDelete(jobId: string) {
  if (!confirm(t('admin.jobs.deleteConfirm'))) return
  try {
    await deleteJob(jobId)
    await refresh()
    toast.success(t('admin.jobs.deleteSuccess'))
  } catch {
    toast.error(t('admin.jobs.deleteError'))
  }
}

function statusVariant(status: number): 'success' | 'neutral' | 'warning' {
  if (status === JobStatus.Published) return 'success'
  if (status === JobStatus.Closed || status === JobStatus.Expired) return 'neutral'
  return 'warning'
}

useSeoMeta({ title: () => t('admin.jobs.seoTitle') })
</script>

<template>
  <div class="flex flex-col gap-6">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('admin.jobs.title') }}</h1>

    <form class="flex gap-3" @submit.prevent="applyFilter">
      <select v-model="statusFilter" class="rounded-lg border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100">
        <option value="">{{ t('admin.jobs.anyStatus') }}</option>
        <option v-for="(i18nKey, value) in JobStatusI18nKey" :key="value" :value="value">{{ t(i18nKey) }}</option>
      </select>
      <BaseButton type="submit">{{ t('jobs.list.search') }}</BaseButton>
    </form>

    <EmptyState v-if="!data?.items.length">{{ t('dashboard.employer.jobsList.empty') }}</EmptyState>

    <ul v-else class="flex flex-col gap-3">
      <li v-for="job in data.items" :key="job.id">
        <Card padding="sm">
          <div class="flex items-start justify-between gap-4">
            <div>
              <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ job.title }}</h2>
              <p class="text-sm text-slate-600 dark:text-slate-400">
                {{ job.companyName }} &middot; {{ t('admin.jobs.views') }}: {{ job.viewCount }} &middot; {{ t('admin.jobs.applicants') }}: {{ job.applicationCount }}
              </p>
            </div>
            <Badge :variant="statusVariant(job.status)">{{ t(JobStatusI18nKey[job.status]) }}</Badge>
          </div>
          <div class="mt-4">
            <button class="text-sm text-red-600 underline dark:text-red-400" @click="onDelete(job.id)">
              {{ t('admin.jobs.delete') }}
            </button>
          </div>
        </Card>
      </li>
    </ul>

    <Pagination
      v-if="data && data.totalPages > 1"
      :page-number="data.pageNumber"
      :total-pages="data.totalPages"
      :has-previous-page="data.hasPreviousPage"
      :has-next-page="data.hasNextPage"
      @change="goToPage"
    />
  </div>
</template>
