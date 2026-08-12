<template>
    <div class="app-container">
        <div class="filter-container">
            <el-input v-model="listQuery.name" :placeholder="$t('role.name')" style="width: 200px" class="filter-item"
                clearable @keyup.enter="handleFilter" />

            <el-button class="filter-item" style="margin-left: 10px" type="primary" :icon="Search"
                @click="handleFilter">
                {{ $t('table.search') }}
            </el-button>
            <el-button v-permission="'system.role.add'" class="filter-item" style="margin-left: 10px" type="primary"
                :icon="Plus" @click="handleCreate">
                {{ $t('table.add') }}
            </el-button>
        </div>

        <el-table :key="tableKey" v-loading="listLoading" :data="list" border fit highlight-current-row
            style="width: 100%">
            <el-table-column :label="$t('table.id')" prop="id" sortable="custom" align="center" width="80">
                <template #default="{ row }">
                    <span>{{ row.id }}</span>
                </template>
            </el-table-column>
            <el-table-column :label="$t('role.name')" width="150px" align="center">
                <template #default="{ row }">
                    <span>{{ row.name }}</span>
                </template>
            </el-table-column>
            <el-table-column :label="$t('table.status')" class-name="status-col" width="100">
                <template #default="{ row }">
                    <el-tag :type="statusFilter(row.status)">
                        {{ statusTextFilter(row.status) }}
                    </el-tag>
                </template>
            </el-table-column>
            <el-table-column :label="$t('table.remark')" align="center">
                <template #default="{ row }">
                    <span>{{ row.remark }}</span>
                </template>
            </el-table-column>

            <el-table-column :label="$t('table.actions')" align="center" width="200"
                class-name="small-padding fixed-width">
                <template #default="{ row, $index }">
                    <el-button v-permission="'system.role.edit'" type="primary" size="small" @click="handleUpdate(row)">
                        {{ $t('table.edit') }}
                    </el-button>
                    <el-button v-permission="'system.role.delete'" size="small" type="danger"
                        @click="handleDelete(row, $index)">
                        {{ $t('table.delete') }}
                    </el-button>
                </template>
            </el-table-column>
        </el-table>

        <pagination v-show="total > 0" :total="total" v-model:page="listQuery.page" v-model:limit="listQuery.prePage"
            @pagination="getList" />

        <el-dialog :title="textMap[dialogStatus]" v-model="dialogFormVisible">
            <el-form ref="dataFormRef" :rules="rules" :model="temp" label-position="left" label-width="70px"
                style="width: 400px; margin-left: 50px">
                <el-form-item :label="$t('role.name')" prop="name">
                    <el-input v-model="temp.name" type="text" placeholder="请输入" />
                </el-form-item>
                <el-form-item :label="$t('table.status')" prop="status">
                    <el-radio-group v-model="temp.status" size="large">
                        <el-radio-button label="正常" />
                        <el-radio-button label="禁用" />
                    </el-radio-group>
                </el-form-item>
                <el-form-item :label="$t('table.remark')" prop="remark">
                    <el-input v-model="temp.remark" :autosize="{ minRows: 2, maxRows: 4 }" type="textarea"
                        placeholder="请输入" />
                </el-form-item>
            </el-form>

            <template #footer>
                <div class="dialog-footer">
                    <el-button @click="dialogFormVisible = false">
                        {{ $t('table.cancel') }}
                    </el-button>
                    <el-button type="primary" @click="dialogStatus === 'create' ? createData() : updateData()">
                        {{ $t('table.confirm') }}
                    </el-button>
                </div>
            </template>
        </el-dialog>
    </div>
</template>

<script setup>
import { ref, reactive, nextTick, onMounted } from 'vue'
import { ElMessageBox, ElNotification } from 'element-plus'
import { Search, Plus } from '@element-plus/icons-vue'

import { queryRoles, updateRole, deleteRole, createRole } from '@/api/role'
import permission from '@/directives/permission'
import Pagination from '@/components/Pagination/index.vue'

// DOM / 组件 Ref 声明
const dataFormRef = ref(null)

