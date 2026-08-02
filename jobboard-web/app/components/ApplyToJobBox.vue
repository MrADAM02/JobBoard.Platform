<script setup lang="ts">
const props = defineProps<{ jobId: string }>()

const auth = useAuthStore()
const { applyToJob, getMyApplications } = useApplicationsApi()
const { t } = useI18n()
const localePath = useLocalePath()

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
  <Card>
    <div v-if="!auth.isAuthenticated" class="text-center">
      <p class="mb-3 text-sm text-slate-600 dark:text-slate-400">{{ t('jobs.apply.loginPrompt') }}</p>
      <BaseButton :to="{ path: localePath('/login'), query: { redirect: localePath(`/jobs/${jobId}`) } }">
        {{ t('jobs.apply.loginToApply') }}
      </BaseButton>
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
        <NuxtLink :to="localePath('/dashboard/candidate/applications')" class="underline">{{ t('jobs.apply.myApplicationsLink') }}</NuxtLink>
      </template>
    </i18n-t>

    <form v-else class="flex flex-col gap-3" @submit.prevent="onApply">
      <BaseTextarea
        id="coverLetter"
        v-model="coverLetter"
        :label="t('jobs.apply.coverLetterLabel')"
        :placeholder="t('jobs.apply.coverLetterPlaceholder')"
      />
      <Alert v-if="error">{{ error }}</Alert>
      <BaseButton type="submit" :loading="submitting">
        {{ submitting ? t('jobs.apply.submitting') : t('jobs.apply.submit') }}
      </BaseButton>
    </form>
  </Card>
</template>
