<script setup lang="ts">
definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Candidate')

const { getMyProfile, updateMyProfile, uploadResume } = useCandidatesApi()
const { data: profile } = await useAsyncData('my-profile', () => getMyProfile())
const { t } = useI18n()

const config = useRuntimeConfig()
const apiOrigin = (config.public.apiBase as string).replace(/\/api$/, '')

const fullName = ref(profile.value?.fullName ?? '')
const headline = ref(profile.value?.headline ?? '')
const bio = ref(profile.value?.bio ?? '')
const skills = ref(profile.value?.skills ?? '')
const resumeUrl = ref(profile.value?.resumeUrl ?? null)
const saved = ref(false)
const error = ref<string | null>(null)
const submitting = ref(false)

async function onSubmit() {
  error.value = null
  saved.value = false
  submitting.value = true
  try {
    await updateMyProfile({
      fullName: fullName.value,
      headline: headline.value || null,
      bio: bio.value || null,
      skills: skills.value || null
    })
    saved.value = true
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

    <div v-if="profile" class="flex flex-col gap-2 rounded-lg border border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-900">
      <label class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ t('dashboard.candidate.profile.resumeLabel') }}</label>
      <a
        v-if="resumeUrl" :href="`${apiOrigin}${resumeUrl}`" target="_blank" rel="noopener"
        class="text-sm text-slate-700 underline dark:text-slate-300"
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
    </div>

    <form v-if="profile" class="flex flex-col gap-4" @submit.prevent="onSubmit">
      <div class="flex flex-col gap-1">
        <label for="fullName" class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ t('dashboard.candidate.profile.fullNameLabel') }}</label>
        <input id="fullName" v-model="fullName" type="text" required class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100">
      </div>
      <div class="flex flex-col gap-1">
        <label for="headline" class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ t('dashboard.candidate.profile.headlineLabel') }}</label>
        <input id="headline" v-model="headline" type="text" :placeholder="t('dashboard.candidate.profile.headlinePlaceholder')" class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100">
      </div>
      <div class="flex flex-col gap-1">
        <label for="bio" class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ t('dashboard.candidate.profile.bioLabel') }}</label>
        <textarea id="bio" v-model="bio" rows="4" class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100" />
      </div>
      <div class="flex flex-col gap-1">
        <label for="skills" class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ t('dashboard.candidate.profile.skillsLabel') }}</label>
        <input id="skills" v-model="skills" type="text" :placeholder="t('dashboard.candidate.profile.skillsPlaceholder')" class="rounded-md border border-slate-300 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100">
      </div>

      <p v-if="error" class="text-sm text-red-600 dark:text-red-400">{{ error }}</p>
      <p v-if="saved" class="text-sm text-emerald-700 dark:text-emerald-400">{{ t('dashboard.candidate.profile.saved') }}</p>

      <button
        type="submit" :disabled="submitting"
        class="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700 disabled:opacity-50 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-300"
      >
        {{ submitting ? t('dashboard.candidate.profile.saving') : t('dashboard.candidate.profile.save') }}
      </button>
    </form>
  </div>
</template>
