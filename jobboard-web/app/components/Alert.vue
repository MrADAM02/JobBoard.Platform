<script setup lang="ts">
// Replaces the repeated `<p v-if="error" class="text-sm text-red-600...">`
// pattern with a boxed, higher-visibility alert - also generalizes the
// one-off amber "set up your company first" box that already existed.
withDefaults(defineProps<{ variant?: 'error' | 'success' | 'warning' | 'info' }>(), { variant: 'error' })

const variantClasses: Record<string, string> = {
  error: 'border-red-200 bg-red-50 text-red-700 dark:border-red-900 dark:bg-red-950/40 dark:text-red-300',
  success:
    'border-emerald-200 bg-emerald-50 text-emerald-700 dark:border-emerald-900 dark:bg-emerald-950/40 dark:text-emerald-300',
  warning: 'border-amber-200 bg-amber-50 text-amber-800 dark:border-amber-900 dark:bg-amber-950/40 dark:text-amber-300',
  info: 'border-sky-200 bg-sky-50 text-sky-700 dark:border-sky-900 dark:bg-sky-950/40 dark:text-sky-300'
}
</script>

<template>
  <div class="rounded-lg border px-4 py-3 text-sm" :class="variantClasses[variant]">
    <slot />
  </div>
</template>
