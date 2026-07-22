import type { CandidateProfile, CandidateProfilePayload } from '~/types/candidate'

export function useCandidatesApi() {
  const { authFetch } = useAuthFetch()

  function getMyProfile() {
    return authFetch<CandidateProfile>('/candidates/me')
  }

  function updateMyProfile(payload: CandidateProfilePayload) {
    return authFetch<void>('/candidates/me', { method: 'PUT', body: payload })
  }

  return { getMyProfile, updateMyProfile }
}
