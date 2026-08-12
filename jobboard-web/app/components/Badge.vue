<script setup lang="ts">
// Replaces the ad-hoc status-pill classes repeated for job status,
// application status, and the "Remote" tag.
withDefaults(defineProps<{ variant?: 'success' | 'warning' | 'neutral' | 'info' | 'danger' }>(), {
  variant: 'neutral'
})

const variantClasses: Record<string, string> = {
  success: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300',
  warning: 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300',
  neutral: 'bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-300',
  info: 'bg-sky-100 text-sky-700 dark:bg-sky-900/40 dark:text-sky-300',
  danger: 'bg-red-100 text-red-700 dark:bg-red-900/40 dark:text-red-300'
}
</script>

<template>
  <span
    class="inline-block whitespace-nowrap rounded-full px-2.5 py-1 text-xs font-medium"
    :class="variantClasses[variant]"
  >
    <slot />
  </span>
</template>
