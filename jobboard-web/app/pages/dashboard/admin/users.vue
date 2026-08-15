<script setup lang="ts">
import { UserRoleI18nKey } from '~/types/auth'

definePageMeta({ middleware: 'auth', ssr: false })
useRequireRole('Admin')

const route = useRoute()
const router = useRouter()
const { getUsers, toggleUserActive } = useAdminApi()
const { t } = useI18n()
const localePath = useLocalePath()
const toast = useToast()
const auth = useAuthStore()

const { data, refresh } = await useAsyncData(
  `admin-users-${route.query.page ?? 1}`,
  () => getUsers(route.query.page ? Number(route.query.page) : 1),
  { watch: [() => route.query] }
)

const toggling = ref<string | null>(null)

async function onToggle(userId: string) {
  toggling.value = userId
  try {
    await toggleUserActive(userId)
    await refresh()
    toast.success(t('admin.users.toggleSuccess'))
  } catch {
    toast.error(t('admin.users.toggleError'))
  } finally {
    toggling.value = null
  }
}

function goToPage(page: number) {
  router.push({ path: localePath('/dashboard/admin/users'), query: { page: String(page) } })
}

useSeoMeta({ title: () => t('admin.users.seoTitle') })
</script>

<template>
  <div class="flex flex-col gap-6">
    <h1 class="text-2xl font-bold text-slate-900 dark:text-slate-100">{{ t('admin.users.title') }}</h1>

    <ul v-if="data?.items.length" class="flex flex-col gap-3">
      <li v-for="user in data.items" :key="user.id">
        <Card padding="sm">
          <div class="flex flex-col items-start gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h2 class="font-semibold text-slate-900 dark:text-slate-100">{{ user.email }}</h2>
              <p class="text-sm text-slate-600 dark:text-slate-400">{{ t(UserRoleI18nKey[user.role]) }}</p>
            </div>
            <div class="flex items-center gap-3">
              <Badge :variant="user.isActive ? 'success' : 'neutral'">
                {{ user.isActive ? t('admin.users.active') : t('admin.users.inactive') }}
              </Badge>
              <BaseButton
                v-if="user.id !== auth.userId"
                size="sm"
                :variant="user.isActive ? 'danger' : 'secondary'"
                :loading="toggling === user.id"
                @click="onToggle(user.id)"
              >
                {{ user.isActive ? t('admin.users.deactivate') : t('admin.users.activate') }}
              </BaseButton>
            </div>
          </div>
        </Card>
      </li>
    </ul>

    <Pagination
      v-if="data && data.totalPages > 1"
      :page-number="data.pageNumber"
      :total-pages="data.totalPages"
      :has-previous-page="data.hasPreviousPage"
      :has-next-page="data.hasNextPage"
      @change="goToPage"
    />
  </div>
</template>
