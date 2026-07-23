<script setup lang="ts">
const props = defineProps<{ error: { statusCode: number; statusMessage?: string } }>()
const { t } = useI18n()

useSeoMeta({
  title: () => (props.error.statusCode === 404 ? t('errors.notFoundSeoTitle') : t('errors.genericSeoTitle'))
})

function goHome() {
  clearError({ redirect: '/' })
}
</script>

<template>
  <div class="flex min-h-screen flex-col items-center justify-center gap-4 bg-slate-50 px-4 text-center dark:bg-slate-950">
    <p class="text-6xl font-bold text-slate-300 dark:text-slate-700">{{ error.statusCode }}</p>
    <h1 class="text-xl font-semibold text-slate-900 dark:text-slate-100">
      {{ error.statusCode === 404 ? t('errors.notFoundTitle') : t('errors.genericTitle') }}
    </h1>
    <p class="text-slate-600 dark:text-slate-400">
      {{ error.statusCode === 404
        ? t('errors.notFoundMessage')
        : (error.statusMessage || t('errors.genericMessage')) }}
    </p>
    <button
      class="rounded-md bg-slate-900 px-5 py-2.5 text-sm font-semibold text-white hover:bg-slate-700 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-300"
      @click="goHome"
    >
      {{ t('errors.backHome') }}
    </button>
  </div>
</template>
