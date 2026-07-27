<script setup lang="ts">
import { JobTypeI18nKey } from '~/types/job'
import type { JobTypeValue } from '~/types/job'

definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Employer')

const route = useRoute()
const router = useRouter()
const jobId = route.params.id as string
const { t } = useI18n()
const localePath = useLocalePath()

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
const expiresAt = ref(job.value?.expiresAt?.slice(0, 10) ?? '')
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
      tags: tags.value || null,
      expiresAt: expiresAt.value || null
    })
    useToast().success(t('dashboard.employer.jobsEdit.success'))
    router.push(localePath('/dashboard/employer/jobs'))
  } catch {
    error.value = t('dashboard.employer.jobsEdit.error')
  } finally {
    submitting.value = false
  }
}

useSeoMeta({ title: () => t('dashboard.employer.jobsEdit.seoTitle') })
</script>

<template>
  <div class="mx-auto flex max-w-lg flex-col gap-6 py-6">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('dashboard.employer.jobsEdit.title') }}</h1>

    <Card v-if="job">
      <form class="flex flex-col gap-4" @submit.prevent="onSubmit">
        <BaseInput id="title" v-model="title" required :label="t('dashboard.employer.jobForm.titleLabel')" />
        <BaseTextarea id="description" v-model="description" :rows="6" required :label="t('dashboard.employer.jobForm.descriptionLabel')" />
        <BaseInput id="location" v-model="location" required :label="t('dashboard.employer.jobForm.locationLabel')" />
        <label class="flex items-center gap-2 text-sm text-slate-700 dark:text-slate-300">
          <input v-model="isRemote" type="checkbox" class="h-4 w-4 rounded border-slate-300 text-primary-600 dark:border-slate-700 dark:bg-slate-900">
          {{ t('dashboard.employer.jobForm.isRemote') }}
        </label>
        <div class="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <BaseInput id="salaryMin" v-model="salaryMin" type="number" :label="t('dashboard.employer.jobForm.salaryMinLabel')" />
          <BaseInput id="salaryMax" v-model="salaryMax" type="number" :label="t('dashboard.employer.jobForm.salaryMaxLabel')" />
        </div>
        <div class="flex flex-col gap-1">
          <label for="jobType" class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ t('dashboard.employer.jobForm.jobTypeLabel') }}</label>
          <select id="jobType" v-model.number="jobType" class="rounded-lg border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100">
            <option v-for="(i18nKey, value) in JobTypeI18nKey" :key="value" :value="Number(value)">{{ t(i18nKey) }}</option>
          </select>
        </div>
        <BaseInput id="tags" v-model="tags" :label="t('dashboard.employer.jobForm.tagsLabel')" />
        <BaseInput id="expiresAt" v-model="expiresAt" type="date" :label="t('dashboard.employer.jobForm.expiresAtLabel')" :hint="t('dashboard.employer.jobForm.expiresAtHint')" />

        <Alert v-if="error">{{ error }}</Alert>

        <BaseButton type="submit" :loading="submitting" class="justify-center">
          {{ submitting ? t('dashboard.employer.jobsEdit.submitting') : t('dashboard.employer.jobsEdit.submit') }}
        </BaseButton>
      </form>
    </Card>
  </div>
</template>
