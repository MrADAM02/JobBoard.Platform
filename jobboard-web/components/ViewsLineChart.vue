<script setup lang="ts">
// Hand-rolled SVG line chart (no charting library, matches the app's existing
// zero-extra-dependency pattern). Time always runs left-to-right regardless of
// locale direction - the standard charting convention for chronological axes,
// even in RTL UIs (Google Analytics, etc. do the same) - so this is
// deliberately NOT mirrored under rtl, unlike the rest of the app's icons.
const props = defineProps<{ data: { date: string, count: number }[] }>()

const { t } = useI18n()
const colorMode = useColorMode()

// Single-hue sequential color (dataviz skill's default "blue" sequential
// hue) - validated CVD-safe pair for light/dark chart surfaces.
const stroke = computed(() => (colorMode.value === 'dark' ? '#3987e5' : '#2a78d6'))

const width = 600
const height = 180
const padding = { top: 12, right: 12, bottom: 24, left: 32 }
const plotWidth = width - padding.left - padding.right
const plotHeight = height - padding.top - padding.bottom

const maxCount = computed(() => Math.max(1, ...props.data.map((d) => d.count)))

const points = computed(() =>
  props.data.map((d, i) => {
    const x = padding.left + (i / Math.max(1, props.data.length - 1)) * plotWidth
    const y = padding.top + plotHeight - (d.count / maxCount.value) * plotHeight
    return { x, y, ...d }
  })
)

const linePath = computed(() =>
  points.value.map((p, i) => `${i === 0 ? 'M' : 'L'}${p.x.toFixed(1)},${p.y.toFixed(1)}`).join(' ')
)

// Area wash under the line - series hue at ~10% opacity, closed down to the baseline.
const areaPath = computed(() => {
  if (!points.value.length) return ''
  const baseline = padding.top + plotHeight
  const first = points.value[0]
  const last = points.value[points.value.length - 1]
  return `${linePath.value} L${last.x.toFixed(1)},${baseline} L${first.x.toFixed(1)},${baseline} Z`
})

const lastPoint = computed(() => points.value[points.value.length - 1])

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })
}
</script>

<template>
  <div>
    <svg :viewBox="`0 0 ${width} ${height}`" class="w-full" role="img" :aria-label="t('dashboard.employer.analytics.viewsChartLabel')">
      <!-- baseline (recessive hairline, matches the app's existing border tokens) -->
      <line
        :x1="padding.left" :y1="padding.top + plotHeight" :x2="width - padding.right" :y2="padding.top + plotHeight"
        class="stroke-slate-200 dark:stroke-slate-700" stroke-width="1"
      />

      <!-- area wash -->
      <path :d="areaPath" :fill="stroke" fill-opacity="0.1" stroke="none" />

      <!-- line -->
      <path :d="linePath" fill="none" :stroke="stroke" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />

      <!-- hover targets with native tooltips - lightweight substitute for a full custom tooltip layer -->
      <circle
        v-for="p in points" :key="p.date" :cx="p.x" :cy="p.y" r="6" fill="transparent"
      >
        <title>{{ formatDate(p.date) }}: {{ p.count }}</title>
      </circle>

      <!-- end marker + direct label, per spec ("lines carry their value at the end") -->
      <template v-if="lastPoint">
        <circle :cx="lastPoint.x" :cy="lastPoint.y" r="4" :fill="stroke" stroke="currentColor" stroke-width="2" class="text-white dark:text-slate-900" />
        <text :x="lastPoint.x - 8" :y="lastPoint.y - 10" text-anchor="end" class="fill-slate-700 text-[11px] font-medium dark:fill-slate-300">
          {{ lastPoint.count }}
        </text>
      </template>

      <!-- minimal x-axis: first/last date only, avoids label collision -->
      <text v-if="points[0]" :x="points[0].x" :y="height - 6" text-anchor="start" class="fill-slate-500 text-[10px] dark:fill-slate-400">
        {{ formatDate(points[0].date) }}
      </text>
      <text v-if="lastPoint" :x="lastPoint.x" :y="height - 6" text-anchor="end" class="fill-slate-500 text-[10px] dark:fill-slate-400">
        {{ formatDate(lastPoint.date) }}
      </text>

      <!-- minimal y-axis: 0 and max only -->
      <text :x="padding.left - 6" :y="padding.top + plotHeight" text-anchor="end" class="fill-slate-500 text-[10px] dark:fill-slate-400">0</text>
      <text :x="padding.left - 6" :y="padding.top + 8" text-anchor="end" class="fill-slate-500 text-[10px] dark:fill-slate-400">{{ maxCount }}</text>
    </svg>

    <details class="mt-2 text-sm text-slate-600 dark:text-slate-400">
      <summary class="cursor-pointer select-none">{{ t('dashboard.employer.analytics.viewAsTable') }}</summary>
      <table class="mt-2 w-full text-start">
        <thead>
          <tr class="border-b border-slate-200 text-xs uppercase text-slate-500 dark:border-slate-700 dark:text-slate-400">
            <th class="py-1 text-start font-medium">{{ t('dashboard.employer.analytics.date') }}</th>
            <th class="py-1 text-start font-medium">{{ t('dashboard.employer.analytics.views') }}</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="d in data" :key="d.date" class="border-b border-slate-100 dark:border-slate-800">
            <td class="py-1">{{ formatDate(d.date) }}</td>
            <td class="py-1">{{ d.count }}</td>
          </tr>
        </tbody>
      </table>
    </details>
  </div>
</template>
