<script setup lang="ts">
// Small icon set for DashboardSidebar nav items - kept separate from the
// one-off inline <svg>s elsewhere since "grid"/"document"/etc. are each
// reused across all three role sidebars (candidate/employer/admin).
defineProps<{
  name: 'grid' | 'document' | 'bookmark' | 'building' | 'chart' | 'people' | 'settings'
}>()
</script>

<template>
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="h-5 w-5">
    <template v-if="name === 'grid'">
      <rect x="3.5" y="3.5" width="7" height="7" rx="1.5" />
      <rect x="13.5" y="3.5" width="7" height="7" rx="1.5" />
      <rect x="3.5" y="13.5" width="7" height="7" rx="1.5" />
      <rect x="13.5" y="13.5" width="7" height="7" rx="1.5" />
    </template>
    <path v-else-if="name === 'document'" stroke-linecap="round" stroke-linejoin="round" d="M7 3.5h7l4 4V20a1 1 0 0 1-1 1H7a1 1 0 0 1-1-1V4.5a1 1 0 0 1 1-1Zm7 0V8h4M9 12h6M9 16h6" />
    <path v-else-if="name === 'bookmark'" stroke-linecap="round" stroke-linejoin="round" d="M6 4a2 2 0 0 0-2 2v14l8-5 8 5V6a2 2 0 0 0-2-2H6z" />
    <path v-else-if="name === 'building'" stroke-linecap="round" stroke-linejoin="round" d="M4 21V6a1 1 0 0 1 1-1h8a1 1 0 0 1 1 1v15M4 21h16M9 21v-4h4v4M8 8h1M8 12h1M12 8h1M12 12h1M14 21v-8h5a1 1 0 0 1 1 1v7" />
    <path v-else-if="name === 'chart'" stroke-linecap="round" stroke-linejoin="round" d="M4 20V10m6 10V4m6 16v-7" />
    <path v-else-if="name === 'people'" stroke-linecap="round" stroke-linejoin="round" d="M9 11a3 3 0 1 0 0-6 3 3 0 0 0 0 6ZM3 20c0-3.3 2.7-6 6-6s6 2.7 6 6M17 11a3 3 0 1 0 0-6M15 14c2.8.3 5 2.7 5 6" />
    <template v-else-if="name === 'settings'">
      <circle cx="12" cy="12" r="3" />
      <path stroke-linecap="round" stroke-linejoin="round" d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 1 1-4 0v-.09a1.65 1.65 0 0 0-1-1.51 1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 1 1 0-4h.09a1.65 1.65 0 0 0 1.51-1 1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 1 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 1 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1Z" />
    </template>
  </svg>
</template>
