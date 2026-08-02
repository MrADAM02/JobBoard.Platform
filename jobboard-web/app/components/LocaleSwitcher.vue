<script setup lang="ts">
// switchLocalePath preserves the current route in the target locale
// (/jobs/abc -> /ar/jobs/abc, not back to /ar) - SSR-safe, unlike building the
// URL by hand.
const { locale, locales, t } = useI18n()
const switchLocalePath = useSwitchLocalePath()

const otherLocales = computed(() => locales.value.filter((l) => l.code !== locale.value))
</script>

<template>
  <div class="flex items-center gap-1 text-sm">
    <NuxtLink
      v-for="loc in otherLocales"
      :key="loc.code"
      :to="switchLocalePath(loc.code)"
      class="rounded-md px-2 py-1 text-slate-600 hover:bg-slate-100 hover:text-slate-900 dark:text-slate-400 dark:hover:bg-slate-800 dark:hover:text-slate-100"
      :aria-label="t('locale.switchLanguage')"
    >
      {{ loc.name }}
    </NuxtLink>
  </div>
</template>
