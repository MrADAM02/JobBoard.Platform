<script setup lang="ts">
// Shared by /jobs and /companies - both paginate the same way (page number in
// the URL query string, server-driven PaginatedList<T> shape). Shows numbered
// page buttons with ellipsis for large page counts, not just Prev/Next, so
// a user can jump straight to a page instead of clicking through every one.
const props = defineProps<{
  pageNumber: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}>()

const emit = defineEmits<{ change: [page: number] }>()

const { t } = useI18n()

// Always shows page 1, the last page, and a window around the current page;
// collapses everything else into a single "…" separator.
const pages = computed(() => {
  const { totalPages: total, pageNumber: current } = props
  const result: (number | '…')[] = []

  for (let page = 1; page <= total; page++) {
    const isEdge = page === 1 || page === total
    const isNearCurrent = page >= current - 1 && page <= current + 1
    if (isEdge || isNearCurrent) {
      result.push(page)
    } else if (result[result.length - 1] !== '…') {
      result.push('…')
    }
  }

  return result
})
</script>

<template>
  <nav :aria-label="t('jobs.list.pageOf', { page: pageNumber, total: totalPages })" class="flex items-center justify-center gap-1 pt-2">
    <BaseButton variant="secondary" size="sm" :disabled="!hasPreviousPage" :aria-label="t('jobs.list.previous')" @click="emit('change', pageNumber - 1)">
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="h-4 w-4 rtl:-scale-x-100">
        <path d="M15 18l-6-6 6-6" />
      </svg>
    </BaseButton>

    <template v-for="(page, index) in pages" :key="`${page}-${index}`">
      <span v-if="page === '…'" class="px-2 text-sm text-slate-400 dark:text-slate-500">&hellip;</span>
      <button
        v-else
        type="button"
        class="min-w-8 rounded-lg px-2.5 py-1.5 text-sm font-medium"
        :class="page === pageNumber
          ? 'bg-primary-600 text-white'
          : 'text-slate-600 hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-slate-800'"
        :aria-current="page === pageNumber ? 'page' : undefined"
        @click="emit('change', page)"
      >
        {{ page }}
      </button>
    </template>

    <BaseButton variant="secondary" size="sm" :disabled="!hasNextPage" :aria-label="t('jobs.list.next')" @click="emit('change', pageNumber + 1)">
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="h-4 w-4 rtl:-scale-x-100">
        <path d="M9 18l6-6-6-6" />
      </svg>
    </BaseButton>
  </nav>
</template>
