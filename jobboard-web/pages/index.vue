<script setup lang="ts">
const { t } = useI18n()
const localePath = useLocalePath()

useSeoMeta({
  title: () => t('home.seoTitle'),
  description: () => t('home.seoDescription')
})
</script>

<template>
  <div class="flex flex-col items-center gap-6 py-16 text-center">
    <h1 class="text-4xl font-bold tracking-tight text-slate-900 dark:text-slate-100">
      {{ t('home.heading') }}
    </h1>
    <p class="max-w-md text-slate-600 dark:text-slate-400">
      {{ t('home.subheading') }}
    </p>
    <NuxtLink
      :to="localePath('/jobs')"
      class="rounded-md bg-slate-900 px-5 py-2.5 text-sm font-semibold text-white hover:bg-slate-700 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-300"
    >
      {{ t('home.browseJobs') }}
    </NuxtLink>
  </div>
</template>
