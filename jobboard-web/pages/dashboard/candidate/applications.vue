<script setup lang="ts">
import { ApplicationStatusLabels } from '~/types/application'

definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Candidate')

const { getMyApplications } = useApplicationsApi()
const { data: applications } = await useAsyncData('my-applications', () => getMyApplications())

useSeoMeta({ title: 'My Applications — JobBoard' })
</script>

<template>
  <div class="flex flex-col gap-6">
    <h1 class="text-2xl font-bold text-slate-900">My Applications</h1>

    <div v-if="!applications?.length" class="rounded-lg border border-slate-200 bg-white p-10 text-center text-slate-500">
      You haven't applied to any jobs yet.
      <NuxtLink to="/jobs" class="block underline">Browse open roles</NuxtLink>
    </div>

    <ul v-else class="flex flex-col gap-3">
      <li v-for="app in applications" :key="app.id" class="rounded-lg border border-slate-200 bg-white p-5">
        <div class="flex items-center justify-between gap-4">
          <div>
            <NuxtLink :to="`/jobs/${app.jobListingId}`" class="font-semibold text-slate-900 hover:underline">
              {{ app.jobTitle }}
            </NuxtLink>
            <p class="text-sm text-slate-600">{{ app.companyName }} &middot; Applied {{ new Date(app.appliedAt).toLocaleDateString() }}</p>
          </div>
          <span class="whitespace-nowrap rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-700">
            {{ ApplicationStatusLabels[app.status] }}
          </span>
        </div>
      </li>
    </ul>
  </div>
</template>
