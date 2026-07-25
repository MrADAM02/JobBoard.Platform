import type { Notification } from '~/types/notification'

export function useNotificationsApi() {
  const { authFetch } = useAuthFetch()

  function getMyNotifications() {
    return authFetch<Notification[]>('/notifications')
  }

  function markAsRead(id: string) {
    return authFetch<void>(`/notifications/${id}/read`, { method: 'PUT' })
  }

  return { getMyNotifications, markAsRead }
}
