<template>
  <el-pagination
    v-model:current-page="currentPage"
    v-model:page-size="pageSize"
    :total="total"
    background
    @current-change="handleCurrentChange"
    @size-change="handleSizeChange"
  />
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps(['page', 'limit', 'total'])
const emit = defineEmits(['update:page', 'update:limit', 'pagination'])

const currentPage = computed({
  get: () => props.page,
  set: (val) => emit('update:page', val)
})

const pageSize = computed({
  get: () => props.limit,
  set: (val) => emit('update:limit', val)
})

const handleCurrentChange = (val) => {
  emit('pagination', { page: val, limit: pageSize.value })
}

const handleSizeChange = (val) => {
  emit('pagination', { page: currentPage.value, limit: val })
}
</script>