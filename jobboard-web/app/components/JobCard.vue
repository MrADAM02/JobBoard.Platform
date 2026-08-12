<script setup lang="ts">
import type { JobListingSummary } from '~/types/job'
import { JobTypeI18nKey } from '~/types/job'

// Shared job-row markup - was previously duplicated inline across pages/jobs/index.vue,
// pages/index.vue's "fresh on the board" section, and the dashboard "recommended" panel.
// Bookmark state stays lifted to the parent (same contract BookmarkButton already had)
// since saved-job ids are fetched once per page, not per card.
defineProps<{
  job: JobListingSummary
  saved?: boolean
  showBookmark?: boolean
}>()

defineEmits<{ 'update:saved': [value: boolean] }>()

const { t } = useI18n()
const localePath = useLocalePath()

function initials(name: string) {
  return name.trim().charAt(0).toUpperCase() || '?'
}
</script>

<template>
  <Card :to="localePath(`/jobs/${job.id}`)" hover padding="sm">
    <div class="flex items-start justify-between gap-4">
      <div class="flex items-start gap-3">
        <div class="flex h-11 w-11 shrink-0 items-center justify-center overflow-hidden rounded-xl bg-cream-200 font-display text-lg font-bold text-primary-700 dark:bg-slate-800 dark:text-primary-400">
          <img v-if="job.companyLogoUrl" :src="job.companyLogoUrl" :alt="job.companyName" class="h-full w-full object-cover">
          <span v-else>{{ initials(job.companyName) }}</span>
        </div>
        <div>
          <h3 class="font-semibold text-slate-900 dark:text-slate-100">{{ job.title }}</h3>
          <p class="text-sm text-slate-600 dark:text-slate-400">{{ job.companyName }}</p>
        </div>
      </div>
      <ClientOnly>
        <BookmarkButton
          v-if="showBookmark"
          :job-id="job.id"
          :model-value="saved ?? false"
          @update:model-value="(v) => $emit('update:saved', v)"
        />
      </ClientOnly>
    </div>

    <div class="mt-3 flex flex-wrap items-center gap-2 text-xs">
      <Badge variant="neutral">{{ job.location }}</Badge>
      <Badge v-if="job.isRemote" variant="success">{{ t('jobs.detail.remote') }}</Badge>
      <Badge variant="neutral">{{ t(JobTypeI18nKey[job.jobType]) }}</Badge>
    </div>

    <div v-if="job.salaryMin || job.salaryMax" class="mt-3 flex items-center justify-between border-t border-slate-100 pt-3 text-sm dark:border-slate-800">
      <span class="font-semibold text-primary-700 dark:text-primary-400">
        ${{ job.salaryMin?.toLocaleString() ?? '?' }} &ndash; ${{ job.salaryMax?.toLocaleString() ?? '?' }}
      </span>
      <span v-if="job.publishedAt" class="text-slate-400 dark:text-slate-500">
        {{ new Date(job.publishedAt).toLocaleDateString() }}
      </span>
    </div>
  </Card>
</template>
