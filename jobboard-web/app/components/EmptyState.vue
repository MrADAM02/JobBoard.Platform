<script setup lang="ts">
// Replaces each page's own hand-rolled empty-list markup.
</script>

<template>
  <div class="rounded-2xl border border-slate-200/70 bg-white p-10 text-center text-slate-500 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-400">
    <p><slot /></p>
    <div v-if="$slots.action" class="mt-3">
      <slot name="action" />
    </div>
  </div>
</template>
