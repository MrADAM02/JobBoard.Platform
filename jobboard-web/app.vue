<script setup lang="ts">
// Global, not per-page: <html lang>/<html dir> must be correct on every page
// (dashboards included) for accessibility and RTL layout correctness, not just
// the SEO-critical job pages. addSeoAttributes also emits hreflang alternates
// on whichever page is currently rendering.
useHead(useLocaleHead({ addSeoAttributes: true }))
</script>

<template>
  <NuxtLayout>
    <NuxtPage />
  </NuxtLayout>
</template>
