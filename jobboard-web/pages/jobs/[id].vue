<script setup lang="ts">
import { JobType, JobTypeLabels } from '~/types/job'

const route = useRoute()
const { getJobListingById } = useJobsApi()
const jobId = route.params.id as string

const { data: job } = await useAsyncData(`job-${jobId}`, () => getJobListingById(jobId))

// A soft-deleted or unknown id fails to fetch - surface Nuxt's real 404 page
// (and a real 404 HTTP status) instead of silently rendering an empty page.
if (!job.value) {
  throw createError({ statusCode: 404, statusMessage: 'Job not found', fatal: true })
}

const salaryRange = computed(() => {
  const { salaryMin, salaryMax } = job.value!
  if (!salaryMin && !salaryMax) return null
  if (salaryMin && salaryMax) return `$${salaryMin.toLocaleString()} – $${salaryMax.toLocaleString()}`
  return `$${(salaryMin ?? salaryMax)!.toLocaleString()}`
})

const employmentType = computed(() => {
  switch (job.value!.jobType) {
    case JobType.FullTime: return 'FULL_TIME'
    case JobType.PartTime: return 'PART_TIME'
    case JobType.Contract: return 'CONTRACTOR'
    case JobType.Internship: return 'INTERN'
    default: return 'OTHER'
  }
})

// useRequestURL must be called synchronously during setup - not lazily inside a
// computed getter, which unhead may resolve outside the Nuxt app context.
const pageUrl = `${useRequestURL().origin}/jobs/${jobId}`
const description = computed(
  () => `${job.value!.title} at ${job.value!.companyName} — ${job.value!.location}. ${job.value!.description.slice(0, 150)}`
)

// SEO payoff of SSR: these tags are present in the raw HTML the server sends,
// so crawlers and social-preview bots see real content without executing JS.
useSeoMeta({
  title: () => `${job.value!.title} at ${job.value!.companyName} — JobBoard`,
  description,
  ogTitle: () => `${job.value!.title} at ${job.value!.companyName}`,
  ogDescription: description,
  ogType: 'website',
  ogUrl: pageUrl
})

// Google's JobPosting rich-results schema - only meaningful because the JSON is
// in the server-rendered response, not injected client-side after the crawl.
useHead({
  script: [
    {
      type: 'application/ld+json',
      innerHTML: computed(() =>
        JSON.stringify({
          '@context': 'https://schema.org',
          '@type': 'JobPosting',
          title: job.value!.title,
          description: job.value!.description,
          datePosted: job.value!.publishedAt ?? undefined,
          employmentType: employmentType.value,
          hiringOrganization: {
            '@type': 'Organization',
            name: job.value!.companyName,
            sameAs: job.value!.companyWebsite ?? undefined
          },
          jobLocation: job.value!.isRemote
            ? undefined
            : {
                '@type': 'Place',
                address: { '@type': 'PostalAddress', addressLocality: job.value!.location }
              },
          jobLocationType: job.value!.isRemote ? 'TELECOMMUTE' : undefined,
          baseSalary:
            job.value!.salaryMin || job.value!.salaryMax
              ? {
                  '@type': 'MonetaryAmount',
                  currency: 'USD',
                  value: {
                    '@type': 'QuantitativeValue',
                    minValue: job.value!.salaryMin ?? undefined,
                    maxValue: job.value!.salaryMax ?? undefined,
                    unitText: 'YEAR'
                  }
                }
              : undefined
        })
      )
    }
  ]
})
</script>

<template>
  <article v-if="job" class="flex flex-col gap-6">
    <NuxtLink to="/jobs" class="text-sm text-slate-500 hover:text-slate-700 dark:text-slate-400 dark:hover:text-slate-200">
      <span class="inline-block rtl:-scale-x-100">&larr;</span> Back to all jobs
    </NuxtLink>

    <header class="rounded-lg border border-slate-200 bg-white p-6 dark:border-slate-700 dark:bg-slate-900">
      <div class="flex items-start justify-between gap-4">
        <div>
          <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ job.title }}</h1>
          <p class="mt-1 text-slate-600 dark:text-slate-400">{{ job.companyName }} &middot; {{ job.location }}</p>
        </div>
        <span
          v-if="job.isRemote"
          class="whitespace-nowrap rounded-full bg-emerald-100 px-3 py-1 text-xs font-medium text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-300"
        >
          Remote
        </span>
      </div>

      <div class="mt-4 flex flex-wrap gap-2 text-xs text-slate-500 dark:text-slate-400">
        <span class="rounded-full bg-slate-100 px-2.5 py-1 dark:bg-slate-800">{{ JobTypeLabels[job.jobType] }}</span>
        <span v-if="salaryRange" class="rounded-full bg-slate-100 px-2.5 py-1 dark:bg-slate-800">{{ salaryRange }}</span>
        <span v-if="job.tags" class="rounded-full bg-slate-100 px-2.5 py-1 dark:bg-slate-800">{{ job.tags }}</span>
      </div>
    </header>

    <section class="rounded-lg border border-slate-200 bg-white p-6 dark:border-slate-700 dark:bg-slate-900">
      <h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-500 dark:text-slate-400">Description</h2>
      <p class="whitespace-pre-line text-slate-700 dark:text-slate-300">{{ job.description }}</p>
    </section>

    <section v-if="job.companyWebsite" class="rounded-lg border border-slate-200 bg-white p-6 dark:border-slate-700 dark:bg-slate-900">
      <h2 class="mb-2 text-sm font-semibold uppercase tracking-wide text-slate-500 dark:text-slate-400">About {{ job.companyName }}</h2>
      <a :href="job.companyWebsite" target="_blank" rel="noopener" class="text-sm text-slate-700 underline dark:text-slate-300">
        {{ job.companyWebsite }}
      </a>
    </section>

    <!-- Client-only: apply state depends on the client-side auth store, which
         has no value during SSR. Keeping this out of the server render doesn't
         cost anything SEO-wise - it's an interactive action, not indexable content. -->
    <ClientOnly>
      <ApplyToJobBox :job-id="job.id" />
    </ClientOnly>
  </article>
</template>
