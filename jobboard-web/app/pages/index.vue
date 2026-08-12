<script setup lang="ts">
const { t } = useI18n()
const localePath = useLocalePath()
const router = useRouter()
const { getJobListings } = useJobsApi()

useSeoMeta({
  title: () => t('home.seoTitle'),
  description: () => t('home.seoDescription')
})

const features = ['search', 'apply', 'employers'] as const

const keyword = ref('')
const location = ref('')

function onSearch() {
  router.push({
    path: localePath('/jobs'),
    query: {
      keyword: keyword.value || undefined,
      location: location.value || undefined
    }
  })
}

// Public, part of SSR so the "fresh on the board" grid is real crawlable
// content, not a client-only afterthought - matches how /jobs already fetches.
const { data: fresh } = await useAsyncData('home-fresh-jobs', () => getJobListings({ pageSize: 6 }))
</script>

<template>
  <div class="flex flex-col gap-16 py-8 sm:py-12">
    <div class="grid grid-cols-1 items-center gap-8 lg:grid-cols-2 lg:gap-12">
      <div class="flex flex-col gap-5">
        <span class="inline-flex w-fit items-center gap-2 text-xs font-semibold uppercase tracking-wider text-primary-700 dark:text-primary-400">
          <span class="h-1.5 w-1.5 rounded-full bg-accent-400" />
          {{ t('home.eyebrow') }}
        </span>
        <h1 class="text-4xl font-bold tracking-tight text-slate-900 dark:text-slate-100 sm:text-5xl">
          {{ t('home.heading') }}
        </h1>
        <p class="max-w-md text-lg text-slate-600 dark:text-slate-400">
          {{ t('home.subheading') }}
        </p>
        <div class="flex flex-col gap-3 sm:flex-row">
          <BaseButton :to="localePath('/jobs')" size="md">{{ t('home.browseJobs') }}</BaseButton>
          <BaseButton :to="localePath('/register')" variant="secondary" size="md">{{ t('home.forEmployers') }}</BaseButton>
        </div>
      </div>

      <form class="flex flex-col gap-3 rounded-2xl border border-slate-200/70 bg-white p-5 dark:border-slate-800 dark:bg-slate-900" @submit.prevent="onSearch">
        <p class="text-sm font-semibold text-slate-700 dark:text-slate-300">{{ t('home.searchCardTitle') }}</p>
        <input
          v-model="keyword"
          type="text"
          :placeholder="t('home.keywordPlaceholder')"
          class="rounded-xl border border-slate-300 px-3 py-2.5 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100 dark:placeholder:text-slate-500"
        >
        <input
          v-model="location"
          type="text"
          :placeholder="t('home.locationPlaceholder')"
          class="rounded-xl border border-slate-300 px-3 py-2.5 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100 dark:placeholder:text-slate-500"
        >
        <BaseButton type="submit" class="justify-center">{{ t('home.searchCta') }}</BaseButton>
      </form>
    </div>

    <div v-if="fresh?.items.length" class="flex flex-col gap-5 rounded-2xl bg-cream-200 p-6 dark:bg-slate-900/40 sm:p-8">
      <div>
        <span class="text-xs font-semibold uppercase tracking-wider text-primary-700 dark:text-primary-400">{{ t('home.freshEyebrow') }}</span>
        <h2 class="mt-1 text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('home.freshTitle') }}</h2>
      </div>
      <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <JobCard v-for="job in fresh.items" :key="job.id" :job="job" />
      </div>
    </div>

    <div class="flex flex-col gap-6">
      <span class="text-xs font-semibold uppercase tracking-wider text-primary-700 dark:text-primary-400">{{ t('home.whyEyebrow') }}</span>
      <div class="grid grid-cols-1 gap-6 sm:grid-cols-3">
        <Card v-for="feature in features" :key="feature">
          <h3 class="font-semibold text-slate-900 dark:text-slate-100">{{ t(`home.features.${feature}.title`) }}</h3>
          <p class="mt-2 text-sm text-slate-600 dark:text-slate-400">{{ t(`home.features.${feature}.desc`) }}</p>
        </Card>
      </div>
    </div>
  </div>
</template>
