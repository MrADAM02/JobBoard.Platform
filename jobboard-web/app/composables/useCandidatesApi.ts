import type { CandidateProfile, CandidateProfilePayload } from '~/types/candidate'

export function useCandidatesApi() {
  const { authFetch } = useAuthFetch()

  function getMyProfile() {
    return authFetch<CandidateProfile>('/candidates/me')
  }

  function updateMyProfile(payload: CandidateProfilePayload) {
    return authFetch<void>('/candidates/me', { method: 'PUT', body: payload })
  }

  function uploadResume(file: File) {
    const formData = new FormData()
    formData.append('file', file)
    // No Content-Type header here - the browser sets multipart/form-data with
    // the correct boundary itself; setting it manually would drop the boundary.
    return authFetch<string>('/candidates/me/resume', { method: 'POST', body: formData })
  }

  return { getMyProfile, updateMyProfile, uploadResume }
}
