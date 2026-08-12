<script setup lang="ts">
// Options are passed via the default slot (plain <option> elements) since
// the choices differ per page (job type, application status, arbitrary
// location lists) - mirrors how the pages already build their <select>s.
const props = withDefaults(
  defineProps<{
    modelValue: string | number
    label?: string
    id?: string
    disabled?: boolean
  }>(),
  { disabled: false }
)

const emit = defineEmits<{ 'update:modelValue': [value: string] }>()

const autoId = useId()
const inputId = computed(() => props.id ?? autoId)
</script>

<template>
  <div class="flex flex-col gap-1">
    <label v-if="label" :for="inputId" class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ label }}</label>
    <select
      :id="inputId"
      :value="modelValue"
      :disabled="disabled"
      class="rounded-xl border border-slate-300 px-3 py-2 text-sm disabled:opacity-50 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100"
      @change="emit('update:modelValue', ($event.target as HTMLSelectElement).value)"
    >
      <slot />
    </select>
  </div>
</template>
