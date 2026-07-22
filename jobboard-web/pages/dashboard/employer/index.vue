<script setup lang="ts">
// ssr: false - auth state is client-only (localStorage), so any data fetch here
// would run unauthenticated on the server. These pages have no SEO value anyway.
definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Employer')

const { getMyCompany } = useCompaniesApi()
const { data: company, status } = await useAsyncData('my-company', () => getMyCompany())

useSeoMeta({ title: 'Employer Dashboard — JobBoard' })
</script>

<template>
  <div class="flex flex-col gap-6">
    <h1 class="text-2xl font-bold text-slate-900">Employer Dashboard</h1>

    <div v-if="status === 'pending'" class="text-slate-500">Loading…</div>

    <div v-else-if="!company" class="rounded-lg border border-slate-200 bg-white p-8 text-center">
      <p class="mb-4 text-slate-600">You haven't set up your company profile yet.</p>
      <NuxtLink
        to="/dashboard/employer/company"
        class="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700"
      >
        Set up company profile
      </NuxtLink>
    </div>

    <div v-else class="flex flex-col gap-4">
      <div class="rounded-lg border border-slate-200 bg-white p-6">
        <div class="flex items-start justify-between">
          <div>
            <h2 class="font-semibold text-slate-900">{{ company.name }}</h2>
            <p class="text-sm text-slate-600">{{ company.location || 'No location set' }}</p>
          </div>
          <NuxtLink to="/dashboard/employer/company" class="text-sm text-slate-600 underline">Edit</NuxtLink>
        </div>
      </div>

      <NuxtLink
        to="/dashboard/employer/jobs"
        class="rounded-lg border border-slate-200 bg-white p-6 hover:border-slate-400"
      >
        <h2 class="font-semibold text-slate-900">My Job Listings</h2>
        <p class="text-sm text-slate-600">View, edit, and manage your postings.</p>
      </NuxtLink>
    </div>
  </div>
</template>
