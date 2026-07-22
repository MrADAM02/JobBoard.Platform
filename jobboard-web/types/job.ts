// Mirrors JobBoard.Api's JSON shapes (camelCase via System.Text.Json defaults).
// JobType is serialized as its underlying int - keep this in sync with
// JobBoard.Domain.Enums.JobType (FullTime=0, PartTime=1, Contract=2, Internship=3, Remote=4).
export const JobType = {
  FullTime: 0,
  PartTime: 1,
  Contract: 2,
  Internship: 3,
  Remote: 4
} as const

export type JobTypeValue = (typeof JobType)[keyof typeof JobType]

export const JobTypeLabels: Record<JobTypeValue, string> = {
  [JobType.FullTime]: 'Full-time',
  [JobType.PartTime]: 'Part-time',
  [JobType.Contract]: 'Contract',
  [JobType.Internship]: 'Internship',
  [JobType.Remote]: 'Remote'
}

export interface JobListingSummary {
  id: string
  title: string
  companyName: string
  companyLogoUrl: string | null
  location: string
  isRemote: boolean
  salaryMin: number | null
  salaryMax: number | null
  jobType: JobTypeValue
  publishedAt: string | null
}

export interface JobListingDetail {
  id: string
  title: string
  description: string
  companyName: string
  companyLogoUrl: string | null
  companyWebsite: string | null
  location: string
  isRemote: boolean
  salaryMin: number | null
  salaryMax: number | null
  jobType: JobTypeValue
  tags: string | null
  publishedAt: string | null
}

export interface PaginatedList<T> {
  items: T[]
  pageNumber: number
  totalPages: number
  totalCount: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface JobListingFilters {
  keyword?: string
  location?: string
  jobType?: JobTypeValue
  remoteOnly?: boolean
  minSalary?: number
  pageNumber?: number
  pageSize?: number
}
