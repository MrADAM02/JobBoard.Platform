import type { JobListingSummary, PaginatedList } from '~/types/job'

export function useSavedJobsApi() {
  const { authFetch } = useAuthFetch()

  function getMySavedJobs(pageNumber = 1, pageSize = 20) {
    return authFetch<PaginatedList<JobListingSummary>>('/saved-jobs', { query: { pageNumber, pageSize } })
  }

  function getMySavedJobIds() {
    return authFetch<string[]>('/saved-jobs/ids')
  }

  function saveJob(jobId: string) {
    return authFetch<void>(`/saved-jobs/${jobId}`, { method: 'POST' })
  }

  function unsaveJob(jobId: string) {
    return authFetch<void>(`/saved-jobs/${jobId}`, { method: 'DELETE' })
  }

  return { getMySavedJobs, getMySavedJobIds, saveJob, unsaveJob }
}
