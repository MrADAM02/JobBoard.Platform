import type { JobListingDetail, JobListingFilters, JobListingSummary, PaginatedList } from '~/types/job'

// Thin wrapper around $fetch, pointed at JobBoard.Api via runtimeConfig.public.apiBase.
// Kept framework-agnostic (no useFetch/useAsyncData here) so pages can choose how
// to call it - useAsyncData in SSR pages, direct calls in client-only interactions.
export function useJobsApi() {
  const config = useRuntimeConfig()
  const apiBase = config.public.apiBase as string

  function getJobListings(filters: JobListingFilters = {}) {
    return $fetch<PaginatedList<JobListingSummary>>(`${apiBase}/jobs`, {
      query: {
        keyword: filters.keyword || undefined,
        location: filters.location || undefined,
        jobType: filters.jobType ?? undefined,
        remoteOnly: filters.remoteOnly || undefined,
        minSalary: filters.minSalary || undefined,
        pageNumber: filters.pageNumber ?? 1,
        pageSize: filters.pageSize ?? 20
      }
    })
  }

  function getJobListingById(id: string) {
    return $fetch<JobListingDetail>(`${apiBase}/jobs/${id}`)
  }

  return { getJobListings, getJobListingById }
}
