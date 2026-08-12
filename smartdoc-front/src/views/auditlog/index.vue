<template>
    <div class="app-container">
        <div class="filter-container">
            <el-form :inline="true">
                <el-form-item :label="$t('auditLog.requestUrl') + ':'">
                    <el-input v-model="listQuery.RequestUrl" :placeholder="$t('auditLog.requestUrl')"
                        style="width: 150px" class="filter-item" @keyup.enter="handleFilter" />
                </el-form-item>
                <el-form-item :label="$t('auditLog.IP') + ':'">
                    <el-input v-model="listQuery.IP" :placeholder="$t('auditLog.IP')" style="width: 150px"
                        class="filter-item" @keyup.enter="handleFilter" />
                </el-form-item>
                <el-form-item :label="$t('auditLog.startTime') + ':'">
                    <el-date-picker v-model="listQuery.timeRange" type="datetimerange" range-separator="~"
                        :start-placeholder="$t('auditLog.startTime')" :end-placeholder="$t('auditLog.endTime')" />
                </el-form-item>
                <el-form-item :label="$t('auditLog.auditLogType') + ':'">
                    <el-checkbox-group v-model="listQuery.auditLogType">
                        <el-checkbox label="0" value="0">正常日志</el-checkbox>
                        <el-checkbox label="99" value="99">异常日志</el-checkbox>
                    </el-checkbox-group>
                </el-form-item>
                <el-form-item>
                    <el-button class="filter-item" style="margin-left: 10px" type="primary" :icon="Search"
                        @click="handleFilter">
                        {{ $t('table.search') }}
                    </el-button>
                </el-form-item>
            </el-form>
        </div>

        <el-table :key="tableKey" v-loading="listLoading" :data="list" border fit highlight-current-row
            style="width: 100%">
            <el-table-column :label="$t('table.id')" prop="id" align="center" width="80">
                <template #default="{ row }">
                    <span>{{ row.id }}</span>
                </template>
            </el-table-column>
            <el-table-column :label="$t('table.date')" width="150px" align="center">
                <template #default="{ row }">
                    <span>{{ row.createTime }}</span>
                </template>
            </el-table-column>
            <el-table-column :label="$t('auditLog.requestUrl')" width="150px" align="center">
                <template #default="{ row }">
                    <span>{{ row.requestUrl }}</span>
                </template>
            </el-table-column>
            <el-table-column :label="$t('auditLog.method')" width="80px" align="center">
                <template #default="{ row }">
                    <span>{{ row.method }}</span>
                </template>
            </el-table-column>
            <el-table-column :show-overflow-tooltip="true" :label="$t('auditLog.requestParam')" width="150px"
                align="center">
                <template #default="{ row }">
                    <span>{{ row.requestParam }}</span>
                </template>
            </el-table-column>
            <el-table-column :show-overflow-tooltip="true" :label="$t('auditLog.returnValue')" width="150px"
                align="center">
                <template #default="{ row }">
                    <span>{{ row.errorMessage ? "异常" : "OK" }}</span>
                </template>
            </el-table-column>
            <el-table-column :label="$t('auditLog.executionTime') + '(ms)'" width="120px" align="center">
                <template #default="{ row }">
                    <span>{{ row.executionTime }}</span>
                </template>
            </el-table-column>
            <el-table-column :label="$t('auditLog.IP')" width="150px" align="center">
                <template #default="{ row }">
                    <span>{{ row.ip }}</span>
                </template>
            </el-table-column>
            <el-table-column :label="$t('auditLog.auditLogType')" class-name="status-col" width="100">
                <template #default="{ row }">
                    <el-tag :type="auditLogTypeFilter(row.auditLogType)">
                        {{ auditLogTypeTextFilter(row.auditLogType) }}
                    </el-tag>
                </template>
            </el-table-column>
            <el-table-column :show-overflow-tooltip="true" :label="$t('auditLog.errorMessage')" align="center">
                <template #default="{ row }">
                    <span>{{ row.errorMessage }}</span>
                    <div>{{ row.error }}</div>
                </template>
            </el-table-column>
            <el-table-column :label="$t('table.actions')" align="center" width="200"
                class-name="small-padding fixed-width">
                <template #default="{ row, $index }">
                    <el-button size="small" @click="handleDetail(row)">
                        {{ $t('table.detail') }}
                    </el-button>
                </template>
            </el-table-column>
        </el-table>

        <pagination v-show="total > 0" :total="total" v-model:page="listQuery.pageIndex"
            v-model:limit="listQuery.pagesize" @pagination="getList" />

        <el-dialog title="日志详情" v-model="dialogFormVisible">
            <el-form ref="dataForm" :model="temp" label-position="left" label-width="120px"
                style="width: 100%; max-width: 800px; margin-left: 20px" class="detail-form">
                <el-form-item :label="$t('table.id') + ':'">
                    <span>{{ temp.id }}</span>
                </el-form-item>
                <el-form-item :label="$t('table.date') + ':'">
                    <span>{{ temp.createTime }}</span>
                </el-form-item>
                <el-form-item :label="$t('auditLog.requestUrl') + ':'">
                    <span>{{ temp.requestUrl }}</span>
                </el-form-item>
                <el-form-item :label="$t('auditLog.method') + ':'">
                    <span>{{ temp.method }}</span>
                </el-form-item>
                <el-form-item :label="$t('auditLog.parameters') + ':'">
                    <span>{{ temp.requestParam }}</span>
                </el-form-item>
                <el-form-item :label="$t('auditLog.returnValue') + ':'">
                    <span>{{ temp.errorMessage ? "异常" : "OK" }}</span>
                </el-form-item>
                <el-form-item :label="$t('auditLog.executionTime') + '(ms)：'">
                    <span>{{ temp.executionTime }}</span>
                </el-form-item>
                <el-form-item :label="$t('auditLog.IP') + ':'">
                    <span>{{ temp.ip }}</span>
                </el-form-item>
                <el-form-item :label="$t('auditLog.auditLogType') + ':'">
                    <el-tag :type="auditLogTypeFilter(temp.auditLogType)">
                        {{ auditLogTypeTextFilter(temp.auditLogType) }}
                    </el-tag>
                </el-form-item>
                <el-form-item :label="$t('auditLog.creator') + ':'">
                    <span>{{ temp.creator }}</span>
                </el-form-item>
                <el-form-item :label="$t('auditLog.errorMessage') + ':'">
                    <span>{{ temp.errorMessage }}</span>
                </el-form-item>
                <el-form-item :label="$t('auditLog.error') + ':'">
                    <div>{{ temp.error }}</div>
                </el-form-item>
            </el-form>
        </el-dialog>
    </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { queryAuditLogs } from '@/api/auditlog'
