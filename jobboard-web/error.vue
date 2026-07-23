<script setup lang="ts">
const props = defineProps<{ error: { statusCode: number; statusMessage?: string } }>()

useSeoMeta({
  title: props.error.statusCode === 404 ? 'Page not found — JobBoard' : 'Something went wrong — JobBoard'
})

function goHome() {
  clearError({ redirect: '/' })
}
</script>

<template>
  <div class="flex min-h-screen flex-col items-center justify-center gap-4 bg-slate-50 px-4 text-center dark:bg-slate-950">
    <p class="text-6xl font-bold text-slate-300 dark:text-slate-700">{{ error.statusCode }}</p>
    <h1 class="text-xl font-semibold text-slate-900 dark:text-slate-100">
      {{ error.statusCode === 404 ? 'Job not found' : 'Something went wrong' }}
    </h1>
    <p class="text-slate-600 dark:text-slate-400">
      {{ error.statusCode === 404
        ? "This listing doesn't exist, or has been removed."
        : (error.statusMessage || 'Please try again in a moment.') }}
    </p>
    <button
      class="rounded-md bg-slate-900 px-5 py-2.5 text-sm font-semibold text-white hover:bg-slate-700 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-300"
      @click="goHome"
    >
      Back to JobBoard
    </button>
  </div>
</template>
