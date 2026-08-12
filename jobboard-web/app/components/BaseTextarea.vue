<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    modelValue: string
    label: string
    id?: string
    rows?: number
    placeholder?: string
    required?: boolean
    error?: string
  }>(),
  { rows: 4, required: false }
)

const emit = defineEmits<{ 'update:modelValue': [value: string] }>()

const autoId = useId()
const inputId = computed(() => props.id ?? autoId)
</script>

<template>
  <div class="flex flex-col gap-1">
    <label :for="inputId" class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ label }}</label>
    <textarea
      :id="inputId"
      :value="modelValue"
      :rows="rows"
      :placeholder="placeholder"
      :required="required"
      class="rounded-xl border px-3 py-2 text-sm dark:bg-slate-900 dark:text-slate-100"
      :class="error ? 'border-red-400 dark:border-red-600' : 'border-slate-300 dark:border-slate-700'"
      @input="emit('update:modelValue', ($event.target as HTMLTextAreaElement).value)"
    />
    <p v-if="error" class="text-xs text-red-600 dark:text-red-400">{{ error }}</p>
  </div>
</template>
