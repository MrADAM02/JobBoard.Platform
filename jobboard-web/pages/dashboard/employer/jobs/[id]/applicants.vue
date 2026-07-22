<script setup lang="ts">
import { ApplicationStatusLabels } from '~/types/application'

definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Employer')

const route = useRoute()
const jobId = route.params.id as string

const { getApplicationsForJob, updateApplicationStatus } = useApplicationsApi()
const { data: applicants, refresh } = await useAsyncData(`applicants-${jobId}`, () => getApplicationsForJob(jobId))

const updating = ref<string | null>(null)

async function onStatusChange(applicationId: string, newStatus: number) {
  updating.value = applicationId
  try {
    await updateApplicationStatus(applicationId, newStatus as never)
    await refresh()
  } finally {
    updating.value = null
  }
}

useSeoMeta({ title: 'Applicants — JobBoard' })
</script>

<template>
  <div class="flex flex-col gap-6">
    <NuxtLink to="/dashboard/employer/jobs" class="text-sm text-slate-500 hover:text-slate-700">
      &larr; Back to my listings
    </NuxtLink>
    <h1 class="text-2xl font-bold text-slate-900">Applicants</h1>

    <div v-if="!applicants?.length" class="rounded-lg border border-slate-200 bg-white p-10 text-center text-slate-500">
      No applications yet.
    </div>

    <ul v-else class="flex flex-col gap-3">
      <li v-for="app in applicants" :key="app.id" class="rounded-lg border border-slate-200 bg-white p-5">
        <div class="flex items-center justify-between gap-4">
          <div>
            <h2 class="font-semibold text-slate-900">{{ app.candidateName }}</h2>
            <p class="text-sm text-slate-600">
              Applied {{ new Date(app.appliedAt).toLocaleDateString() }}
              <template v-if="app.resumeUrl">
                &middot; <a :href="app.resumeUrl" target="_blank" rel="noopener" class="underline">Resume</a>
              </template>
            </p>
          </div>
          <select
            :value="app.status"
            :disabled="updating === app.id"
            class="rounded-md border border-slate-300 px-2 py-1.5 text-sm"
            @change="onStatusChange(app.id, Number(($event.target as HTMLSelectElement).value))"
          >
            <option v-for="(label, value) in ApplicationStatusLabels" :key="value" :value="Number(value)">
              {{ label }}
            </option>
          </select>
        </div>
      </li>
    </ul>
  </div>
</template>
