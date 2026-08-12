<script setup lang="ts">
// Wraps the label+input+error pattern repeated on every form in the app
// (login, register, job forms, profile/company forms). modelValue stays a
// plain string regardless of `type` - callers that need type="number" still
// convert with Number(...) at submit time, matching the existing pages.
const props = withDefaults(
  defineProps<{
    modelValue: string
    label: string
    id?: string
    type?: string
    placeholder?: string
    required?: boolean
    disabled?: boolean
    autocomplete?: string
    minlength?: number
    error?: string
    hint?: string
  }>(),
  { type: 'text', required: false, disabled: false }
)

const emit = defineEmits<{ 'update:modelValue': [value: string] }>()

const autoId = useId()
const inputId = computed(() => props.id ?? autoId)
</script>

<template>
  <div class="flex flex-col gap-1">
    <label :for="inputId" class="text-sm font-medium text-slate-700 dark:text-slate-300">{{ label }}</label>
    <input
      :id="inputId"
      :type="type"
      :value="modelValue"
      :placeholder="placeholder"
      :required="required"
      :disabled="disabled"
      :autocomplete="autocomplete"
      :minlength="minlength"
      class="rounded-xl border px-3 py-2 text-sm disabled:opacity-50 dark:bg-slate-900 dark:text-slate-100"
      :class="error ? 'border-red-400 dark:border-red-600' : 'border-slate-300 dark:border-slate-700'"
      @input="emit('update:modelValue', ($event.target as HTMLInputElement).value)"
    >
    <p v-if="hint && !error" class="text-xs text-slate-500 dark:text-slate-400">{{ hint }}</p>
    <p v-if="error" class="text-xs text-red-600 dark:text-red-400">{{ error }}</p>
  </div>
</template>
