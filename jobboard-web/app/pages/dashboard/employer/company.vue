<script setup lang="ts">
definePageMeta({ layout: 'dashboard', middleware: 'auth', ssr: false })
useRequireRole('Employer')

const { getMyCompany, createCompany, updateCompany, uploadLogo } = useCompaniesApi()
const router = useRouter()
const { t } = useI18n()
const localePath = useLocalePath()
const toast = useToast()

const { data: existing } = await useAsyncData('my-company-edit', () => getMyCompany())

const config = useRuntimeConfig()
const apiOrigin = (config.public.apiBase as string).replace(/\/api$/, '')

const name = ref(existing.value?.name ?? '')
const website = ref(existing.value?.website ?? '')
const description = ref(existing.value?.description ?? '')
const location = ref(existing.value?.location ?? '')
const logoUrl = ref(existing.value?.logoUrl ?? null)
const error = ref<string | null>(null)
const submitting = ref(false)

const logoUploading = ref(false)
const logoError = ref<string | null>(null)

async function onLogoChange(event: Event) {
  const file = (event.target as HTMLInputElement).files?.[0]
  if (!file || !existing.value) return

  logoError.value = null
  logoUploading.value = true
  try {
    logoUrl.value = await uploadLogo(existing.value.id, file)
    toast.success(t('dashboard.employer.company.logoUploaded'))
  } catch {
    logoError.value = t('dashboard.employer.company.logoError')
  } finally {
    logoUploading.value = false
  }
}

async function onSubmit() {
  error.value = null
  submitting.value = true
  const payload = {
    name: name.value,
    website: website.value || null,
    description: description.value || null,
    location: location.value || null
  }
  try {
    if (existing.value) {
      await updateCompany(existing.value.id, payload)
    } else {
      await createCompany(payload)
    }
    toast.success(t('dashboard.employer.company.saved'))
    router.push(localePath('/dashboard/employer'))
  } catch {
    error.value = t('dashboard.employer.company.error')
  } finally {
    submitting.value = false
  }
}

useSeoMeta({ title: () => (existing.value ? t('dashboard.employer.company.editTitle') : t('dashboard.employer.company.setupTitle')) + ' — JobBoard' })
</script>

<template>
  <div class="mx-auto flex max-w-lg flex-col gap-6 py-6">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">
      {{ existing ? t('dashboard.employer.company.editTitle') : t('dashboard.employer.company.setupTitle') }}
    </h1>

    <Card v-if="existing" padding="sm" class="flex flex-col gap-2">
      <label class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ t('dashboard.employer.company.logoLabel') }}</label>
      <img v-if="logoUrl" :src="`${apiOrigin}${logoUrl}`" alt="Company logo" class="h-16 w-16 rounded-md object-contain">
      <p v-else class="text-sm text-slate-500 dark:text-slate-400">{{ t('dashboard.employer.company.noLogo') }}</p>
      <input
        type="file" accept=".png,.jpg,.jpeg,.svg" :disabled="logoUploading"
        class="text-sm dark:text-slate-300"
        @change="onLogoChange"
      >
      <p v-if="logoUploading" class="text-xs text-slate-500 dark:text-slate-400">{{ t('dashboard.employer.company.uploading') }}</p>
      <p v-if="logoError" class="text-xs text-red-600 dark:text-red-400">{{ logoError }}</p>
    </Card>
    <p v-else class="text-sm text-slate-500 dark:text-slate-400">{{ t('dashboard.employer.company.saveFirst') }}</p>

    <Card>
      <form class="flex flex-col gap-4" @submit.prevent="onSubmit">
        <BaseInput id="name" v-model="name" required :label="t('dashboard.employer.company.nameLabel')" />
        <BaseInput id="website" v-model="website" type="url" placeholder="https://" :label="t('dashboard.employer.company.websiteLabel')" />
        <BaseInput id="location" v-model="location" :label="t('dashboard.employer.company.locationLabel')" />
        <BaseTextarea id="description" v-model="description" :rows="4" :label="t('dashboard.employer.company.descriptionLabel')" />

        <Alert v-if="error">{{ error }}</Alert>

        <BaseButton type="submit" :loading="submitting" class="justify-center">
          {{ submitting ? t('dashboard.employer.company.saving') : t('dashboard.employer.company.save') }}
        </BaseButton>
      </form>
    </Card>
  </div>
</template>
