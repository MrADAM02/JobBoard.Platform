<script setup lang="ts">
definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Candidate')

const { getMyProfile, updateMyProfile } = useCandidatesApi()
const { data: profile } = await useAsyncData('my-profile', () => getMyProfile())

const fullName = ref(profile.value?.fullName ?? '')
const headline = ref(profile.value?.headline ?? '')
const bio = ref(profile.value?.bio ?? '')
const skills = ref(profile.value?.skills ?? '')
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
    error.value = 'Could not save your profile. Please check your details.'
  } finally {
    submitting.value = false
  }
}

useSeoMeta({ title: 'My Profile — JobBoard' })
</script>

<template>
  <div class="mx-auto flex max-w-lg flex-col gap-6 py-6">
    <h1 class="text-2xl font-bold text-slate-900">My Profile</h1>

    <form v-if="profile" class="flex flex-col gap-4" @submit.prevent="onSubmit">
      <div class="flex flex-col gap-1">
        <label for="fullName" class="text-sm font-medium text-slate-700">Full name</label>
        <input id="fullName" v-model="fullName" type="text" required class="rounded-md border border-slate-300 px-3 py-2 text-sm">
      </div>
      <div class="flex flex-col gap-1">
        <label for="headline" class="text-sm font-medium text-slate-700">Headline</label>
        <input id="headline" v-model="headline" type="text" placeholder="Senior Backend Engineer" class="rounded-md border border-slate-300 px-3 py-2 text-sm">
      </div>
      <div class="flex flex-col gap-1">
        <label for="bio" class="text-sm font-medium text-slate-700">Bio</label>
        <textarea id="bio" v-model="bio" rows="4" class="rounded-md border border-slate-300 px-3 py-2 text-sm" />
      </div>
      <div class="flex flex-col gap-1">
        <label for="skills" class="text-sm font-medium text-slate-700">Skills (comma-separated)</label>
        <input id="skills" v-model="skills" type="text" placeholder="C#, Vue, PostgreSQL" class="rounded-md border border-slate-300 px-3 py-2 text-sm">
      </div>

      <p v-if="error" class="text-sm text-red-600">{{ error }}</p>
      <p v-if="saved" class="text-sm text-emerald-700">Profile saved.</p>

      <button
        type="submit" :disabled="submitting"
        class="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700 disabled:opacity-50"
      >
        {{ submitting ? 'Saving…' : 'Save' }}
      </button>
    </form>
  </div>
</template>
