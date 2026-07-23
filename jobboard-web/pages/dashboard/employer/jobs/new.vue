<script setup lang="ts">
import { JobTypeLabels } from '~/types/job'
import type { JobTypeValue } from '~/types/job'

definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Employer')

const { getMyCompany } = useCompaniesApi()
const { createJobListing } = useJobsApi()
const router = useRouter()

const { data: company } = await useAsyncData('my-company-for-new-job', () => getMyCompany())

const title = ref('')
const description = ref('')
const location = ref('')
const isRemote = ref(false)
const salaryMin = ref('')
const salaryMax = ref('')
const jobType = ref<JobTypeValue>(0)
const tags = ref('')
const publishImmediately = ref(true)
const error = ref<string | null>(null)
const submitting = ref(false)

async function onSubmit() {
  if (!company.value) return
  error.value = null
  submitting.value = true
  try {
    await createJobListing({
      companyId: company.value.id,
      title: title.value,
      description: description.value,
      location: location.value,
      isRemote: isRemote.value,
      salaryMin: salaryMin.value ? Number(salaryMin.value) : null,
      salaryMax: salaryMax.value ? Number(salaryMax.value) : null,
      jobType: jobType.value,
      tags: tags.value || null,
      publishImmediately: publishImmediately.value
    })
    router.push('/dashboard/employer/jobs')
  } catch {
    error.value = 'Could not create the job listing. Please check your details.'
  } finally {
    submitting.value = false
  }
}

useSeoMeta({ title: 'Post a Job — JobBoard' })
</script>

<template>
  <div class="mx-auto flex max-w-lg flex-col gap-6 py-6">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">Post a job</h1>

    <div v-if="!company" class="rounded-lg border border-amber-200 bg-amber-50 p-6 text-amber-800 dark:border-amber-900 dark:bg-amber-950/40 dark:text-amber-300">
      Set up your <NuxtLink to="/dashboard/employer/company" class="underline">company profile</NuxtLink> first.
    </div>

    <form v-else class="flex flex-col gap-4" @submit.prevent="onSubmit">
      <div class="flex flex-col gap-1">
        <label for="title" class="text-sm font-medium text-slate-700 dark:text-slate-300">Title</label>
        <input id="title" v-model="title" type="text" required class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100">
      </div>
      <div class="flex flex-col gap-1">
        <label for="description" class="text-sm font-medium text-slate-700 dark:text-slate-300">Description</label>
        <textarea id="description" v-model="description" rows="6" required class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100" />
      </div>
      <div class="flex flex-col gap-1">
        <label for="location" class="text-sm font-medium text-slate-700 dark:text-slate-300">Location</label>
        <input id="location" v-model="location" type="text" required class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100">
      </div>
      <label class="flex items-center gap-2 text-sm text-slate-700 dark:text-slate-300">
        <input v-model="isRemote" type="checkbox" class="h-4 w-4 rounded border-slate-300 dark:border-slate-700 dark:bg-slate-900">
        This role is remote
      </label>
      <div class="grid grid-cols-2 gap-3">
        <div class="flex flex-col gap-1">
          <label for="salaryMin" class="text-sm font-medium text-slate-700 dark:text-slate-300">Salary min</label>
          <input id="salaryMin" v-model="salaryMin" type="number" class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100">
        </div>
        <div class="flex flex-col gap-1">
          <label for="salaryMax" class="text-sm font-medium text-slate-700 dark:text-slate-300">Salary max</label>
          <input id="salaryMax" v-model="salaryMax" type="number" class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100">
        </div>
      </div>
      <div class="flex flex-col gap-1">
        <label for="jobType" class="text-sm font-medium text-slate-700 dark:text-slate-300">Job type</label>
        <select id="jobType" v-model.number="jobType" class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100">
          <option v-for="(label, value) in JobTypeLabels" :key="value" :value="Number(value)">{{ label }}</option>
        </select>
      </div>
      <div class="flex flex-col gap-1">
        <label for="tags" class="text-sm font-medium text-slate-700 dark:text-slate-300">Tags (comma-separated)</label>
        <input id="tags" v-model="tags" type="text" placeholder="dotnet, vue, remote" class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100">
      </div>
      <label class="flex items-center gap-2 text-sm text-slate-700 dark:text-slate-300">
        <input v-model="publishImmediately" type="checkbox" class="h-4 w-4 rounded border-slate-300 dark:border-slate-700 dark:bg-slate-900">
        Publish immediately (otherwise saved as a draft)
      </label>

      <p v-if="error" class="text-sm text-red-600 dark:text-red-400">{{ error }}</p>

      <button
        type="submit" :disabled="submitting"
        class="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700 disabled:opacity-50 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-300"
      >
        {{ submitting ? 'Posting…' : 'Post job' }}
      </button>
    </form>
  </div>
</template>
