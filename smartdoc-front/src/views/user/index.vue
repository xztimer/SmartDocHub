<template>
  <div class="app-container">
    <el-row :gutter="20">
      <el-col :span="5">
        <div class="dept-tree-sidebar">
          <div class="tree-header">部门架构</div>
          <el-tree :data="deptTree" :props="{ label: 'deptName', children: 'children' }" node-key="id"
            default-expand-all highlight-current @node-click="handleDeptClick" />
        </div>
      </el-col>

      <el-col :span="19">
        <!-- 顶部搜索栏 -->
        <div class="filter-container">
          <el-tag v-if="selectedDeptName" closable type="info" class="filter-item dept-tag" @close="handleClearDept">
            部门: {{ selectedDeptName }}
          </el-tag>

          <el-input v-model="listQuery.userName" :placeholder="$t('user.userName')" style="width: 200px" clearable
            class="filter-item" @keyup.enter="handleFilter" />

          <el-button class="filter-item" style="margin-left: 10px" type="primary" :icon="Search" @click="handleFilter">
            {{ $t('table.search') }}
          </el-button>
          <el-button v-permission="'system.user.add'" class="filter-item" style="margin-left: 10px" type="primary" :icon="Plus" @click="handleCreate">
            {{ $t('table.add') }}
          </el-button>
        </div>

        <!-- 用户表格 -->
        <el-table :key="tableKey" v-loading="listLoading" :data="list" border fit highlight-current-row
          style="width: 100%">
          <el-table-column :label="$t('table.id')" prop="id" align="center" width="80">
            <template #default="{ row }">
              <span>{{ row.id }}</span>
            </template>
          </el-table-column>
          <el-table-column :label="$t('table.date')" width="150px" align="center">
            <template #default="{ row }">
              <span>{{ parseTime(row.createTime, '{y}-{m}-{d} {h}:{i}') }}</span>
            </template>
          </el-table-column>
          <el-table-column :label="$t('user.userName')" width="150px" align="center">
            <template #default="{ row }">
              <span>{{ row.userName }}</span>
            </template>
          </el-table-column>
          <el-table-column :label="$t('user.nickName')" width="150px" align="center">
            <template #default="{ row }">
              <span>{{ row.nickName }}</span>
            </template>
          </el-table-column>
          <el-table-column :label="$t('user.deptName')" width="150px" align="center">
            <template #default="{ row }">
              <span>{{ row.deptName }}</span>
            </template>
          </el-table-column>
          <el-table-column :label="$t('user.role')" width="150px" align="center">
            <template #default="{ row }">
              <span>{{row.roles?.map(x => x.name).join('，')}}</span>
            </template>
          </el-table-column>
          <el-table-column :label="$t('table.status')" class-name="status-col" width="100" align="center">
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
              <el-button v-permission="'system.user.edit'" type="primary" size="small" @click="handleUpdate(row)">
                {{ $t('table.edit') }}
              </el-button>
              <el-button v-permission="'system.user.delete'" size="small" type="danger"
                @click="handleDelete(row, $index)">
                {{ $t('table.delete') }}
              </el-button>
            </template>
          </el-table-column>
        </el-table>

        <pagination v-show="total > 0" :total="total" v-model:page="listQuery.pageIndex"
          v-model:limit="listQuery.pageSize" @pagination="getList" />
      </el-col>
    </el-row>

    <el-dialog :title="textMap[dialogStatus]" v-model="dialogFormVisible">
      <el-form ref="dataFormRef" :rules="rules" :model="temp" label-position="left" label-width="90px"
        style="width: 400px; margin-left: 50px">
        <el-form-item :label="$t('user.userName')" prop="userName">
          <el-input v-model="temp.userName" type="text" placeholder="请输入" :readonly="temp.id > 0" />
        </el-form-item>
        <el-form-item :label="$t('user.nickName')" prop="nickName">
          <el-input v-model="temp.nickName" type="text" placeholder="请输入" />
        </el-form-item>
        <el-form-item label="所属部门" prop="deptId">
          <el-tree-select v-model="temp.deptId" :data="deptTree"
            :props="{ label: 'deptName', value: 'id', children: 'children' }" value-key="id" placeholder="请选择所属部门"
            check-strictly clearable style="width: 100%" />
        </el-form-item>
        <el-form-item :label="$t('user.role')" prop="roleNames" v-if="temp.id > 0">
          <el-select v-model="temp.roleNames" multiple placeholder="选择角色" size="large">
            <el-option v-for="item in roles" :key="item.id" :label="item.name" :value="item.name" />
          </el-select>
        </el-form-item>
        <el-form-item :label="$t('login.password')" prop="password">
          <el-input v-model="temp.password" type="password" placeholder="请输入" show-password />
        </el-form-item>
        <el-form-item :label="$t('login.surePassword')" prop="surePassword">
          <el-input v-model="temp.surePassword" type="password" placeholder="请输入" show-password />
        </el-form-item>
        <el-form-item :label="$t('table.status')" prop="status">
          <el-radio-group v-model="temp.status" size="large">
            <el-radio-button label="正常" value="正常" />
            <el-radio-button label="禁用" value="禁用" />
          </el-radio-group>
        </el-form-item>
        <el-form-item :label="$t('table.remark')" prop="remark">
          <el-input v-model="temp.remark" :autosize="{ minRows: 2, maxRows: 4 }" type="textarea" placeholder="请输入" />
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
import { ref, reactive, nextTick, onMounted, computed } from 'vue'
import { ElMessageBox, ElNotification, ElMessage } from 'element-plus'

