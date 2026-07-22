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

export const JobStatus = {
  Draft: 0,
  Published: 1,
  Closed: 2,
  Expired: 3,
  Deleted: 4
} as const

export type JobStatusValue = (typeof JobStatus)[keyof typeof JobStatus]

export const JobStatusLabels: Record<JobStatusValue, string> = {
  [JobStatus.Draft]: 'Draft',
  [JobStatus.Published]: 'Published',
  [JobStatus.Closed]: 'Closed',
  [JobStatus.Expired]: 'Expired',
  [JobStatus.Deleted]: 'Deleted'
}

export interface MyJobListing {
  id: string
  title: string
  location: string
  isRemote: boolean
  status: JobStatusValue
  viewCount: number
  applicationCount: number
  publishedAt: string | null
  createdAt: string
}

export interface CreateOrUpdateJobPayload {
  companyId?: string
  title: string
  description: string
  location: string
  isRemote: boolean
  salaryMin: number | null
  salaryMax: number | null
  jobType: JobTypeValue
  tags: string | null
  publishImmediately?: boolean
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
