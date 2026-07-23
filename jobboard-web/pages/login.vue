<script setup lang="ts">
const { login } = useAuthApi()
const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const email = ref('')
const password = ref('')
const error = ref<string | null>(null)
const submitting = ref(false)

async function onSubmit() {
  error.value = null
  submitting.value = true
  try {
    const result = await login({ email: email.value, password: password.value })
    auth.setAuth(result)
    const redirect = route.query.redirect as string | undefined
    router.push(redirect || (result.role === 'Employer' ? '/dashboard/employer' : '/dashboard/candidate'))
  } catch {
    error.value = 'Invalid email or password.'
  } finally {
    submitting.value = false
  }
}

useSeoMeta({ title: 'Log in — JobBoard' })
</script>

<template>
  <div class="mx-auto flex max-w-sm flex-col gap-6 py-12">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">Log in</h1>

    <form class="flex flex-col gap-4" @submit.prevent="onSubmit">
      <div class="flex flex-col gap-1">
        <label for="email" class="text-sm font-medium text-slate-700 dark:text-slate-300">Email</label>
        <input
          id="email" v-model="email" type="email" required autocomplete="email"
          class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100"
        >
      </div>
      <div class="flex flex-col gap-1">
        <label for="password" class="text-sm font-medium text-slate-700 dark:text-slate-300">Password</label>
        <input
          id="password" v-model="password" type="password" required autocomplete="current-password"
          class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100"
        >
      </div>

      <p v-if="error" class="text-sm text-red-600 dark:text-red-400">{{ error }}</p>

      <button
        type="submit" :disabled="submitting"
        class="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700 disabled:opacity-50 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-300"
      >
        {{ submitting ? 'Logging in…' : 'Log in' }}
      </button>
    </form>

    <p class="text-center text-sm text-slate-600 dark:text-slate-400">
      Don't have an account? <NuxtLink to="/register" class="font-medium text-slate-900 underline dark:text-slate-100">Register</NuxtLink>
    </p>
  </div>
</template>
