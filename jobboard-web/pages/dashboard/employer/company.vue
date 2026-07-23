<script setup lang="ts">
definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Employer')

const { getMyCompany, createCompany, updateCompany, uploadLogo } = useCompaniesApi()
const router = useRouter()

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
  } catch {
    logoError.value = 'Could not upload logo. Use a PNG, JPEG, or SVG under 2 MB.'
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
    router.push('/dashboard/employer')
  } catch {
    error.value = 'Could not save company profile. Please check your details.'
  } finally {
    submitting.value = false
  }
}

useSeoMeta({ title: 'Company Profile — JobBoard' })
</script>

<template>
  <div class="mx-auto flex max-w-lg flex-col gap-6 py-6">
    <h1 class="text-2xl font-bold text-slate-900">
      {{ existing ? 'Edit company profile' : 'Set up your company profile' }}
    </h1>

    <div v-if="existing" class="flex flex-col gap-2 rounded-lg border border-slate-200 bg-white p-4">
      <label class="text-sm font-medium text-slate-700">Logo</label>
      <img v-if="logoUrl" :src="`${apiOrigin}${logoUrl}`" alt="Company logo" class="h-16 w-16 rounded-md object-contain">
      <p v-else class="text-sm text-slate-500">No logo uploaded yet.</p>
      <input
        type="file" accept=".png,.jpg,.jpeg,.svg" :disabled="logoUploading"
        class="text-sm"
        @change="onLogoChange"
      >
      <p v-if="logoUploading" class="text-xs text-slate-500">Uploading…</p>
      <p v-if="logoError" class="text-xs text-red-600">{{ logoError }}</p>
    </div>
    <p v-else class="text-sm text-slate-500">Save your company profile first, then you can upload a logo.</p>

    <form class="flex flex-col gap-4" @submit.prevent="onSubmit">
      <div class="flex flex-col gap-1">
        <label for="name" class="text-sm font-medium text-slate-700">Company name</label>
        <input
          id="name" v-model="name" type="text" required
          class="rounded-md border border-slate-300 px-3 py-2 text-sm"
        >
      </div>
      <div class="flex flex-col gap-1">
        <label for="website" class="text-sm font-medium text-slate-700">Website</label>
        <input
          id="website" v-model="website" type="url" placeholder="https://"
          class="rounded-md border border-slate-300 px-3 py-2 text-sm"
        >
      </div>
      <div class="flex flex-col gap-1">
        <label for="location" class="text-sm font-medium text-slate-700">Location</label>
        <input
          id="location" v-model="location" type="text"
          class="rounded-md border border-slate-300 px-3 py-2 text-sm"
        >
      </div>
      <div class="flex flex-col gap-1">
        <label for="description" class="text-sm font-medium text-slate-700">Description</label>
        <textarea
          id="description" v-model="description" rows="4"
          class="rounded-md border border-slate-300 px-3 py-2 text-sm"
        />
      </div>

      <p v-if="error" class="text-sm text-red-600">{{ error }}</p>

      <button
        type="submit" :disabled="submitting"
        class="rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-700 disabled:opacity-50"
      >
        {{ submitting ? 'Saving…' : 'Save' }}
      </button>
    </form>
  </div>
</template>
