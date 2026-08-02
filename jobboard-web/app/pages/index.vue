<script setup lang="ts">
const { t } = useI18n()
const localePath = useLocalePath()

useSeoMeta({
  title: () => t('home.seoTitle'),
  description: () => t('home.seoDescription')
})

const features = ['search', 'apply', 'employers'] as const
</script>

<template>
  <div class="flex flex-col gap-16 py-12 sm:py-16">
    <div class="flex flex-col items-center gap-6 text-center">
      <h1 class="max-w-2xl text-4xl font-bold tracking-tight text-slate-900 dark:text-slate-100 sm:text-5xl">
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

    <div class="grid grid-cols-1 gap-6 sm:grid-cols-3">
      <Card v-for="feature in features" :key="feature">
        <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ t(`home.features.${feature}.title`) }}</h2>
        <p class="mt-2 text-sm text-slate-600 dark:text-slate-400">{{ t(`home.features.${feature}.desc`) }}</p>
      </Card>
    </div>
  </div>
</template>
