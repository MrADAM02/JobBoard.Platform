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

export const JobTypeI18nKey: Record<JobTypeValue, string> = {
  [JobType.FullTime]: 'jobs.type.fullTime',
  [JobType.PartTime]: 'jobs.type.partTime',
  [JobType.Contract]: 'jobs.type.contract',
  [JobType.Internship]: 'jobs.type.internship',
  [JobType.Remote]: 'jobs.type.remote'
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

export const JobStatusI18nKey: Record<JobStatusValue, string> = {
  [JobStatus.Draft]: 'jobs.status.draft',
  [JobStatus.Published]: 'jobs.status.published',
  [JobStatus.Closed]: 'jobs.status.closed',
  [JobStatus.Expired]: 'jobs.status.expired',
  [JobStatus.Deleted]: 'jobs.status.deleted'
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
