<script setup lang="ts">
// useColorMode() reads the cookie server-side, so this renders correctly in
// the initial SSR HTML - unlike auth state, it does not need <ClientOnly>.
const colorMode = useColorMode()
const { t } = useI18n()

function toggle() {
  colorMode.preference = colorMode.value === 'dark' ? 'light' : 'dark'
}
</script>

<template>
  <button
    type="button"
    class="rounded-md p-1.5 text-slate-600 hover:bg-slate-100 hover:text-slate-900 dark:text-slate-400 dark:hover:bg-slate-800 dark:hover:text-slate-100"
    :aria-label="colorMode.value === 'dark' ? t('theme.switchToLight') : t('theme.switchToDark')"
    @click="toggle"
  >
    <!-- Both icons always render (never a v-if on colorMode.value) - visibility
         toggles via the dark: class instead. colorMode.value can resolve
         differently on the client than the server-rendered fallback (e.g. no
         cookie yet + an OS-level dark preference), and swapping which <svg>
         exists in the DOM would be a hydration mismatch; swapping which one
         is display:none is not. -->
    <svg class="block h-5 w-5 dark:hidden" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
      <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" />
    </svg>
    <svg class="hidden h-5 w-5 dark:block" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
      <circle cx="12" cy="12" r="4" />
      <path d="M12 2v2M12 20v2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M2 12h2M20 12h2M4.93 19.07l1.41-1.41M17.66 6.34l1.41-1.41" />
    </svg>
  </button>
</template>
