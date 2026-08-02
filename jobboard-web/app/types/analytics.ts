import type { ApplicationStatusValue } from '~/types/application'

export interface DailyViews {
  date: string
  count: number
}

export interface StatusCount {
  status: ApplicationStatusValue
  count: number
}

export interface TopJob {
  id: string
  title: string
  viewCount: number
  applicationCount: number
}

export interface EmployerAnalytics {
  totalViews: number
  totalApplications: number
  totalJobsPosted: number
  viewsByDay: DailyViews[]
  applicationsByStatus: StatusCount[]
  topJobs: TopJob[]
}