// 状态定义
const tableKey = ref(0)
const list = ref([])
const total = ref(0)
const listLoading = ref(true)
const dialogFormVisible = ref(false)
const dialogStatus = ref('')

const textMap = {
    update: '编辑',
    create: '创建'
}

const listQuery = reactive({
    page: 1,
    prePage: 20,
    name: ''
})

const temp = ref({
    id: undefined,
    name: '',
    roleName: '',
    fullName: '',
    remark: '',
    status: '正常'
})

const rules = {
    name: [{ required: true, message: '角色名必输', trigger: 'blur' }],
    status: [{ required: true, message: '状态必选', trigger: 'blur' }]
}

const statusFilter = (status) => {
    const statusMap = {
        '0': 'danger',
        '1': 'success',
        deleted: 'danger'
    }
    return statusMap[status] || 'info'
}

const statusTextFilter = (status) => {
    const statusMap = {
        '0': '禁用',
        '1': '正常'
    }
    return statusMap[status] || '未知'
}

// 逻辑方法
const getList = async () => {
    listLoading.value = true
    try {
        const response = await queryRoles(listQuery)
        list.value = response.roles
        total.value = response.total
    } catch (error) {
        console.error('获取角色列表失败', error)
    } finally {
        listLoading.value = false
    }
}

const handleFilter = () => {
    listQuery.page = 1
    getList()
}

const resetTemp = () => {
    temp.value = {
        id: undefined,
        name: '',
        remark: '',
        status: '正常'
    }
}

const handleCreate = () => {
    resetTemp()
    dialogStatus.value = 'create'
    dialogFormVisible.value = true
    nextTick(() => {
        dataFormRef.value?.clearValidate()
    })
}

const createData = async () => {
    if (!dataFormRef.value) return
    try {
        const valid = await dataFormRef.value.validate()
        if (valid) {
            const data = { ...temp.value }
            data.status = data.status === '正常' ? 1 : 0
            console.log(data);
            debugger
            await createRole(data)
            getList()
            dialogFormVisible.value = false
            ElNotification({
                title: '成功',
                message: '创建成功',
                type: 'success',
                duration: 2000
            })
        }
    } catch (error) {
        console.error('表单校验未通过或创建失败:', error)
    }
}

const handleUpdate = (row) => {
    temp.value = { ...row }
    temp.value.status = temp.value.status === 1 ? '正常' : '禁用'
    dialogStatus.value = 'update'
    dialogFormVisible.value = true
    nextTick(() => {
        dataFormRef.value?.clearValidate()
    })
}

const updateData = async () => {
    if (!dataFormRef.value) return
    try {
        const valid = await dataFormRef.value.validate()
        if (valid) {
            const tempData = { ...temp.value }
            tempData.status = tempData.status === '正常' ? 1 : 0
            await updateRole(tempData)

            const index = list.value.findIndex((v) => v.id === temp.value.id)
            temp.value.status = temp.value.status === '正常' ? 1 : 0
            if (index !== -1) {
                list.value.splice(index, 1, { ...temp.value })
            }
            dialogFormVisible.value = false
            ElNotification({
                title: '成功',
                message: '更新成功',
                type: 'success',
                duration: 2000
            })
        }
    } catch (error) {
        console.error('表单校验未通过或更新失败:', error)
    }
}

const handleDelete = (row, index) => {
    ElMessageBox.confirm('确认删除角色?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
    })
        .then(async () => {
            try {
                const response = await deleteRole(row.id)
                if (response && response.code !== 200) {
                    ElNotification({
                        type: 'info',
                        message: '删除失败，错误原因：' + response.message
                    })
                } else {
                    ElNotification({
                        title: '成功',
                        message: '删除成功',
                        type: 'success',
                        duration: 2000
                    })
                    list.value.splice(index, 1)
                }
            } catch (error) {
                console.error('删除角色失败', error)
            }
        })
        .catch(() => {
            ElNotification({
                type: 'info',
                message: '已取消删除'
            })
        })
}

// 生命周期
onMounted(() => {
    getList()
})
</script>