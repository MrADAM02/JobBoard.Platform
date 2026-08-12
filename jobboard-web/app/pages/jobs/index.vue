<script setup lang="ts">
import type { JobListingFilters, JobTypeValue } from '~/types/job'
import { JobTypeI18nKey } from '~/types/job'

const route = useRoute()
const router = useRouter()
const { getJobListings, getJobLocations } = useJobsApi()
const { getMySavedJobIds } = useSavedJobsApi()
const { t } = useI18n()
const localePath = useLocalePath()
const auth = useAuthStore()

// Client-only, matches how ApplyToJobBox checks "already applied" - a
// crawler rendering this page server-side has no auth state anyway.
const savedIds = ref<string[]>([])
onMounted(async () => {
  if (!auth.isCandidate) return
  try {
    savedIds.value = await getMySavedJobIds()
  } catch {
    // non-fatal - bookmark buttons just default to "not saved"
  }
})

function onToggleSaved(jobId: string, saved: boolean) {
  savedIds.value = saved ? [...savedIds.value, jobId] : savedIds.value.filter((id) => id !== jobId)
}

// Populates the location filter as a select - a free-text box let candidates
// type a location that matches nothing (typos, a city with no open jobs).
// Public, so it renders correctly in the SSR HTML too.
const { data: locations } = await useAsyncData('job-locations', () => getJobLocations())

// Filters are read from - and written back to - the URL query string, not
// component-local state, so a direct request to a filtered URL (what a search
// crawler or a shared link hits) renders the same server-side HTML as an
// in-app navigation. No client-only filter state that a crawler would miss.
const keyword = ref((route.query.keyword as string) || '')
const location = ref((route.query.location as string) || '')
const jobType = ref<string>((route.query.jobType as string) || '')
const remoteOnly = ref(route.query.remoteOnly === 'true')
const minSalary = ref((route.query.minSalary as string) || '')

function currentFilters(): JobListingFilters {
  return {
    keyword: (route.query.keyword as string) || undefined,
    location: (route.query.location as string) || undefined,
    jobType: route.query.jobType ? (Number(route.query.jobType) as JobTypeValue) : undefined,
    remoteOnly: route.query.remoteOnly === 'true' || undefined,
    minSalary: route.query.minSalary ? Number(route.query.minSalary) : undefined,
    pageNumber: route.query.page ? Number(route.query.page) : 1
  }
}

const { data, status, error } = await useAsyncData(
  () => `jobs-${JSON.stringify(route.query)}`,
  () => getJobListings(currentFilters()),
  { watch: [() => route.query] }
)

function applyFilters() {
  router.push({
    path: localePath('/jobs'),
    query: {
      keyword: keyword.value || undefined,
      location: location.value || undefined,
      jobType: jobType.value || undefined,
      remoteOnly: remoteOnly.value ? 'true' : undefined,
      minSalary: minSalary.value || undefined
    }
  })
}

function goToPage(page: number) {
  router.push({ path: localePath('/jobs'), query: { ...route.query, page: String(page) } })
}

useSeoMeta({
  title: () => t('jobs.list.seoTitle'),
  description: () => t('jobs.list.seoDescription')
})
</script>

<template>
  <div class="flex flex-col gap-8">
    <div>
      <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('jobs.list.heading') }}</h1>
      <p class="mt-1 text-sm text-slate-600 dark:text-slate-400">
        {{ t('jobs.list.resultCount', { count: data?.totalCount ?? 0 }, data?.totalCount ?? 0) }}
      </p>
    </div>

    <form
      class="grid grid-cols-1 gap-3 rounded-xl border border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-900 sm:grid-cols-2 lg:grid-cols-5"
      @submit.prevent="applyFilters"
    >
      <input
        v-model="keyword"
        type="text"
        :placeholder="t('jobs.list.keywordPlaceholder')"
        class="rounded-lg border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100 dark:placeholder:text-slate-500 lg:col-span-2"
      >
      <select
        v-model="location"
        class="rounded-lg border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100"
      >
        <option value="">{{ t('jobs.list.anyLocation') }}</option>
        <option v-for="loc in locations" :key="loc" :value="loc">{{ loc }}</option>
      </select>
      <select v-model="jobType" class="rounded-lg border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100">
        <option value="">{{ t('jobs.list.anyJobType') }}</option>
        <option v-for="(i18nKey, value) in JobTypeI18nKey" :key="value" :value="value">
          {{ t(i18nKey) }}
        </option>
      </select>
      <input
        v-model="minSalary"
        type="number"
        :placeholder="t('jobs.list.minSalaryPlaceholder')"
        class="rounded-lg border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100 dark:placeholder:text-slate-500"
      >
      <label class="flex items-center gap-2 text-sm text-slate-700 dark:text-slate-300 sm:col-span-2 lg:col-span-1">
        <input v-model="remoteOnly" type="checkbox" class="h-4 w-4 rounded border-slate-300 text-primary-600 dark:border-slate-700 dark:bg-slate-900">
        {{ t('jobs.list.remoteOnly') }}
      </label>
      <BaseButton type="submit" class="justify-center sm:col-span-2 lg:col-span-1">
        {{ t('jobs.list.search') }}
      </BaseButton>
    </form>

    <div v-if="status === 'pending'" class="py-16 text-center text-slate-500 dark:text-slate-400">
      {{ t('jobs.list.loading') }}
    </div>

    <Alert v-else-if="error">{{ t('jobs.list.loadError') }}</Alert>

    <EmptyState v-else-if="!data?.items.length">{{ t('jobs.list.empty') }}</EmptyState>

    <ul v-else class="grid grid-cols-1 gap-3 sm:grid-cols-2">
      <li v-for="job in data.items" :key="job.id">
        <JobCard
          :job="job"
          :saved="savedIds.includes(job.id)"
          :show-bookmark="auth.isCandidate"
          @update:saved="(v) => onToggleSaved(job.id, v)"
        />
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
