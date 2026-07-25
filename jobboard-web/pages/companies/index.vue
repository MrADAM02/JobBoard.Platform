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
      class="flex gap-3 rounded-lg border border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-900"
      @submit.prevent="applyFilters"
    >
      <input
        v-model="keyword"
        type="text"
        :placeholder="t('companies.keywordPlaceholder')"
        class="flex-1 rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100 dark:placeholder:text-slate-500"
      >
      <button
        type="submit"
        class="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-300"
      >
        {{ t('companies.search') }}
      </button>
    </form>

    <div v-if="status === 'pending'" class="py-16 text-center text-slate-500 dark:text-slate-400">
      {{ t('companies.loading') }}
    </div>

    <div v-else-if="error" class="rounded-lg border border-red-200 bg-red-50 p-6 text-center text-red-700 dark:border-red-900 dark:bg-red-950/40 dark:text-red-300">
      {{ t('companies.loadError') }}
    </div>

    <div v-else-if="!data?.items.length" class="rounded-lg border border-slate-200 bg-white p-10 text-center text-slate-500 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-400">
      {{ t('companies.empty') }}
    </div>

    <ul v-else class="grid grid-cols-1 gap-3 sm:grid-cols-2">
      <li v-for="company in data.items" :key="company.id">
        <NuxtLink
          :to="localePath(`/companies/${company.id}`)"
          class="flex items-center gap-4 rounded-lg border border-slate-200 bg-white p-5 transition hover:border-slate-400 hover:shadow-sm dark:border-slate-700 dark:bg-slate-900 dark:hover:border-slate-500"
        >
          <img
            v-if="company.logoUrl"
            :src="company.logoUrl"
            :alt="company.name"
            class="h-12 w-12 flex-shrink-0 rounded-md object-cover"
          >
          <div v-else class="flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-md bg-slate-100 text-lg font-semibold text-slate-500 dark:bg-slate-800 dark:text-slate-400">
            {{ company.name.charAt(0).toUpperCase() }}
          </div>
          <div>
            <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ company.name }}</h2>
            <p v-if="company.location" class="text-sm text-slate-600 dark:text-slate-400">{{ company.location }}</p>
            <p class="text-xs text-slate-500 dark:text-slate-400">
              {{ t('companies.openJobCount', { count: company.openJobCount }, company.openJobCount) }}
            </p>
          </div>
        </NuxtLink>
      </li>
    </ul>

    <div v-if="data && data.totalPages > 1" class="flex items-center justify-center gap-3 pt-2">
      <button
        :disabled="!data.hasPreviousPage"
        class="rounded-md border border-slate-300 px-3 py-1.5 text-sm disabled:opacity-40 dark:border-slate-700"
        @click="goToPage(data.pageNumber - 1)"
      >
        {{ t('jobs.list.previous') }}
      </button>
      <span class="text-sm text-slate-600 dark:text-slate-400">{{ t('jobs.list.pageOf', { page: data.pageNumber, total: data.totalPages }) }}</span>
      <button
        :disabled="!data.hasNextPage"
        class="rounded-md border border-slate-300 px-3 py-1.5 text-sm disabled:opacity-40 dark:border-slate-700"
        @click="goToPage(data.pageNumber + 1)"
      >
        {{ t('jobs.list.next') }}
      </button>
    </div>
  </div>
</template>
