<script setup lang="ts">
const props = defineProps<{ jobId: string }>()

const auth = useAuthStore()
const { applyToJob, getMyApplications } = useApplicationsApi()

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
    error.value = 'Could not submit your application. Please try again.'
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="rounded-lg border border-slate-200 bg-white p-6">
    <div v-if="!auth.isAuthenticated" class="text-center">
      <p class="mb-3 text-sm text-slate-600">Log in as a candidate to apply for this role.</p>
      <NuxtLink
        :to="`/login?redirect=/jobs/${jobId}`"
        class="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700"
      >
        Log in to apply
      </NuxtLink>
    </div>

    <p v-else-if="!auth.isCandidate" class="text-center text-sm text-slate-500">
      Only candidate accounts can apply to jobs.
    </p>

    <p v-else-if="submitted || alreadyApplied" class="text-center text-sm font-medium text-emerald-700">
      You've applied to this job. Track it from <NuxtLink to="/dashboard/candidate/applications" class="underline">My Applications</NuxtLink>.
    </p>

    <form v-else class="flex flex-col gap-3" @submit.prevent="onApply">
      <label for="coverLetter" class="text-sm font-medium text-slate-700">Cover letter (optional)</label>
      <textarea
        id="coverLetter" v-model="coverLetter" rows="4"
        class="rounded-md border border-slate-300 px-3 py-2 text-sm"
        placeholder="Why are you a good fit for this role?"
      />
      <p v-if="error" class="text-sm text-red-600">{{ error }}</p>
      <button
        type="submit" :disabled="submitting"
        class="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700 disabled:opacity-50"
      >
        {{ submitting ? 'Submitting…' : 'Apply now' }}
      </button>
    </form>
  </div>
</template>
