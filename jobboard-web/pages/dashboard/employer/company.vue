<script setup lang="ts">
definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Employer')

const { getMyCompany, createCompany, updateCompany } = useCompaniesApi()
const router = useRouter()

const { data: existing } = await useAsyncData('my-company-edit', () => getMyCompany())

const name = ref(existing.value?.name ?? '')
const website = ref(existing.value?.website ?? '')
const description = ref(existing.value?.description ?? '')
const location = ref(existing.value?.location ?? '')
const error = ref<string | null>(null)
const submitting = ref(false)

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
