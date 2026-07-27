import type { AdminJobListing, AdminUser, PlatformStats } from '~/types/admin'
import type { JobStatusValue } from '~/types/job'
import type { PaginatedList } from '~/types/job'

export function useAdminApi() {
  const { authFetch } = useAuthFetch()

  function getStats() {
    return authFetch<PlatformStats>('/admin/stats')
  }

  function getUsers(pageNumber = 1, pageSize = 20) {
    return authFetch<PaginatedList<AdminUser>>('/admin/users', { query: { pageNumber, pageSize } })
  }

  function toggleUserActive(userId: string) {
    return authFetch<void>(`/admin/users/${userId}/active`, { method: 'PUT' })
  }

  function getJobs(status?: JobStatusValue, pageNumber = 1, pageSize = 20) {
    return authFetch<PaginatedList<AdminJobListing>>('/admin/jobs', { query: { status, pageNumber, pageSize } })
  }

  function deleteJob(jobId: string) {
    return authFetch<void>(`/jobs/${jobId}`, { method: 'DELETE' })
  }

  return { getStats, getUsers, toggleUserActive, getJobs, deleteJob }
}
