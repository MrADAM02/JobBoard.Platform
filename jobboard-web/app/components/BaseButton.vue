<script setup lang="ts">
// Polymorphic: renders a NuxtLink when `to` is given, a <button> otherwise -
// one shared style so CTA links (Post a Job, Browse Jobs) and real submit
// buttons look identical instead of each page hand-rolling its own classes.
const props = withDefaults(
  defineProps<{
    variant?: 'primary' | 'secondary' | 'ghost' | 'danger'
    size?: 'sm' | 'md'
    to?: string | Record<string, unknown>
    type?: 'button' | 'submit'
    disabled?: boolean
    loading?: boolean
  }>(),
  { variant: 'primary', size: 'md', type: 'button', disabled: false, loading: false }
)

const variantClasses: Record<string, string> = {
  primary: 'bg-primary-600 text-white hover:bg-primary-700 dark:bg-primary-500 dark:hover:bg-primary-400',
  secondary:
    'border border-slate-300 text-slate-700 hover:bg-slate-50 dark:border-slate-700 dark:text-slate-200 dark:hover:bg-slate-800',
  ghost: 'text-slate-600 hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-slate-800',
  danger: 'bg-red-600 text-white hover:bg-red-700 dark:bg-red-500 dark:hover:bg-red-400'
}

const sizeClasses: Record<string, string> = {
  sm: 'px-3 py-1.5 text-sm',
  md: 'px-4 py-2 text-sm'
}

const classes = computed(() => [
  'inline-flex items-center justify-center gap-2 rounded-xl font-semibold transition-colors disabled:cursor-not-allowed disabled:opacity-50',
  variantClasses[props.variant],
  sizeClasses[props.size]
])

const isDisabled = computed(() => props.disabled || props.loading)
</script>

<template>
  <NuxtLink v-if="to" :to="to" :class="classes">
    <Spinner v-if="loading" />
    <slot />
  </NuxtLink>
  <button v-else :type="type" :disabled="isDisabled" :class="classes">
    <Spinner v-if="loading" />
    <slot />
  </button>
</template>
