export interface Company {
  id: string
  name: string
  logoUrl: string | null
  website: string | null
  description: string | null
  location: string | null
}

export interface CompanyPayload {
  name: string
  website: string | null
  description: string | null
  location: string | null
}

export interface CompanySummary {
  id: string
  name: string
  logoUrl: string | null
  location: string | null
  website: string | null
  openJobCount: number
}
