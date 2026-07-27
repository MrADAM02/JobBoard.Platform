import type { UserRoleValue } from '~/types/auth'
import type { JobStatusValue } from '~/types/job'

export interface PlatformStats {
  totalUsers: number
  totalCandidates: number
  totalEmployers: number
  totalCompanies: number
  totalJobListings: number
  totalApplications: number
}

export interface AdminUser {
  id: string
  email: string
  role: UserRoleValue
  isActive: boolean
  createdAt: string
}

export interface AdminJobListing {
  id: string
  title: string
  companyName: string
  status: JobStatusValue
  viewCount: number
  applicationCount: number
  createdAt: string
}
