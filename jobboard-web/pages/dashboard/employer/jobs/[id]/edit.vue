<script setup lang="ts">
import { JobTypeLabels } from '~/types/job'
import type { JobTypeValue } from '~/types/job'

definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Employer')

const route = useRoute()
const router = useRouter()
const jobId = route.params.id as string

const { getJobListingById, updateJobListing } = useJobsApi()
const { data: job } = await useAsyncData(`edit-job-${jobId}`, () => getJobListingById(jobId))

const title = ref(job.value?.title ?? '')
const description = ref(job.value?.description ?? '')
const location = ref(job.value?.location ?? '')
const isRemote = ref(job.value?.isRemote ?? false)
const salaryMin = ref(job.value?.salaryMin?.toString() ?? '')
const salaryMax = ref(job.value?.salaryMax?.toString() ?? '')
const jobType = ref<JobTypeValue>(job.value?.jobType ?? 0)
const tags = ref(job.value?.tags ?? '')
const error = ref<string | null>(null)
const submitting = ref(false)

async function onSubmit() {
  error.value = null
  submitting.value = true
  try {
    await updateJobListing(jobId, {
      title: title.value,
      description: description.value,
      location: location.value,
      isRemote: isRemote.value,
      salaryMin: salaryMin.value ? Number(salaryMin.value) : null,
      salaryMax: salaryMax.value ? Number(salaryMax.value) : null,
      jobType: jobType.value,
      tags: tags.value || null
    })
    router.push('/dashboard/employer/jobs')
  } catch {
    error.value = 'Could not save changes. Please check your details.'
  } finally {
    submitting.value = false
  }
}

useSeoMeta({ title: 'Edit Job — JobBoard' })
</script>

<template>
  <div class="mx-auto flex max-w-lg flex-col gap-6 py-6">
    <h1 class="text-2xl font-bold text-slate-900">Edit job</h1>

    <form v-if="job" class="flex flex-col gap-4" @submit.prevent="onSubmit">
      <div class="flex flex-col gap-1">
        <label for="title" class="text-sm font-medium text-slate-700">Title</label>
        <input id="title" v-model="title" type="text" required class="rounded-md border border-slate-300 px-3 py-2 text-sm">
      </div>
      <div class="flex flex-col gap-1">
        <label for="description" class="text-sm font-medium text-slate-700">Description</label>
        <textarea id="description" v-model="description" rows="6" required class="rounded-md border border-slate-300 px-3 py-2 text-sm" />
      </div>
      <div class="flex flex-col gap-1">
        <label for="location" class="text-sm font-medium text-slate-700">Location</label>
        <input id="location" v-model="location" type="text" required class="rounded-md border border-slate-300 px-3 py-2 text-sm">
      </div>
      <label class="flex items-center gap-2 text-sm text-slate-700">
        <input v-model="isRemote" type="checkbox" class="h-4 w-4 rounded border-slate-300">
        This role is remote
      </label>
      <div class="grid grid-cols-2 gap-3">
        <div class="flex flex-col gap-1">
          <label for="salaryMin" class="text-sm font-medium text-slate-700">Salary min</label>
          <input id="salaryMin" v-model="salaryMin" type="number" class="rounded-md border border-slate-300 px-3 py-2 text-sm">
        </div>
        <div class="flex flex-col gap-1">
          <label for="salaryMax" class="text-sm font-medium text-slate-700">Salary max</label>
          <input id="salaryMax" v-model="salaryMax" type="number" class="rounded-md border border-slate-300 px-3 py-2 text-sm">
        </div>
      </div>
      <div class="flex flex-col gap-1">
        <label for="jobType" class="text-sm font-medium text-slate-700">Job type</label>
        <select id="jobType" v-model.number="jobType" class="rounded-md border border-slate-300 px-3 py-2 text-sm">
          <option v-for="(label, value) in JobTypeLabels" :key="value" :value="Number(value)">{{ label }}</option>
        </select>
      </div>
      <div class="flex flex-col gap-1">
        <label for="tags" class="text-sm font-medium text-slate-700">Tags (comma-separated)</label>
        <input id="tags" v-model="tags" type="text" class="rounded-md border border-slate-300 px-3 py-2 text-sm">
      </div>

      <p v-if="error" class="text-sm text-red-600">{{ error }}</p>

      <button
        type="submit" :disabled="submitting"
        class="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700 disabled:opacity-50"
      >
        {{ submitting ? 'Saving…' : 'Save changes' }}
      </button>
    </form>
  </div>
</template>
