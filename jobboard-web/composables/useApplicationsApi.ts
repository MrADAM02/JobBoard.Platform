import type { ApplicationStatusValue, ApplicationSummary, MyApplication } from '~/types/application'

export function useApplicationsApi() {
  const { authFetch } = useAuthFetch()

  function applyToJob(jobListingId: string, coverLetter: string | null) {
    return authFetch<string>('/applications', { method: 'POST', body: { jobListingId, coverLetter } })
  }

  function getMyApplications() {
    return authFetch<MyApplication[]>('/applications/mine')
  }

  function getApplicationsForJob(jobListingId: string) {
    return authFetch<ApplicationSummary[]>(`/applications/job/${jobListingId}`)
  }

  function updateApplicationStatus(applicationId: string, newStatus: ApplicationStatusValue) {
    return authFetch<void>(`/applications/${applicationId}/status`, {
      method: 'PUT',
      body: { applicationId, newStatus }
    })
  }

  return { applyToJob, getMyApplications, getApplicationsForJob, updateApplicationStatus }
}
