<script setup lang="ts">
const { login } = useAuthApi()
const auth = useAuthStore()
const router = useRouter()
const route = useRoute()
const { t } = useI18n()
const localePath = useLocalePath()

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
    router.push(redirect || localePath(result.role === 'Employer' ? '/dashboard/employer' : '/dashboard/candidate'))
  } catch {
    error.value = t('auth.login.error')
  } finally {
    submitting.value = false
  }
}

useSeoMeta({ title: () => t('auth.login.seoTitle') })
</script>

<template>
  <div class="mx-auto flex max-w-sm flex-col gap-6 py-12">
    <h1 class="text-center text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('auth.login.heading') }}</h1>

    <Card>
      <form class="flex flex-col gap-4" @submit.prevent="onSubmit">
        <BaseInput id="email" v-model="email" type="email" required autocomplete="email" :label="t('auth.login.emailLabel')" />
        <BaseInput id="password" v-model="password" type="password" required autocomplete="current-password" :label="t('auth.login.passwordLabel')" />

        <Alert v-if="error">{{ error }}</Alert>

        <BaseButton type="submit" :loading="submitting" class="justify-center">
          {{ submitting ? t('auth.login.submitting') : t('auth.login.submit') }}
        </BaseButton>
      </form>
    </Card>

    <p class="text-center text-sm text-slate-600 dark:text-slate-400">
      {{ t('auth.login.noAccount') }} <NuxtLink :to="localePath('/register')" class="font-medium text-primary-600 underline dark:text-primary-400">{{ t('auth.login.registerLink') }}</NuxtLink>
    </p>
  </div>
</template>
