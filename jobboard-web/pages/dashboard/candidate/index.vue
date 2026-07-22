<script setup lang="ts">
definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Candidate')

useSeoMeta({ title: 'Candidate Dashboard — JobBoard' })
</script>

<template>
  <div class="flex flex-col gap-4">
    <h1 class="text-2xl font-bold text-slate-900">Candidate Dashboard</h1>

    <NuxtLink
      to="/dashboard/candidate/applications"
      class="rounded-lg border border-slate-200 bg-white p-6 hover:border-slate-400"
    >
      <h2 class="font-semibold text-slate-900">My Applications</h2>
      <p class="text-sm text-slate-600">Track the status of jobs you've applied to.</p>
    </NuxtLink>

    <NuxtLink
      to="/dashboard/candidate/profile"
      class="rounded-lg border border-slate-200 bg-white p-6 hover:border-slate-400"
    >
      <h2 class="font-semibold text-slate-900">My Profile</h2>
      <p class="text-sm text-slate-600">Update your headline, bio, and skills.</p>
    </NuxtLink>

    <NuxtLink
      to="/jobs"
      class="rounded-lg border border-slate-200 bg-white p-6 hover:border-slate-400"
    >
      <h2 class="font-semibold text-slate-900">Browse Jobs</h2>
      <p class="text-sm text-slate-600">Find your next role.</p>
    </NuxtLink>
  </div>
</template>
