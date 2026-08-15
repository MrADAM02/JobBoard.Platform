<script setup lang="ts">
definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Candidate')

const { getMyProfile, updateMyProfile, uploadResume } = useCandidatesApi()
const { data: profile } = await useAsyncData('my-profile', () => getMyProfile())
const { t } = useI18n()
const toast = useToast()

const config = useRuntimeConfig()
const apiOrigin = (config.public.apiBase as string).replace(/\/api$/, '')

const fullName = ref(profile.value?.fullName ?? '')
const headline = ref(profile.value?.headline ?? '')
const bio = ref(profile.value?.bio ?? '')
const skills = ref(profile.value?.skills ?? '')
const resumeUrl = ref(profile.value?.resumeUrl ?? null)
const error = ref<string | null>(null)
const submitting = ref(false)

async function onSubmit() {
  error.value = null
  submitting.value = true
  try {
    await updateMyProfile({
      fullName: fullName.value,
      headline: headline.value || null,
      bio: bio.value || null,
      skills: skills.value || null
    })
    toast.success(t('dashboard.candidate.profile.saved'))
  } catch {
    error.value = t('dashboard.candidate.profile.error')
  } finally {
    submitting.value = false
  }
}

const resumeUploading = ref(false)
const resumeError = ref<string | null>(null)

async function onResumeChange(event: Event) {
  const file = (event.target as HTMLInputElement).files?.[0]
  if (!file) return

  resumeError.value = null
  resumeUploading.value = true
  try {
    resumeUrl.value = await uploadResume(file)
    toast.success(t('dashboard.candidate.profile.resumeUploaded'))
  } catch {
    resumeError.value = t('dashboard.candidate.profile.resumeError')
  } finally {
    resumeUploading.value = false
  }
}

useSeoMeta({ title: () => t('dashboard.candidate.profile.seoTitle') })
</script>

<template>
  <div class="mx-auto flex max-w-lg flex-col gap-6 py-6">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('dashboard.candidate.profile.title') }}</h1>

    <Card v-if="profile" padding="sm" class="flex flex-col gap-2">
      <label class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ t('dashboard.candidate.profile.resumeLabel') }}</label>
      <a
        v-if="resumeUrl" :href="`${apiOrigin}${resumeUrl}`" target="_blank" rel="noopener"
        class="text-sm text-primary-600 underline dark:text-primary-400"
      >
        {{ t('dashboard.candidate.profile.viewResume') }}
      </a>
      <p v-else class="text-sm text-slate-500 dark:text-slate-400">{{ t('dashboard.candidate.profile.noResume') }}</p>
      <input
        type="file" accept=".pdf,.doc,.docx" :disabled="resumeUploading"
        class="text-sm dark:text-slate-300"
        @change="onResumeChange"
      >
      <p v-if="resumeUploading" class="text-xs text-slate-500 dark:text-slate-400">{{ t('dashboard.candidate.profile.uploading') }}</p>
      <p v-if="resumeError" class="text-xs text-red-600 dark:text-red-400">{{ resumeError }}</p>
    </Card>

    <Card v-if="profile">
      <form class="flex flex-col gap-4" @submit.prevent="onSubmit">
        <BaseInput id="fullName" v-model="fullName" required :label="t('dashboard.candidate.profile.fullNameLabel')" />
        <BaseInput id="headline" v-model="headline" :placeholder="t('dashboard.candidate.profile.headlinePlaceholder')" :label="t('dashboard.candidate.profile.headlineLabel')" />
        <BaseTextarea id="bio" v-model="bio" :rows="4" :label="t('dashboard.candidate.profile.bioLabel')" />
        <BaseInput id="skills" v-model="skills" :placeholder="t('dashboard.candidate.profile.skillsPlaceholder')" :label="t('dashboard.candidate.profile.skillsLabel')" />

        <Alert v-if="error">{{ error }}</Alert>

        <BaseButton type="submit" :loading="submitting" class="justify-center">
          {{ submitting ? t('dashboard.candidate.profile.saving') : t('dashboard.candidate.profile.save') }}
        </BaseButton>
      </form>
    </Card>
  </div>
</template>
