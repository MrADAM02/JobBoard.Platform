<template>
  <div class="min-h-screen flex flex-col bg-slate-50 text-slate-900">
    <header class="border-b border-slate-200 bg-white">
      <nav class="mx-auto flex max-w-5xl items-center justify-between px-4 py-4">
        <NuxtLink to="/" class="text-lg font-semibold tracking-tight">
          JobBoard
        </NuxtLink>
        <div class="flex gap-6 text-sm font-medium text-slate-600">
          <NuxtLink to="/jobs" class="hover:text-slate-900">Browse Jobs</NuxtLink>
        </div>
      </nav>
    </header>

    <main class="mx-auto w-full max-w-5xl flex-1 px-4 py-8">
      <slot />
    </main>

    <footer class="border-t border-slate-200 bg-white py-6 text-center text-sm text-slate-500">
      JobBoard &mdash; built with ASP.NET Core &amp; Nuxt
    </footer>
  </div>
</template>
