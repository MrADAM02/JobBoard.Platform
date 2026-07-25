<script setup lang="ts">
const route = useRoute()
const { getCompanyById } = useCompaniesApi()
const { t } = useI18n()
const localePath = useLocalePath()
const companyId = route.params.id as string

const { data: company } = await useAsyncData(`company-${companyId}`, () => getCompanyById(companyId))

if (!company.value) {
  throw createError({ statusCode: 404, statusMessage: t('errors.notFoundTitle'), fatal: true })
}

useSeoMeta({
  title: () => t('companies.detail.seoTitle', { name: company.value!.name }),
  description: () => company.value!.description || t('companies.detail.seoDescription', { name: company.value!.name })
})
</script>

<template>
  <article v-if="company" class="flex flex-col gap-6">
    <NuxtLink :to="localePath('/companies')" class="text-sm text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200">
      <span class="inline-block rtl:-scale-x-100">&larr;</span> {{ t('companies.detail.backToAll') }}
    </NuxtLink>

    <header class="flex items-center gap-4 rounded-lg border border-slate-200 bg-white p-6 dark:border-slate-700 dark:bg-slate-900">
      <img
        v-if="company.logoUrl"
        :src="company.logoUrl"
        :alt="company.name"
        class="h-16 w-16 flex-shrink-0 rounded-md object-cover"
      >
      <div v-else class="flex h-16 w-16 flex-shrink-0 items-center justify-center rounded-md bg-slate-100 text-2xl font-semibold text-slate-500 dark:bg-slate-800 dark:text-slate-400">
        {{ company.name.charAt(0).toUpperCase() }}
      </div>
      <div>
        <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ company.name }}</h1>
        <p v-if="company.location" class="mt-1 text-slate-600 dark:text-slate-400">{{ company.location }}</p>
      </div>
    </header>

    <section v-if="company.description" class="rounded-lg border border-slate-200 bg-white p-6 dark:border-slate-700 dark:bg-slate-900">
      <p class="whitespace-pre-line text-slate-700 dark:text-slate-300">{{ company.description }}</p>
    </section>

    <section v-if="company.website" class="rounded-lg border border-slate-200 bg-white p-6 dark:border-slate-700 dark:bg-slate-900">
      <h2 class="mb-2 text-sm font-semibold uppercase tracking-wide text-slate-500 dark:text-slate-400">{{ t('companies.detail.website') }}</h2>
      <a :href="company.website" target="_blank" rel="noopener" class="text-sm text-slate-700 underline dark:text-slate-300">
        {{ company.website }}
      </a>
    </section>
  </article>
</template>
