export const ApplicationStatus = {
  Applied: 0,
  UnderReview: 1,
  InterviewScheduled: 2,
  Rejected: 3,
  Offered: 4,
  Withdrawn: 5
} as const

export type ApplicationStatusValue = (typeof ApplicationStatus)[keyof typeof ApplicationStatus]

export const ApplicationStatusI18nKey: Record<ApplicationStatusValue, string> = {
  [ApplicationStatus.Applied]: 'applicationStatus.applied',
  [ApplicationStatus.UnderReview]: 'applicationStatus.underReview',
  [ApplicationStatus.InterviewScheduled]: 'applicationStatus.interviewScheduled',
  [ApplicationStatus.Rejected]: 'applicationStatus.rejected',
  [ApplicationStatus.Offered]: 'applicationStatus.offered',
  [ApplicationStatus.Withdrawn]: 'applicationStatus.withdrawn'
}

export interface MyApplication {
  id: string
  jobListingId: string
  jobTitle: string
  companyName: string
  status: ApplicationStatusValue
  appliedAt: string
}

export interface ApplicationSummary {
  id: string
  candidateName: string
  resumeUrl: string | null
  status: ApplicationStatusValue
  appliedAt: string
}
