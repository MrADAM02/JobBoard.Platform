<script setup lang="ts">
// Hand-rolled HTML/CSS horizontal bar chart, not SVG - Tailwind's flex-row is
// bidi-aware (follows the inline axis), so this mirrors correctly under RTL
// for free, unlike a hand-coordinated SVG chart would.
const props = defineProps<{ data: { label: string, count: number }[] }>()

const { t } = useI18n()
const colorMode = useColorMode()

// First 6 slots of the validated 8-slot categorical theme, fixed order -
// never reassigned per render, never cycled (see the dataviz skill's palette).
const categoricalLight = ['#2a78d6', '#eb6834', '#1baf7a', '#eda100', '#e87ba4', '#008300']
const categoricalDark = ['#3987e5', '#d95926', '#199e70', '#c98500', '#d55181', '#008300']
const colors = computed(() => (colorMode.value === 'dark' ? categoricalDark : categoricalLight))

const maxCount = computed(() => Math.max(1, ...props.data.map((d) => d.count)))
function widthPercent(count: number) {
  return Math.max(2, (count / maxCount.value) * 100)
}
</script>

<template>
  <div>
    <div class="flex flex-col gap-3">
      <div v-for="(item, i) in data" :key="item.label" class="flex items-center gap-3">
        <span class="w-32 flex-shrink-0 truncate text-sm text-slate-700 dark:text-slate-300">{{ item.label }}</span>
        <div class="h-5 flex-1 rounded-full bg-slate-100 dark:bg-slate-800">
          <div
            class="h-5 rounded-e-full transition-all"
            :style="{ width: widthPercent(item.count) + '%', backgroundColor: colors[i % colors.length] }"
          />
        </div>
        <span class="w-6 flex-shrink-0 text-end text-sm font-medium text-slate-900 dark:text-slate-100">{{ item.count }}</span>
      </div>
    </div>

    <details class="mt-3 text-sm text-slate-600 dark:text-slate-400">
      <summary class="cursor-pointer select-none">{{ t('dashboard.employer.analytics.viewAsTable') }}</summary>
      <table class="mt-2 w-full text-start">
        <thead>
          <tr class="border-b border-slate-200 text-xs uppercase text-slate-500 dark:border-slate-700 dark:text-slate-400">
            <th class="py-1 text-start font-medium">{{ t('dashboard.employer.analytics.status') }}</th>
            <th class="py-1 text-start font-medium">{{ t('dashboard.employer.analytics.count') }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in data" :key="item.label" class="border-b border-slate-100 dark:border-slate-800">
            <td class="py-1">{{ item.label }}</td>
            <td class="py-1">{{ item.count }}</td>
          </tr>
        </tbody>
      </table>
    </details>
  </div>
</template>
