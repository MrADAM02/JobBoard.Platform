<script setup lang="ts">
// Polymorphic like BaseButton (`to` renders a NuxtLink) - covers both static
// panels (profile sections, form wrappers) and clickable list-item cards
// (job/company listings, dashboard overview links) with one shared style.
// Extra classes passed by the caller merge onto the rendered root as normal.
withDefaults(
  defineProps<{
    to?: string | Record<string, unknown>
    hover?: boolean
    padding?: 'sm' | 'md' | 'lg'
  }>(),
  { hover: false, padding: 'md' }
)

const paddingClasses: Record<string, string> = {
  sm: 'p-4',
  md: 'p-6',
  lg: 'p-8'
}
</script>

<template>
  <NuxtLink
    v-if="to"
    :to="to"
    class="block rounded-xl border border-slate-200 bg-white transition dark:border-slate-700 dark:bg-slate-900"
    :class="[paddingClasses[padding], hover ? 'hover:border-primary-400 hover:shadow-sm dark:hover:border-primary-500' : '']"
  >
    <slot />
  </NuxtLink>
  <div
    v-else
    class="rounded-xl border border-slate-200 bg-white dark:border-slate-700 dark:bg-slate-900"
    :class="paddingClasses[padding]"
  >
    <slot />
  </div>
</template>
