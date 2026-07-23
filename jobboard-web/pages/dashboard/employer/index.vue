<script setup lang="ts">
// ssr: false - auth state is client-only (localStorage), so any data fetch here
// would run unauthenticated on the server. These pages have no SEO value anyway.
definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Employer')

const { getMyCompany } = useCompaniesApi()
const { data: company, status } = await useAsyncData('my-company', () => getMyCompany())
const { t } = useI18n()

useSeoMeta({ title: () => t('dashboard.employer.overview.title') + ' — JobBoard' })
</script>

<template>
  <div class="flex flex-col gap-6">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('dashboard.employer.overview.title') }}</h1>

    <div v-if="status === 'pending'" class="text-slate-500 dark:text-slate-400">{{ t('dashboard.employer.overview.loading') }}</div>

    <div v-else-if="!company" class="rounded-lg border border-slate-200 bg-white p-8 text-center dark:border-slate-700 dark:bg-slate-900">
      <p class="mb-4 text-slate-600 dark:text-slate-400">{{ t('dashboard.employer.overview.noCompany') }}</p>
      <NuxtLink
        to="/dashboard/employer/company"
        class="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-300"
      >
        {{ t('dashboard.employer.overview.setupCompany') }}
      </NuxtLink>
    </div>

    <div v-else class="flex flex-col gap-4">
      <div class="rounded-lg border border-slate-200 bg-white p-6 dark:border-slate-700 dark:bg-slate-900">
        <div class="flex items-start justify-between">
          <div>
            <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ company.name }}</h2>
            <p class="text-sm text-slate-600 dark:text-slate-400">{{ company.location || t('dashboard.employer.overview.noLocation') }}</p>
          </div>
          <NuxtLink to="/dashboard/employer/company" class="text-sm text-slate-600 underline dark:text-slate-400">{{ t('dashboard.employer.overview.edit') }}</NuxtLink>
        </div>
      </div>

      <NuxtLink
        to="/dashboard/employer/jobs"
        class="rounded-lg border border-slate-200 bg-white p-6 hover:border-slate-400 dark:border-slate-700 dark:bg-slate-900 dark:hover:border-slate-500"
      >
        <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ t('dashboard.employer.overview.myListingsTitle') }}</h2>
        <p class="text-sm text-slate-600 dark:text-slate-400">{{ t('dashboard.employer.overview.myListingsDesc') }}</p>
      </NuxtLink>
    </div>
  </div>
</template>
