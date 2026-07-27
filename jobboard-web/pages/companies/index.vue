<script setup lang="ts">
const route = useRoute()
const router = useRouter()
const { getCompanies } = useCompaniesApi()
const { t } = useI18n()
const localePath = useLocalePath()

const keyword = ref((route.query.keyword as string) || '')

const { data, status, error } = await useAsyncData(
  () => `companies-${JSON.stringify(route.query)}`,
  () => getCompanies((route.query.keyword as string) || undefined, route.query.page ? Number(route.query.page) : 1),
  { watch: [() => route.query] }
)

function applyFilters() {
  router.push({ path: localePath('/companies'), query: { keyword: keyword.value || undefined } })
}

function goToPage(page: number) {
  router.push({ path: localePath('/companies'), query: { ...route.query, page: String(page) } })
}

useSeoMeta({
  title: () => t('companies.seoTitle'),
  description: () => t('companies.seoDescription')
})
</script>

<template>
  <div class="flex flex-col gap-8">
    <div>
      <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('companies.heading') }}</h1>
      <p class="mt-1 text-sm text-slate-600 dark:text-slate-400">
        {{ t('companies.resultCount', { count: data?.totalCount ?? 0 }, data?.totalCount ?? 0) }}
      </p>
    </div>

    <form
      class="flex gap-3 rounded-xl border border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-900"
      @submit.prevent="applyFilters"
    >
      <input
        v-model="keyword"
        type="text"
        :placeholder="t('companies.keywordPlaceholder')"
        class="flex-1 rounded-lg border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100 dark:placeholder:text-slate-500"
      >
      <BaseButton type="submit">{{ t('companies.search') }}</BaseButton>
    </form>

    <div v-if="status === 'pending'" class="py-16 text-center text-slate-500 dark:text-slate-400">
      {{ t('companies.loading') }}
    </div>

    <Alert v-else-if="error">{{ t('companies.loadError') }}</Alert>

    <EmptyState v-else-if="!data?.items.length">{{ t('companies.empty') }}</EmptyState>

    <ul v-else class="grid grid-cols-1 gap-3 sm:grid-cols-2">
      <li v-for="company in data.items" :key="company.id">
        <Card :to="localePath(`/companies/${company.id}`)" hover padding="sm">
          <div class="flex items-center gap-4">
            <img
              v-if="company.logoUrl"
              :src="company.logoUrl"
              :alt="company.name"
              class="h-12 w-12 flex-shrink-0 rounded-md object-cover"
            >
            <div v-else class="flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-md bg-primary-100 text-lg font-semibold text-primary-700 dark:bg-primary-900/40 dark:text-primary-300">
              {{ company.name.charAt(0).toUpperCase() }}
            </div>
            <div>
              <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ company.name }}</h2>
              <p v-if="company.location" class="text-sm text-slate-600 dark:text-slate-400">{{ company.location }}</p>
              <p class="text-xs text-slate-500 dark:text-slate-400">
                {{ t('companies.openJobCount', { count: company.openJobCount }, company.openJobCount) }}
              </p>
            </div>
          </div>
        </Card>
      </li>
    </ul>

    <Pagination
      v-if="data && data.totalPages > 1"
      :page-number="data.pageNumber"
      :total-pages="data.totalPages"
      :has-previous-page="data.hasPreviousPage"
      :has-next-page="data.hasNextPage"
      @change="goToPage"
    />
  </div>
</template>
