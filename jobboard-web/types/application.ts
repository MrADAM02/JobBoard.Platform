export const ApplicationStatus = {
  Applied: 0,
  UnderReview: 1,
  InterviewScheduled: 2,
  Rejected: 3,
  Offered: 4,
  Withdrawn: 5
} as const

export type ApplicationStatusValue = (typeof ApplicationStatus)[keyof typeof ApplicationStatus]

export const ApplicationStatusLabels: Record<ApplicationStatusValue, string> = {
  [ApplicationStatus.Applied]: 'Applied',
  [ApplicationStatus.UnderReview]: 'Under Review',
  [ApplicationStatus.InterviewScheduled]: 'Interview Scheduled',
  [ApplicationStatus.Rejected]: 'Rejected',
  [ApplicationStatus.Offered]: 'Offered',
  [ApplicationStatus.Withdrawn]: 'Withdrawn'
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
