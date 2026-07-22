import type {
  CreateOrUpdateJobPayload,
  JobListingDetail,
  JobListingFilters,
  JobListingSummary,
  MyJobListing,
  PaginatedList
} from '~/types/job'

// Thin wrapper around $fetch, pointed at JobBoard.Api via runtimeConfig.public.apiBase.
// Kept framework-agnostic (no useFetch/useAsyncData here) so pages can choose how
// to call it - useAsyncData in SSR pages, direct calls in client-only interactions.
export function useJobsApi() {
  const config = useRuntimeConfig()
  const apiBase = config.public.apiBase as string
  const { authFetch } = useAuthFetch()

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

  // Employer-only endpoints below - all go through authFetch.
  function getMyJobListings(pageNumber = 1, pageSize = 20) {
    return authFetch<PaginatedList<MyJobListing>>('/jobs/mine', { query: { pageNumber, pageSize } })
  }

  function createJobListing(payload: CreateOrUpdateJobPayload) {
    return authFetch<string>('/jobs', { method: 'POST', body: payload })
  }

  function updateJobListing(id: string, payload: CreateOrUpdateJobPayload) {
    return authFetch<void>(`/jobs/${id}`, { method: 'PUT', body: { id, ...payload } })
  }

  function closeJobListing(id: string) {
    return authFetch<void>(`/jobs/${id}/close`, { method: 'POST' })
  }

  function deleteJobListing(id: string) {
    return authFetch<void>(`/jobs/${id}`, { method: 'DELETE' })
  }

  return {
    getJobListings,
    getJobListingById,
    getMyJobListings,
    createJobListing,
    updateJobListing,
    closeJobListing,
    deleteJobListing
  }
}
