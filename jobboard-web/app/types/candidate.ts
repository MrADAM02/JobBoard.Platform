export interface CandidateProfile {
  id: string
  fullName: string
  headline: string | null
  bio: string | null
  resumeUrl: string | null
  skills: string | null
}

export interface CandidateProfilePayload {
  fullName: string
  headline: string | null
  bio: string | null
  skills: string | null
}
