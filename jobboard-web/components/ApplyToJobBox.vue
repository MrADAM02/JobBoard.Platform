<script setup lang="ts">
const props = defineProps<{ jobId: string }>()

const auth = useAuthStore()
const { applyToJob, getMyApplications } = useApplicationsApi()
const { t } = useI18n()

const coverLetter = ref('')
const submitting = ref(false)
const submitted = ref(false)
const alreadyApplied = ref(false)
const error = ref<string | null>(null)

onMounted(async () => {
  if (!auth.isCandidate) return
  try {
    const mine = await getMyApplications()
    alreadyApplied.value = mine.some((a) => a.jobListingId === props.jobId)
  } catch {
    // non-fatal - worst case the apply button shows and the API rejects a duplicate
  }
})

async function onApply() {
  error.value = null
  submitting.value = true
  try {
    await applyToJob(props.jobId, coverLetter.value || null)
    submitted.value = true
  } catch {
    error.value = t('jobs.apply.error')
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="rounded-lg border border-slate-200 bg-white p-6 dark:border-slate-700 dark:bg-slate-900">
    <div v-if="!auth.isAuthenticated" class="text-center">
      <p class="mb-3 text-sm text-slate-600 dark:text-slate-400">{{ t('jobs.apply.loginPrompt') }}</p>
      <NuxtLink
        :to="`/login?redirect=/jobs/${jobId}`"
        class="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-300"
      >
        {{ t('jobs.apply.loginToApply') }}
      </NuxtLink>
    </div>

    <p v-else-if="!auth.isCandidate" class="text-center text-sm text-slate-500 dark:text-slate-400">
      {{ t('jobs.apply.onlyCandidates') }}
    </p>

    <i18n-t
      v-else-if="submitted || alreadyApplied"
      keypath="jobs.apply.appliedMessage" tag="p"
      class="text-center text-sm font-medium text-emerald-700 dark:text-emerald-400"
    >
      <template #link>
        <NuxtLink to="/dashboard/candidate/applications" class="underline">{{ t('jobs.apply.myApplicationsLink') }}</NuxtLink>
      </template>
    </i18n-t>

    <form v-else class="flex flex-col gap-3" @submit.prevent="onApply">
      <label for="coverLetter" class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ t('jobs.apply.coverLetterLabel') }}</label>
      <textarea
        id="coverLetter" v-model="coverLetter" rows="4"
        class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100"
        :placeholder="t('jobs.apply.coverLetterPlaceholder')"
      />
      <p v-if="error" class="text-sm text-red-600 dark:text-red-400">{{ error }}</p>
      <button
        type="submit" :disabled="submitting"
        class="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700 disabled:opacity-50 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-300"
      >
        {{ submitting ? t('jobs.apply.submitting') : t('jobs.apply.submit') }}
      </button>
    </form>
  </div>
</template>
