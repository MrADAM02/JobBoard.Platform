<script setup lang="ts">
// ssr: false - auth state is client-only (localStorage), so any data fetch here
// would run unauthenticated on the server. These pages have no SEO value anyway.
definePageMeta({ layout: 'dashboard', middleware: 'auth', ssr: false })
useRequireRole('Employer')

const { getMyCompany } = useCompaniesApi()
const { data: company, status } = await useAsyncData('my-company', () => getMyCompany())
const { t } = useI18n()
const localePath = useLocalePath()

useSeoMeta({ title: () => t('dashboard.employer.overview.title') + ' — JobBoard' })
</script>

<template>
  <div class="flex flex-col gap-6">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('dashboard.employer.overview.title') }}</h1>

    <div v-if="status === 'pending'" class="text-slate-500 dark:text-slate-400">{{ t('dashboard.employer.overview.loading') }}</div>

    <Card v-else-if="!company" padding="lg" class="text-center">
      <p class="mb-4 text-slate-600 dark:text-slate-400">{{ t('dashboard.employer.overview.noCompany') }}</p>
      <BaseButton :to="localePath('/dashboard/employer/company')">
        {{ t('dashboard.employer.overview.setupCompany') }}
      </BaseButton>
    </Card>

    <div v-else class="flex flex-col gap-4">
      <Card>
        <div class="flex items-start justify-between">
          <div>
            <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ company.name }}</h2>
            <p class="text-sm text-slate-600 dark:text-slate-400">{{ company.location || t('dashboard.employer.overview.noLocation') }}</p>
          </div>
          <NuxtLink :to="localePath('/dashboard/employer/company')" class="text-sm text-primary-600 underline dark:text-primary-400">{{ t('dashboard.employer.overview.edit') }}</NuxtLink>
        </div>
      </Card>

      <Card :to="localePath('/dashboard/employer/jobs')" hover>
        <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ t('dashboard.employer.overview.myListingsTitle') }}</h2>
        <p class="text-sm text-slate-600 dark:text-slate-400">{{ t('dashboard.employer.overview.myListingsDesc') }}</p>
      </Card>

      <Card :to="localePath('/dashboard/employer/analytics')" hover>
        <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ t('dashboard.employer.overview.analyticsTitle') }}</h2>
        <p class="text-sm text-slate-600 dark:text-slate-400">{{ t('dashboard.employer.overview.analyticsDesc') }}</p>
      </Card>
    </div>
  </div>
</template>
