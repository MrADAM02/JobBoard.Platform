<script setup lang="ts">
// Self-contained (calls the API itself) rather than lifting save/unsave up
// to the parent - the parent only needs to know the initial saved state.
// .stop.prevent on click matters here specifically: this button is nested
// inside Card's <NuxtLink> on the /jobs list, so an unguarded click would
// also navigate to the job detail page.
const props = defineProps<{ jobId: string, modelValue: boolean }>()
const emit = defineEmits<{ 'update:modelValue': [value: boolean] }>()

const { t } = useI18n()
const { saveJob, unsaveJob } = useSavedJobsApi()
const loading = ref(false)

async function toggle() {
  if (loading.value) return
  loading.value = true
  const next = !props.modelValue
  try {
    await (next ? saveJob(props.jobId) : unsaveJob(props.jobId))
    emit('update:modelValue', next)
  } catch {
    // non-fatal - button just stays in its previous state
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <button
    type="button"
    class="rounded-lg p-1.5 transition-colors disabled:opacity-50"
    :class="modelValue
      ? 'bg-accent-400 text-slate-900 hover:bg-accent-500'
      : 'text-slate-500 hover:bg-slate-100 hover:text-primary-600 dark:text-slate-400 dark:hover:bg-slate-800'"
    :aria-label="modelValue ? t('jobs.list.unsave') : t('jobs.list.save')"
    :disabled="loading"
    @click.stop.prevent="toggle"
  >
    <svg
      xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"
      class="h-5 w-5" :class="modelValue ? 'fill-slate-900 stroke-slate-900' : 'fill-none'"
    >
      <path stroke-linecap="round" stroke-linejoin="round" d="M6 4a2 2 0 0 0-2 2v14l8-5 8 5V6a2 2 0 0 0-2-2H6z" />
    </svg>
  </button>
</template>
