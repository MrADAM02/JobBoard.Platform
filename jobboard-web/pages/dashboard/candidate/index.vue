<script setup lang="ts">
definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Candidate')

useSeoMeta({ title: 'Candidate Dashboard — JobBoard' })
</script>

<template>
  <div class="flex flex-col gap-4">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">Candidate Dashboard</h1>

    <NuxtLink
      to="/dashboard/candidate/applications"
      class="rounded-lg border border-slate-200 bg-white p-6 hover:border-slate-400 dark:border-slate-700 dark:bg-slate-900 dark:hover:border-slate-500"
    >
      <h2 class="font-semibold text-slate-900 dark:text-slate-100">My Applications</h2>
      <p class="text-sm text-slate-600 dark:text-slate-400">Track the status of jobs you've applied to.</p>
    </NuxtLink>

    <NuxtLink
      to="/dashboard/candidate/profile"
      class="rounded-lg border border-slate-200 bg-white p-6 hover:border-slate-400 dark:border-slate-700 dark:bg-slate-900 dark:hover:border-slate-500"
    >
      <h2 class="font-semibold text-slate-900 dark:text-slate-100">My Profile</h2>
      <p class="text-sm text-slate-600 dark:text-slate-400">Update your headline, bio, and skills.</p>
    </NuxtLink>

    <NuxtLink
      to="/jobs"
      class="rounded-lg border border-slate-200 bg-white p-6 hover:border-slate-400 dark:border-slate-700 dark:bg-slate-900 dark:hover:border-slate-500"
    >
      <h2 class="font-semibold text-slate-900 dark:text-slate-100">Browse Jobs</h2>
      <p class="text-sm text-slate-600 dark:text-slate-400">Find your next role.</p>
    </NuxtLink>
  </div>
</template>
