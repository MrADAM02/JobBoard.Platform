<script setup lang="ts">
import { UserRole } from '~/types/auth'
import type { UserRoleValue } from '~/types/auth'

const { register } = useAuthApi()
const auth = useAuthStore()
const router = useRouter()
const { t } = useI18n()
const localePath = useLocalePath()

const email = ref('')
const password = ref('')
const fullName = ref('')
const role = ref<UserRoleValue>(UserRole.Candidate)
const error = ref<string | null>(null)
const submitting = ref(false)

async function onSubmit() {
  error.value = null
  submitting.value = true
  try {
    const result = await register({
      email: email.value,
      password: password.value,
      fullName: fullName.value,
      role: role.value
    })
    auth.setAuth(result)
    router.push(localePath(result.role === 'Employer' ? '/dashboard/employer' : '/dashboard/candidate'))
  } catch (err) {
    // ExceptionHandlingMiddleware shapes FluentValidation failures as an array of
    // { PropertyName, ErrorMessage } (PascalCase - it serializes with plain
    // System.Text.Json defaults, not MVC's camelCase JsonOptions), and everything
    // else (duplicate email, etc.) as { message }. These come from the backend,
    // which isn't localized (per Phase 7 scope), so they stay in English.
    const data = (err as { data?: { errors?: unknown } })?.data
    const errors = data?.errors
    error.value = Array.isArray(errors)
      ? errors.map((e: { ErrorMessage?: string }) => e.ErrorMessage).filter(Boolean).join(' ')
      : (errors as { message?: string })?.message || t('auth.register.genericError')
  } finally {
    submitting.value = false
  }
}

useSeoMeta({ title: () => t('auth.register.seoTitle') })
</script>

<template>
  <div class="mx-auto flex max-w-sm flex-col gap-6 py-12">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('auth.register.heading') }}</h1>

    <form class="flex flex-col gap-4" @submit.prevent="onSubmit">
      <div class="flex flex-col gap-1">
        <label class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ t('auth.register.iAmA') }}</label>
        <div class="flex gap-3">
          <label
            class="flex flex-1 cursor-pointer items-center justify-center rounded-md border px-3 py-2 text-sm"
            :class="role === UserRole.Candidate ? 'border-slate-900 bg-slate-900 text-white dark:border-slate-100 dark:bg-slate-100 dark:text-slate-900' : 'border-slate-300 text-slate-700 dark:border-slate-700 dark:text-slate-300'"
          >
            <input v-model.number="role" type="radio" :value="UserRole.Candidate" class="sr-only">
            {{ t('auth.register.candidate') }}
          </label>
          <label
            class="flex flex-1 cursor-pointer items-center justify-center rounded-md border px-3 py-2 text-sm"
            :class="role === UserRole.Employer ? 'border-slate-900 bg-slate-900 text-white dark:border-slate-100 dark:bg-slate-100 dark:text-slate-900' : 'border-slate-300 text-slate-700 dark:border-slate-700 dark:text-slate-300'"
          >
            <input v-model.number="role" type="radio" :value="UserRole.Employer" class="sr-only">
            {{ t('auth.register.employer') }}
          </label>
        </div>
      </div>

      <div class="flex flex-col gap-1">
        <label for="fullName" class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ t('auth.register.fullNameLabel') }}</label>
        <input
          id="fullName" v-model="fullName" type="text" required
          class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100"
        >
      </div>
      <div class="flex flex-col gap-1">
        <label for="email" class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ t('auth.register.emailLabel') }}</label>
        <input
          id="email" v-model="email" type="email" required autocomplete="email"
          class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100"
        >
      </div>
      <div class="flex flex-col gap-1">
        <label for="password" class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ t('auth.register.passwordLabel') }}</label>
        <input
          id="password" v-model="password" type="password" required minlength="8" autocomplete="new-password"
          class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100"
        >
        <p class="text-xs text-slate-500 dark:text-slate-400">{{ t('auth.register.passwordHint') }}</p>
      </div>

      <p v-if="error" class="text-sm text-red-600 dark:text-red-400">{{ error }}</p>

      <button
        type="submit" :disabled="submitting"
        class="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700 disabled:opacity-50 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-300"
      >
        {{ submitting ? t('auth.register.submitting') : t('auth.register.submit') }}
      </button>
    </form>

    <p class="text-center text-sm text-slate-600 dark:text-slate-400">
      {{ t('auth.register.haveAccount') }} <NuxtLink :to="localePath('/login')" class="font-medium text-slate-900 underline dark:text-slate-100">{{ t('auth.register.loginLink') }}</NuxtLink>
    </p>
  </div>
</template>
