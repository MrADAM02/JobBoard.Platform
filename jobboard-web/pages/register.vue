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
    <h1 class="text-center text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('auth.register.heading') }}</h1>

    <Card>
      <form class="flex flex-col gap-4" @submit.prevent="onSubmit">
        <div class="flex flex-col gap-1">
          <label class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ t('auth.register.iAmA') }}</label>
          <div class="flex gap-3">
            <label
              class="flex flex-1 cursor-pointer items-center justify-center rounded-lg border px-3 py-2 text-sm"
              :class="role === UserRole.Candidate ? 'border-primary-600 bg-primary-600 text-white' : 'border-slate-300 text-slate-700 dark:border-slate-700 dark:text-slate-300'"
            >
              <input v-model.number="role" type="radio" :value="UserRole.Candidate" class="sr-only">
              {{ t('auth.register.candidate') }}
            </label>
            <label
              class="flex flex-1 cursor-pointer items-center justify-center rounded-lg border px-3 py-2 text-sm"
              :class="role === UserRole.Employer ? 'border-primary-600 bg-primary-600 text-white' : 'border-slate-300 text-slate-700 dark:border-slate-700 dark:text-slate-300'"
            >
              <input v-model.number="role" type="radio" :value="UserRole.Employer" class="sr-only">
              {{ t('auth.register.employer') }}
            </label>
          </div>
        </div>

        <BaseInput id="fullName" v-model="fullName" type="text" required :label="t('auth.register.fullNameLabel')" />
        <BaseInput id="email" v-model="email" type="email" required autocomplete="email" :label="t('auth.register.emailLabel')" />
        <BaseInput
          id="password" v-model="password" type="password" required :minlength="8" autocomplete="new-password"
          :label="t('auth.register.passwordLabel')" :hint="t('auth.register.passwordHint')"
        />

        <Alert v-if="error">{{ error }}</Alert>

        <BaseButton type="submit" :loading="submitting" class="justify-center">
          {{ submitting ? t('auth.register.submitting') : t('auth.register.submit') }}
        </BaseButton>
      </form>
    </Card>

    <p class="text-center text-sm text-slate-600 dark:text-slate-400">
      {{ t('auth.register.haveAccount') }} <NuxtLink :to="localePath('/login')" class="font-medium text-primary-600 underline dark:text-primary-400">{{ t('auth.register.loginLink') }}</NuxtLink>
    </p>
  </div>
</template>
