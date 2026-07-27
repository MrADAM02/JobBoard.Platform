<script setup lang="ts">
import { JobTypeI18nKey } from '~/types/job'
import type { JobTypeValue } from '~/types/job'

definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Employer')

const { getMyCompany } = useCompaniesApi()
const { createJobListing } = useJobsApi()
const router = useRouter()
const { t } = useI18n()
const localePath = useLocalePath()

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
    useToast().success(t('dashboard.employer.jobsNew.success'))
    router.push(localePath('/dashboard/employer/jobs'))
  } catch {
    error.value = t('dashboard.employer.jobsNew.error')
  } finally {
    submitting.value = false
  }
}

useSeoMeta({ title: () => t('dashboard.employer.jobsNew.seoTitle') })
</script>

<template>
  <div class="mx-auto flex max-w-lg flex-col gap-6 py-6">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('dashboard.employer.jobsNew.title') }}</h1>

    <Alert v-if="!company" variant="warning">
      {{ t('dashboard.employer.jobsNew.setupCompanyFirst') }}
      <NuxtLink :to="localePath('/dashboard/employer/company')" class="underline">{{ t('dashboard.employer.jobsNew.companyProfileLink') }}</NuxtLink>
    </Alert>

    <Card v-else>
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
        <BaseInput id="tags" v-model="tags" :placeholder="t('dashboard.employer.jobForm.tagsPlaceholder')" :label="t('dashboard.employer.jobForm.tagsLabel')" />
        <label class="flex items-center gap-2 text-sm text-slate-700 dark:text-slate-300">
          <input v-model="publishImmediately" type="checkbox" class="h-4 w-4 rounded border-slate-300 text-primary-600 dark:border-slate-700 dark:bg-slate-900">
          {{ t('dashboard.employer.jobsNew.publishImmediately') }}
        </label>

        <Alert v-if="error">{{ error }}</Alert>

        <BaseButton type="submit" :loading="submitting" class="justify-center">
          {{ submitting ? t('dashboard.employer.jobsNew.submitting') : t('dashboard.employer.jobsNew.submit') }}
        </BaseButton>
      </form>
    </Card>
  </div>
</template>
