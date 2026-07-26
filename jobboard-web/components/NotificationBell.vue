<script setup lang="ts">
import type { Notification } from '~/types/notification'

const { t } = useI18n()
const { getMyNotifications, markAsRead } = useNotificationsApi()

const notifications = ref<Notification[]>([])
const open = ref(false)

const unreadCount = computed(() => notifications.value.filter((n) => !n.isRead).length)

onMounted(async () => {
  try {
    notifications.value = await getMyNotifications()
  } catch {
    // non-fatal - the bell just stays empty if this fails
  }
})

async function onItemClick(notification: Notification) {
  if (notification.isRead) return
  notification.isRead = true
  try {
    await markAsRead(notification.id)
  } catch {
    notification.isRead = false
  }
}
</script>

<template>
  <div class="relative">
    <button
      type="button"
      class="relative rounded-md p-1.5 text-slate-600 hover:bg-slate-100 hover:text-slate-900 dark:text-slate-400 dark:hover:bg-slate-800 dark:hover:text-slate-100"
      :aria-label="t('notifications.title')"
      @click="open = !open"
    >
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="h-5 w-5">
        <path d="M18 8a6 6 0 0 0-12 0c0 7-3 9-3 9h18s-3-2-3-9" />
        <path d="M13.73 21a2 2 0 0 1-3.46 0" />
      </svg>
      <span
        v-if="unreadCount > 0"
        class="absolute -end-1 -top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-semibold text-white"
      >
        {{ unreadCount }}
      </span>
    </button>

    <div v-if="open" class="fixed inset-0 z-10" @click="open = false" />

    <div
      v-if="open"
      class="absolute end-0 z-20 mt-2 w-72 max-w-[calc(100vw-2rem)] rounded-lg border border-slate-200 bg-white shadow-lg dark:border-slate-700 dark:bg-slate-900 sm:w-80"
    >
      <div class="border-b border-slate-200 px-4 py-2 text-sm font-semibold text-slate-900 dark:border-slate-700 dark:text-slate-100">
        {{ t('notifications.title') }}
      </div>
      <div class="max-h-80 overflow-y-auto">
        <p v-if="notifications.length === 0" class="px-4 py-6 text-center text-sm text-slate-500 dark:text-slate-400">
          {{ t('notifications.empty') }}
        </p>
        <button
          v-for="notification in notifications"
          :key="notification.id"
          type="button"
          class="block w-full border-b border-slate-100 px-4 py-3 text-start text-sm last:border-0 hover:bg-slate-50 dark:border-slate-800 dark:hover:bg-slate-800"
          @click="onItemClick(notification)"
        >
          <span class="flex items-start gap-2">
            <span
              class="mt-1.5 h-2 w-2 flex-shrink-0 rounded-full"
              :class="notification.isRead ? 'bg-transparent' : 'bg-blue-500'"
            />
            <span class="text-slate-700 dark:text-slate-300">{{ notification.message }}</span>
          </span>
        </button>
      </div>
    </div>
  </div>
</template>
