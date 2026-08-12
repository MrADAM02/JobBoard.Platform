<script setup lang="ts">
import { JobTypeI18nKey } from '~/types/job'

definePageMeta({ layout: 'dashboard', middleware: 'auth', ssr: false })
useRequireRole('Candidate')

const route = useRoute()
const router = useRouter()
const { getMySavedJobs } = useSavedJobsApi()
const { t } = useI18n()
const localePath = useLocalePath()

const { data, refresh } = await useAsyncData(
  `saved-jobs-${route.query.page ?? 1}`,
  () => getMySavedJobs(route.query.page ? Number(route.query.page) : 1),
  { watch: [() => route.query] }
)

function goToPage(page: number) {
  router.push({ path: localePath('/dashboard/candidate/saved-jobs'), query: { page: String(page) } })
}

function onUnsaved() {
  refresh()
}

useSeoMeta({ title: () => t('dashboard.candidate.savedJobs.seoTitle') })
</script>

<template>
  <div class="flex flex-col gap-6">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('dashboard.candidate.savedJobs.title') }}</h1>

    <EmptyState v-if="!data?.items.length">
      {{ t('dashboard.candidate.savedJobs.empty') }}
      <template #action>
        <NuxtLink :to="localePath('/jobs')" class="text-sm text-primary-600 underline dark:text-primary-400">{{ t('dashboard.candidate.applications.browseOpenRoles') }}</NuxtLink>
      </template>
    </EmptyState>

    <ul v-else class="flex flex-col gap-3">
      <li v-for="job in data.items" :key="job.id">
        <Card :to="localePath(`/jobs/${job.id}`)" hover padding="sm">
          <div class="flex items-start justify-between gap-4">
            <div>
              <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ job.title }}</h2>
              <p class="text-sm text-slate-600 dark:text-slate-400">{{ job.companyName }} &middot; {{ job.location }}</p>
            </div>
            <div class="flex items-center gap-1">
              <Badge v-if="job.isRemote" variant="success">{{ t('jobs.detail.remote') }}</Badge>
              <BookmarkButton :job-id="job.id" :model-value="true" @update:model-value="onUnsaved" />
            </div>
          </div>
          <div class="mt-3 flex flex-wrap gap-2 text-xs">
            <Badge variant="neutral">{{ t(JobTypeI18nKey[job.jobType]) }}</Badge>
            <Badge v-if="job.salaryMin || job.salaryMax" variant="neutral">
              ${{ job.salaryMin?.toLocaleString() ?? '?' }} &ndash; ${{ job.salaryMax?.toLocaleString() ?? '?' }}
            </Badge>
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