import { Plus, Search } from '@element-plus/icons-vue'
import { queryUsers, updateUser, deleteUser, createUser } from '@/api/user'
import { getAllRoles } from '@/api/role'
import { queryDepartments } from '@/api/department'
import { parseTime, listToTree } from '@/utils'
import Pagination from '@/components/Pagination/index.vue'
import permission from '@/directives/permission'

const dataFormRef = ref(null)


const tableKey = ref(0)
const list = ref([])
const total = ref(0)
const listLoading = ref(true)
const dialogFormVisible = ref(false)
const dialogStatus = ref('')
const roles = ref([])
const rawDeptList = ref([])
const selectedDeptName = ref('')

const textMap = {
  update: '编辑',
  create: '创建'
}

const listQuery = reactive({
  pageIndex: 1,
  pageSize: 20,
  userName: '',
  deptId: undefined
})

const temp = ref({
  id: undefined,
  userName: '',
  nickName: '',
  password: '',
  surePassword: '',
  roleNames: [],
  remark: '',
  deptId: undefined,
  status: '正常'
})

// 表单校验规则
const rules = {
  userName: [{ required: true, message: '用户名必输', trigger: 'blur' }],
  nickName: [{ required: true, message: '姓名必输', trigger: 'blur' }],
  deptId: [{ required: true, message: '请选择所属部门', trigger: 'change' }],
  password: [
    {
      trigger: 'blur',
      validator: (rule, value, callback) => {
        if (!temp.value.id && !value) {
          callback(new Error('密码必输'))
        } else if (temp.value.id > 0 && !value) {
          callback()
        } else {
          const reg = /^.*(?=.*[0-9])(?=.*[A-Z])(?=.*[a-z])(?=.*[!@#$%^&*?])\w{6,}/
          if (!reg.test(value)) {
            callback(new Error('密码必须大于6位，并且包含特殊字符、大小写字母'))
          } else {
            callback()
          }
        }
      }
    }
  ],
  surePassword: [
    {
      trigger: 'blur',
      validator: (rule, value, callback) => {
        temp.value.password === value ? callback() : callback(new Error('密码不一致'))
      }
    }
  ],
  status: [{ required: true, message: '状态必选', trigger: 'blur' }]
}

const deptTree = computed(() => {
  return listToTree(rawDeptList.value)
})

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

// 获取部门列表数据
const getDeptList = async () => {
  try {
    const response = await queryDepartments()
    rawDeptList.value = response
  } catch (error) {
    ElMessage.error('获取部门架构失败')
    console.error(error)
  }
}

const getList = async () => {
  listLoading.value = true
  try {
    const response = await queryUsers(listQuery)
    list.value = response.users
    total.value = response.total
  } catch (error) {
    console.error('获取用户列表失败', error)
  } finally {
    listLoading.value = false
  }
}

const handleDeptClick = (data) => {
  listQuery.deptId = data.id
  selectedDeptName.value = data.deptName
  handleFilter()
}

const handleClearDept = () => {
  listQuery.deptId = undefined
  selectedDeptName.value = ''
  handleFilter()
}

const getRoleList = async () => {
  listLoading.value = true
  try {
    const response = await getAllRoles()
    roles.value = response
  } catch (error) {
    console.error('获取角色列表失败', error)
  } finally {
    listLoading.value = false
  }
}

const handleFilter = () => {
  listQuery.pageIndex = 1
  getList()
}

const resetTemp = () => {
  temp.value = {
    id: undefined,
    userName: '',
    nickName: '',
    password: '',
    surePassword: '',
    roleNames: [],
    remark: '',
    status: '正常'
  }
}

const handleCreate = () => {
  resetTemp()
  if (listQuery.deptId) {
    temp.value.deptId = listQuery.deptId
  }
  dialogStatus.value = 'create'
  dialogFormVisible.value = true
  nextTick(() => {
    dataFormRef.value?.clearValidate()
  })
}

const createData = () => {
  dataFormRef.value?.validate(async (valid) => {
    if (valid) {
      const data = { ...temp.value }
      data.status = data.status === '正常' ? 1 : 0
      try {
        await createUser(data)
        getList()
        dialogFormVisible.value = false
        ElNotification({
          title: '成功',
          message: '创建成功',
          type: 'success',
          duration: 2000
        })
      } catch (error) {
        console.error('创建用户失败', error)
      }
    }
  })
}

const handleUpdate = (row) => {
  temp.value = { ...row, roleNames: [] }
  temp.value.status = temp.value.status === 1 ? '正常' : '禁用'
  temp.value.roleNames = temp.value.roles ? temp.value.roles.map((x) => x.name) : []
  temp.value.deptId = row.deptId

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
    if (!valid) return
    const tempData = { ...temp.value }
    tempData.status = tempData.status === '正常' ? 1 : 0

    await updateUser(tempData)
    getList()

    dialogFormVisible.value = false
    ElNotification({ title: '成功', message: '更新成功', type: 'success', duration: 2000 })
  } catch (error) {
    console.error('更新失败或表单校验未通过:', error)
  }
}

const handleDelete = (row, index) => {
  ElMessageBox.confirm('确认删除用户?', '提示', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  })
    .then(async () => {
      try {
        const response = await deleteUser(row.id)
        if (response && response.code !== 200) {
          ElNotification({
            type: 'info',
            message: '删除失败，错误原因：' + response.message
          })
        } else {
          ElNotification({
            title: '成功',
            message: '更新成功',
            type: 'success',
            duration: 2000
          })
          list.value.splice(index, 1)
        }
      } catch (error) {
        console.error('删除用户失败', error)
      }
    })
    .catch(() => {
      ElNotification({
        type: 'info',
        message: '已取消删除'
      })
    })
}

// 生命周期挂载
onMounted(() => {
  getDeptList()
  getList()
  getRoleList()
})
</script>

<style scoped>
.dept-tree-sidebar {
  background: #fff;
  border: 1px solid #e6ebf5;
  border-radius: 4px;
  padding: 15px;
  min-height: calc(100vh - 120px);
}

.tree-header {
  font-size: 14px;
  font-weight: bold;
  color: #333;
  margin-bottom: 12px;
  padding-bottom: 8px;
  border-bottom: 1px solid #f0f0f0;
}

.filter-container {
  padding-bottom: 10px;
  display: flex;
  align-items: center;
}

.dept-tag {
  margin-right: 10px;
}
</style>