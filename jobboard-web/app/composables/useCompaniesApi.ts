import type { Company, CompanyPayload, CompanySummary } from '~/types/company'
import type { PaginatedList } from '~/types/job'
import type { EmployerAnalytics } from '~/types/analytics'

export function useCompaniesApi() {
  const { authFetch } = useAuthFetch()
  const config = useRuntimeConfig()
  const apiBase = config.public.apiBase as string

  function getCompanies(keyword?: string, pageNumber = 1, pageSize = 20) {
    return $fetch<PaginatedList<CompanySummary>>(`${apiBase}/companies`, {
      query: { keyword: keyword || undefined, pageNumber, pageSize }
    })
  }

  function getCompanyById(id: string) {
    return $fetch<Company>(`${apiBase}/companies/${id}`)
  }

  function getMyCompany() {
    return authFetch<Company | null>('/companies/mine')
  }

  function getMyAnalytics() {
    return authFetch<EmployerAnalytics | null>('/companies/mine/analytics')
  }

  function createCompany(payload: CompanyPayload) {
    return authFetch<string>('/companies', { method: 'POST', body: payload })
  }

  function updateCompany(id: string, payload: CompanyPayload) {
    return authFetch<void>(`/companies/${id}`, { method: 'PUT', body: { id, ...payload } })
  }

  function uploadLogo(id: string, file: File) {
    const formData = new FormData()
    formData.append('file', file)
    return authFetch<string>(`/companies/${id}/logo`, { method: 'POST', body: formData })
  }

  return { getCompanies, getCompanyById, getMyCompany, getMyAnalytics, createCompany, updateCompany, uploadLogo }
}