import { parseTime } from '@/utils'
import { Search } from '@element-plus/icons-vue'
import Pagination from '@/components/Pagination/index.vue'

// 响应式数据定义
const tableKey = ref(0)
const list = ref([])
const total = ref(0)
const listLoading = ref(true)
const dialogFormVisible = ref(false)
const temp = ref({})

const listQuery = reactive({
    pageIndex: 1,
    pagesize: 10,
    RequestUrl: '',
    IP: '',
    timeRange: null,
    startTime: '',
    endTime: '',
    auditLogType: []
})

const auditLogTypeFilter = (status) => {
    const statusMap = {
        '0': 'success',
        '99': 'danger'
    }
    return statusMap[status] || 'info'
}

const auditLogTypeTextFilter = (status) => {
    const statusMap = {
        '0': '正常',
        '99': '异常'
    }
    return statusMap[status] || '未知'
}

const getList = async () => {
    listLoading.value = true

    if (listQuery.timeRange && listQuery.timeRange.length === 2) {
        listQuery.startTime = parseTime(listQuery.timeRange[0])
        listQuery.endTime = parseTime(listQuery.timeRange[1])
    } else {
        listQuery.startTime = ''
        listQuery.endTime = ''
    }

    try {
        const response = await queryAuditLogs(listQuery)
        list.value = response.auditLogs
        total.value = response.total
    } catch (error) {
        console.error('获取日志列表失败', error)
    } finally {
        listLoading.value = false
    }
}


const handleFilter = () => {
    listQuery.page = 1
    getList()
}

const handleDetail = (row) => {
    temp.value = { ...row }
    dialogFormVisible.value = true
}

// 生命钩子
onMounted(() => {
    getList()
})
</script>

<style scoped>
.detail-form :deep(.el-form-item) {
    margin-bottom: 0px;
}
</style>