<script setup lang="ts">
import { JobStatus, JobStatusLabels } from '~/types/job'

definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Employer')

const { getMyJobListings, closeJobListing, deleteJobListing } = useJobsApi()

const { data, refresh } = await useAsyncData('my-jobs', () => getMyJobListings())

async function onClose(id: string) {
  await closeJobListing(id)
  await refresh()
}

async function onDelete(id: string) {
  if (!confirm('Delete this listing? This cannot be undone.')) return
  await deleteJobListing(id)
  await refresh()
}

function statusBadgeClass(status: number) {
  if (status === JobStatus.Published) return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300'
  if (status === JobStatus.Closed) return 'bg-slate-200 text-slate-600 dark:bg-slate-700 dark:text-slate-300'
  return 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300'
}

useSeoMeta({ title: 'My Job Listings — JobBoard' })
</script>

<template>
  <div class="flex flex-col gap-6">
    <div class="flex items-center justify-between">
      <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">My Job Listings</h1>
      <NuxtLink
        to="/dashboard/employer/jobs/new"
        class="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-300"
      >
        Post a job
      </NuxtLink>
    </div>

    <div v-if="!data?.items.length" class="rounded-lg border border-slate-200 bg-white p-10 text-center text-slate-500 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-400">
      You haven't posted any jobs yet.
    </div>

    <ul v-else class="flex flex-col gap-3">
      <li v-for="job in data.items" :key="job.id" class="rounded-lg border border-slate-200 bg-white p-5 dark:border-slate-700 dark:bg-slate-900">
        <div class="flex items-start justify-between gap-4">
          <div>
            <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ job.title }}</h2>
            <p class="text-sm text-slate-600 dark:text-slate-400">{{ job.location }} &middot; {{ job.applicationCount }} applicant{{ job.applicationCount === 1 ? '' : 's' }} &middot; {{ job.viewCount }} views</p>
          </div>
          <span class="whitespace-nowrap rounded-full px-2.5 py-1 text-xs font-medium" :class="statusBadgeClass(job.status)">
            {{ JobStatusLabels[job.status] }}
          </span>
        </div>
        <div class="mt-4 flex flex-wrap gap-3 text-sm">
          <NuxtLink :to="`/dashboard/employer/jobs/${job.id}/applicants`" class="text-slate-700 underline dark:text-slate-300">
            View applicants
          </NuxtLink>
          <NuxtLink :to="`/dashboard/employer/jobs/${job.id}/edit`" class="text-slate-700 underline dark:text-slate-300">
            Edit
          </NuxtLink>
          <button
            v-if="job.status === JobStatus.Published"
            class="text-slate-700 underline dark:text-slate-300"
            @click="onClose(job.id)"
          >
            Close
          </button>
          <button class="text-red-600 underline dark:text-red-400" @click="onDelete(job.id)">
            Delete
          </button>
        </div>
      </li>
    </ul>
  </div>
</template>
