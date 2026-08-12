<template>
    <div class="department-management">
        <!-- 顶部搜索栏 -->
        <el-form :inline="true" :model="searchForm" class="search-form">
            <el-form-item>
                <el-input v-model="searchForm.key" placeholder="部门名称" clearable />
            </el-form-item>
            <el-form-item>
                <el-button type="primary" :icon="Search" @click="handleSearch">查询</el-button>
                <el-button v-permission="'system.department.add'" type="primary" :icon="Plus"
                    @click="handleAdd()">新增</el-button>
            </el-form-item>
        </el-form>

        <el-table ref="tableRef" :data="deptTree" style="width: 100%" row-key="id" default-expand-all border
            :tree-props="{ children: 'children', hasChildren: 'hasChildren' }">
            <el-table-column prop="deptName" label="部门名称" min-width="180" align="center" />
            <el-table-column prop="code" label="部门编码" min-width="120" align="center" />
            <el-table-column prop="description" label="部门描述" min-width="120" align="center" />
            <el-table-column prop="sort" label="排序" width="80" align="center" />

            <el-table-column prop="status" label="状态" width="100" align="center">
                <template #default="scope">
                    <el-tag :type="statusFilter(scope.row.status)" size="small">
                        {{ statusTextFilter(scope.row.status) }}
                    </el-tag>
                </template>
            </el-table-column>

            <el-table-column label="操作" min-width="200" align="left">
                <template #default="scope">
                    <!-- 任何节点均可新增子节点 -->
                    <el-button v-permission="'system.department.add'" type="primary" :icon="Plus"
                        @click="handleAdd(scope.row)">新增</el-button>

                    <!-- 仅当非根节点（parentId不为null且不为0）时，才显示编辑和删除 -->
                    <el-button v-permission="'system.department.edit'"
                        v-if="scope.row.parentId !== null && scope.row.parentId !== 0" type="primary" :icon="Edit"
                        @click="handleEdit(scope.row)">编辑</el-button>
                    <el-button v-permission="'system.department.delete'"
                        v-if="scope.row.parentId !== null && scope.row.parentId !== 0" type="danger" :icon="Delete"
                        @click="handleDelete(scope.row)">删除</el-button>
                </template>
            </el-table-column>
        </el-table>

        <!-- 增改弹窗 -->
        <el-dialog :title="textMap[dialogStatus]" v-model="dialogFormVisible" width="500px">
            <el-form ref="dataFormRef" :rules="rules" :model="temp" label-position="left" label-width="100px"
                style="width: 400px; margin-left: 30px">

                <el-form-item label="上级部门" prop="parentId">
                    <el-tree-select v-model="temp.parentId" :data="deptTree"
                        :props="{ label: 'deptName', value: 'id', children: 'children' }" value-key="id"
                        placeholder="请选择上级部门" check-strictly clearable style="width: 100%" />
                </el-form-item>

                <el-form-item label="部门名称" prop="deptName">
                    <el-input v-model="temp.deptName" type="text" placeholder="请输入部门名称" />
                </el-form-item>

                <el-form-item label="部门编码" prop="code">
                    <el-input v-model="temp.code" type="text" placeholder="请输入部门编码" />
                </el-form-item>

                <el-form-item label="排序" prop="sort">
                    <el-input-number v-model="temp.sort" :min="0" style="width: 100%" />
                </el-form-item>

                <el-form-item label="状态" prop="status">
                    <el-radio-group v-model="temp.status">
                        <el-radio-button :value="1">正常</el-radio-button>
                        <el-radio-button :value="0">禁用</el-radio-button>
                    </el-radio-group>
                </el-form-item>

                <el-form-item label="说明" prop="description">
                    <el-input v-model="temp.description" :autosize="{ minRows: 2, maxRows: 4 }" type="textarea"
                        placeholder="请输入部门描述说明" />
                </el-form-item>
            </el-form>

            <template #footer>
                <div class="dialog-footer">
                    <el-button @click="dialogFormVisible = false">取消</el-button>
                    <el-button type="primary" @click="dialogStatus === 'create' ? createData() : updateData()">
                        确定
                    </el-button>
                </div>
            </template>
        </el-dialog>
    </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed, nextTick } from 'vue'
import { Search, Plus, Edit, Delete } from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { listToTree } from '@/utils'
import permission from '@/directives/permission'
import { queryDepartments, addDepartment, updateDepartment, deleteDepartment } from '@/api/department'

const searchForm = reactive({
    key: ''
})
const dialogFormVisible = ref(false)
const textMap = {
    update: '编辑部门',
    create: '新增部门'
}
const dialogStatus = ref('')
const tableData = ref([])
const dataFormRef = ref(null)

// 对应后端的真实 temp 结构
const temp = ref({
    id: undefined,
    parentId: null,
    deptName: '',
    code: '',
    sort: 1,
    description: "",
    status: 1
})

// 表单校验规则：满足“上级部门和部门名称必填”
const rules = reactive({
    parentId: [
        { required: true, message: '请选择上级部门', trigger: 'change' }
    ],
    deptName: [
        { required: true, message: '请输入部门名称', trigger: 'blur' }
    ]
})

const statusFilter = (status) => {
    return status === 1 ? 'success' : 'danger'
}
const statusTextFilter = (status) => {
    return status === 1 ? '正常' : '禁用'
}

const handleSearch = () => {
    getDepartments()
}

const resetTemp = () => {
    temp.value = {
        id: undefined,
        parentId: null,
        deptName: '',
        code: '',
        sort: 1,
        description: "",
        status: 1
    }
}

// 新增功能：若传入 row，则代表点击行新增，自动带出当前行的 id 作为新部门的 parentId
const handleAdd = (row) => {
    resetTemp()
    dialogStatus.value = 'create'
    if (row && row.id) {
        temp.value.parentId = row.id // 自动带出当前节点作为上级部门
    }
    dialogFormVisible.value = true
    nextTick(() => {
        if (dataFormRef.value) dataFormRef.value.clearValidate()
    })
}

// 编辑功能
const handleEdit = (row) => {
    dialogStatus.value = 'update'
    // 深拷贝单行数据填入表单
    temp.value = { ...row }
    dialogFormVisible.value = true
    nextTick(() => {
        if (dataFormRef.value) dataFormRef.value.clearValidate()
    })
}

// 创建提交
const createData = () => {
    dataFormRef.value.validate((valid) => {
        if (valid) {
            addDepartment(temp.value).then(() => {
                dialogFormVisible.value = false
                ElMessage.success('创建成功')
                getDepartments()
            })
        }
    })
}

// 更新提交
const updateData = () => {
    dataFormRef.value.validate((valid) => {
        if (valid) {
            updateDepartment(temp.value).then(() => {
                dialogFormVisible.value = false
                ElMessage.success('更新成功')
                getDepartments()
            })
        }
    })
}

// 删除功能
const handleDelete = (row) => {
    ElMessageBox.confirm(`确定要删除部门【${row.deptName}】吗？`, '警告', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
    }).then(() => {
        deleteDepartment(row.id).then(() => {
            ElMessage.success('删除成功')
            getDepartments()
        })
    }).catch(() => { })
}

function getDepartments() {
    queryDepartments(searchForm).then(response => {
        tableData.value = response
    }).catch(error => {
        ElMessage.error('获取部门数据失败')
        console.error(error)
    })
}

const deptTree = computed(() => {
    return listToTree(tableData.value)
})

onMounted(() => {
    getDepartments()
})
</script>

<style scoped>
.department-management {
    padding: 16px;
}

.search-form {
    margin-bottom: 16px;
}
</style>