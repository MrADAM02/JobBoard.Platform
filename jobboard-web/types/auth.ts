// Mirrors JobBoard.Domain.Enums.UserRole (Candidate=0, Employer=1, Admin=2).
// Self-registering as Admin is blocked server-side, so only these two are offered.
export const UserRole = {
  Candidate: 0,
  Employer: 1,
  Admin: 2
} as const

export type UserRoleValue = (typeof UserRole)[keyof typeof UserRole]

export interface AuthResult {
  accessToken: string
  refreshToken: string
  userId: string
  email: string
  role: 'Candidate' | 'Employer' | 'Admin'
}

export interface RegisterPayload {
  email: string
  password: string
  fullName: string
  role: UserRoleValue
}

export interface LoginPayload {
  email: string
  password: string
}
