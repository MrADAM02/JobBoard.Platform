<script setup lang="ts">
import type { JobListingFilters, JobTypeValue } from '~/types/job'
import { JobTypeLabels } from '~/types/job'

const route = useRoute()
const router = useRouter()
const { getJobListings } = useJobsApi()

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
    path: '/jobs',
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
  router.push({ path: '/jobs', query: { ...route.query, page: String(page) } })
}

useSeoMeta({
  title: 'Browse Jobs — JobBoard',
  description: 'Search open job listings by keyword, location, job type, and salary.'
})
</script>

<template>
  <div class="flex flex-col gap-8">
    <div>
      <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">Browse Jobs</h1>
      <p class="mt-1 text-sm text-slate-600 dark:text-slate-400">
        {{ data?.totalCount ?? 0 }} open position{{ data?.totalCount === 1 ? '' : 's' }}
      </p>
    </div>

    <form
      class="grid grid-cols-1 gap-3 rounded-lg border border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-900 sm:grid-cols-2 lg:grid-cols-5"
      @submit.prevent="applyFilters"
    >
      <input
        v-model="keyword"
        type="text"
        placeholder="Keyword (title or description)"
        class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100 dark:placeholder:text-slate-500 lg:col-span-2"
      >
      <input
        v-model="location"
        type="text"
        placeholder="Location"
        class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100 dark:placeholder:text-slate-500"
      >
      <select v-model="jobType" class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100">
        <option value="">Any job type</option>
        <option v-for="(label, value) in JobTypeLabels" :key="value" :value="value">
          {{ label }}
        </option>
      </select>
      <input
        v-model="minSalary"
        type="number"
        placeholder="Min salary"
        class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100 dark:placeholder:text-slate-500"
      >
      <label class="flex items-center gap-2 text-sm text-slate-700 dark:text-slate-300 sm:col-span-2 lg:col-span-1">
        <input v-model="remoteOnly" type="checkbox" class="h-4 w-4 rounded border-slate-300 dark:border-slate-700 dark:bg-slate-900">
        Remote only
      </label>
      <button
        type="submit"
        class="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-300 sm:col-span-2 lg:col-span-1"
      >
        Search
      </button>
    </form>

    <div v-if="status === 'pending'" class="py-16 text-center text-slate-500 dark:text-slate-400">
      Loading jobs&hellip;
    </div>

    <div v-else-if="error" class="rounded-lg border border-red-200 bg-red-50 p-6 text-center text-red-700 dark:border-red-900 dark:bg-red-950/40 dark:text-red-300">
      Something went wrong loading job listings. Please try again.
    </div>

    <div v-else-if="!data?.items.length" class="rounded-lg border border-slate-200 bg-white p-10 text-center text-slate-500 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-400">
      No jobs match your search.
    </div>

    <ul v-else class="flex flex-col gap-3">
      <li v-for="job in data.items" :key="job.id">
        <NuxtLink
          :to="`/jobs/${job.id}`"
          class="block rounded-lg border border-slate-200 bg-white p-5 transition hover:border-slate-400 hover:shadow-sm dark:border-slate-700 dark:bg-slate-900 dark:hover:border-slate-500"
        >
          <div class="flex items-start justify-between gap-4">
            <div>
              <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ job.title }}</h2>
              <p class="text-sm text-slate-600 dark:text-slate-400">{{ job.companyName }} &middot; {{ job.location }}</p>
            </div>
            <span
              v-if="job.isRemote"
              class="whitespace-nowrap rounded-full bg-emerald-100 px-2.5 py-1 text-xs font-medium text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300"
            >
              Remote
            </span>
          </div>
          <div class="mt-3 flex flex-wrap gap-2 text-xs text-slate-500 dark:text-slate-400">
            <span class="rounded-full bg-slate-100 px-2.5 py-1 dark:bg-slate-800">{{ JobTypeLabels[job.jobType] }}</span>
            <span v-if="job.salaryMin || job.salaryMax" class="rounded-full bg-slate-100 px-2.5 py-1 dark:bg-slate-800">
              ${{ job.salaryMin?.toLocaleString() ?? '?' }} &ndash; ${{ job.salaryMax?.toLocaleString() ?? '?' }}
            </span>
          </div>
        </NuxtLink>
      </li>
    </ul>

    <div v-if="data && data.totalPages > 1" class="flex items-center justify-center gap-3 pt-2">
      <button
        :disabled="!data.hasPreviousPage"
        class="rounded-md border border-slate-300 px-3 py-1.5 text-sm disabled:opacity-40 dark:border-slate-700"
        @click="goToPage(data.pageNumber - 1)"
      >
        Previous
      </button>
      <span class="text-sm text-slate-600 dark:text-slate-400">Page {{ data.pageNumber }} of {{ data.totalPages }}</span>
      <button
        :disabled="!data.hasNextPage"
        class="rounded-md border border-slate-300 px-3 py-1.5 text-sm disabled:opacity-40 dark:border-slate-700"
        @click="goToPage(data.pageNumber + 1)"
      >
        Next
      </button>
    </div>
  </div>
</template>
