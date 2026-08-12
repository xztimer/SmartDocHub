<template>
    <div class="app-container" style="height: 90%">
        <el-container style="border: 1px solid #eee">
            <!-- 侧边菜单树 -->
            <el-aside width="250px"
                style="background-color: rgb(238, 241, 246); margin-bottom: 0; padding-bottom: 20px">
                <div style="padding-bottom: 10px">
                    <el-row>
                        <el-col :span="10" :offset="2"><strong>菜单</strong></el-col>
                        <el-col :span="12">
                            <el-button v-permission="'system.permission.add'" type="primary" :icon="Edit"
                                @click="handleCreate('menu')">
                                添加菜单
                            </el-button>
                        </el-col>
                    </el-row>
                </div>
                <el-menu default-active="2" class="el-menu-vertical-demo" :default-openeds="menuOpeneds">
                    <template v-for="(menu, index) in treeMenus" :key="menu.id || index">
                        <el-sub-menu :index="index + ''">
                            <template #title>
                                <i v-if="menu.icon && menu.icon.indexOf('el-icon') >= 0" :class="menu.icon"
                                    style="margin: 0; width: 18px" />
                                <svg-icon v-if="menu.icon && menu.icon.indexOf('el-icon') < 0" :icon-class="menu.icon"
                                    style="margin: 0; width: 18px" />
                                <span>{{ menu.name }}</span>
                            </template>
                            <template v-for="child in menu.childs" :key="child.id || child.code">
                                <el-menu-item :index="child.code">
                                    <i v-if="child.icon && child.icon.indexOf('el-icon') >= 0" :class="child.icon"
                                        style="margin: 0; width: 18px" />
                                    <svg-icon v-if="child.icon && child.icon.indexOf('el-icon') < 0"
                                        :icon-class="child.icon" style="margin: 0; width: 18px" />
                                    {{ child.name }}
                                </el-menu-item>
                            </template>
                        </el-sub-menu>
                    </template>
                </el-menu>
            </el-aside>

            <!-- 右侧权限表格 -->
            <el-container>
                <el-table :key="tableKey" v-loading="listLoading" :data="tableMenus" border fit highlight-current-row
                    style="width: 100%">
                    <!-- 展开列：按钮与 API 权限设置 -->
                    <el-table-column type="expand">
                        <template #default="props">
                            <div v-if="props.row.parentId == 0">此菜单为目录</div>
                            <template v-if="props.row.parentId > 0">
                                <h3>
                                    按钮/功能权限：设置页面功能、按钮权限
                                    <el-button v-permission="'system.permission.add'"
                                        @click="handleCreate('button', props.row.id)" type="primary" :icon="Edit"
                                        style="margin-left: 10px">
                                        添加功能权限
                                    </el-button>
                                </h3>
                                <el-table v-loading="listLoading" stripe
                                    :data="list.filter((x) => x.parentId == props.row.id && x.type == 1)" border fit
                                    highlight-current-row style="width: 100%">
                                    <el-table-column :label="$t('table.id')" prop="id" align="center" width="80">
                                        <template #default="{ row }">
                                            <span>{{ row.id }}</span>
                                        </template>
                                    </el-table-column>
                                    <el-table-column :label="$t('permission.elementName')" width="150px" align="center">
                                        <template #default="{ row }">
                                            <span>{{ row.name }}</span>
                                        </template>
                                    </el-table-column>
                                    <el-table-column :label="$t('permission.permissionCode')" class-name="status-col"
                                        width="150">
                                        <template #default="{ row }">
                                            {{ row.code }}
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
                                            <el-button v-permission="'system.permission.edit'" type="primary"
                                                size="small" @click="handleUpdate(row)">
                                                {{ $t('table.edit') }}
                                            </el-button>
                                            <el-button v-permission="'system.permission.delete'" size="small"
                                                type="danger" @click="handleDelete(row, $index)">
                                                {{ $t('table.delete') }}
                                            </el-button>
                                        </template>
                                    </el-table-column>
                                </el-table>

                                <h3>
                                    API权限：设置API访问权限
                                    <el-button v-permission="'system.permission.add'"
                                        @click="handleCreate('api', props.row.id)" type="primary" :icon="Edit"
                                        style="margin-left: 10px">
                                        添加API权限
                                    </el-button>
                                </h3>
                                <el-table v-loading="listLoading" stripe
                                    :data="list.filter((x) => x.parentId == props.row.id && x.type == 2)" border fit
                                    highlight-current-row style="width: 100%">
                                    <el-table-column :label="$t('table.id')" prop="id" align="center" width="80">
                                        <template #default="{ row }">
                                            <span>{{ row.id }}</span>
                                        </template>
                                    </el-table-column>
                                    <el-table-column :label="$t('permission.apiName')" width="150px" align="center">
                                        <template #default="{ row }">
                                            <span>{{ row.name }}</span>
                                        </template>
                                    </el-table-column>
                                    <el-table-column :label="$t('permission.permissionCode')" class-name="status-col"
                                        width="150">
                                        <template #default="{ row }">
                                            {{ row.code }}
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
                                            <el-button v-permission="'system.permission.edit'" type="primary"
                                                size="small" @click="handleUpdate(row)">
                                                {{ $t('table.edit') }}
                                            </el-button>
                                            <el-button v-permission="'system.permission.delete'" size="small"
                                                type="danger" @click="handleDelete(row, $index)">
                                                {{ $t('table.delete') }}
                                            </el-button>
                                        </template>
                                    </el-table-column>
                                </el-table>
                            </template>
                        </template>
                    </el-table-column>

                    <!-- 主表格列 -->
                    <el-table-column :label="$t('table.id')" prop="id" align="center" width="80">
                        <template #default="{ row }">
                            <span>{{ row.id }}</span>
                        </template>
                    </el-table-column>
                    <el-table-column :label="$t('permission.menuName')" width="150px">
                        <template #default="{ row }">
                            <div :style="{ 'padding-left': row.parentId == 0 ? '0px' : '15px' }">
                                <i v-if="row.icon && row.icon.indexOf('el-icon') >= 0" :class="row.icon"
                                    style="margin: 0; width: 18px" />
                                <svg-icon v-if="row.icon && row.icon.indexOf('el-icon') < 0" :icon-class="row.icon"
                                    style="margin: 0; width: 18px" />
                                {{ row.name }}
                            </div>
                        </template>
                    </el-table-column>
                    <el-table-column :label="$t('permission.permissionCode')" class-name="status-col" width="150">
                        <template #default="{ row }">
                            {{ row.code }}
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
                            <el-button v-permission="'system.permission.edit'" type="primary" size="small"
                                @click="handleUpdate(row)">
                                {{ $t('table.edit') }}
                            </el-button>
                            <el-button v-permission="'system.permission.delete'" size="small" type="danger"
                                @click="handleDelete(row, $index)">
                                {{ $t('table.delete') }}
                            </el-button>
                        </template>
                    </el-table-column>
                </el-table>
            </el-container>
        </el-container>

        <!-- 弹窗表单 -->
        <el-dialog :title="textMap[dialogStatus] + dialogTitle" v-model="dialogFormVisible">
            <el-form ref="dataFormRef" :rules="rules" :model="temp" label-position="left" label-width="100px"
                style="width: 400px; margin-left: 50px">
                <el-form-item :label="$t('permission.type')" prop="type">
                    <el-radio-group v-model="temp.type" size="large" :disabled="temp.id > 0">
                        <el-radio-button label="菜单" value="菜单" />
                        <el-radio-button label="按钮" value="按钮" />
                        <el-radio-button label="Api" value="Api" />
                    </el-radio-group>
                </el-form-item>
                <el-form-item :label="$t('permission.parentMenu')" prop="parentId">
                    <el-autocomplete popper-class="my-autocomplete" v-model="temp.parentName"
                        :fetch-suggestions="querySearch" placeholder="请输入内容" @select="handleSelect">
                        <template #suffix>
                            <el-icon class="el-input__icon" @click="handleIconClick">
                                <Edit />
                            </el-icon>
                        </template>
                        <template #default="{ item }">
                            <div :style="{ 'margin-left': item.parentId == 0 ? '0px' : '20px' }">
                                <i v-if="item.icon && item.icon.indexOf('el-icon') >= 0" :class="item.icon"
                                    style="margin: 0; width: 18px" />
                                <svg-icon v-if="item.icon && item.icon.indexOf('el-icon') < 0" :icon-class="item.icon"
                                    style="margin: 0; width: 18px" />
                                <span :style="{ color: item.parentId == 0 && temp.type != '菜单' ? '#ccc' : '' }">
                                    {{ item.name }}
                                </span>
                            </div>
                        </template>
                    </el-autocomplete>
                </el-form-item>
                <el-form-item :label="$t('permission.permissionName')" prop="name">
                    <el-input v-model="temp.name" type="text" placeholder="请输入" />
                </el-form-item>
                <el-form-item :label="$t('permission.permissionCode')" prop="code">
                    <el-input v-model="temp.code" type="text" placeholder="请输入" />
                </el-form-item>
                <el-form-item :label="$t('table.status')" prop="status">
                    <el-radio-group v-model="temp.status" size="large">
                        <el-radio-button label="正常" value="正常" />
                        <el-radio-button label="禁用" value="禁用" />
                    </el-radio-group>
                </el-form-item>
                <el-form-item v-if="temp.type == 'Api'" :label="$t('permission.apiUrl')" prop="path">
                    <el-input v-model="temp.path" type="text" placeholder="兼容正则表达式" />
                </el-form-item>
                <el-form-item v-if="temp.type == 'Api'" :label="$t('permission.apiMethod')" prop="apiMethod">
                    <el-radio-group v-model="temp.apiMethod" size="large">
                        <el-radio-button label="Get" value="Get" />
                        <el-radio-button label="Post" value="Post" />
                        <el-radio-button label="Put" value="Put" />
                        <el-radio-button label="Delete" value="Delete" />
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
import { Edit } from '@element-plus/icons-vue'
import { getAllPermissions, updatePermission, deletePermission, createPermission } from '@/api/permission'
// DOM / 组件 Ref
const dataFormRef = ref(null)

// 响应式数据
const tableKey = ref(0)
const list = ref([])
const total = ref(0)
const listLoading = ref(true)
const dialogFormVisible = ref(false)
const dialogStatus = ref('')
const dialogTitle = ref('')

const treeMenus = ref([])
const tableMenus = ref([])
const menuOpeneds = ref([])

const listQuery = reactive({})

const textMap = {
    update: '编辑',
    create: '创建'
}

const temp = ref({
    id: undefined,
    name: '',
    code: '',
    remark: '',
    status: '正常',
    type: '菜单',
    parentName: '',
    parentId: 0,
    path: '',
    apiMethod: '',
    icon: ''
})

const rules = {
    parentId: [{ required: true, message: '父节点必选', trigger: 'blur' }],
    name: [{ required: true, message: '角色名必输', trigger: 'blur' }],
    code: [{ required: true, message: '编码必输', trigger: 'blur' }],
    path: [{ required: true, message: 'Api Url必输', trigger: 'blur' }],
    apiMethod: [{ required: true, message: 'Api动作必选', trigger: 'blur' }],
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

const getList = async () => {
    listLoading.value = true
    try {
        const response = await getAllPermissions(listQuery)
        const rawList = response || []
        console.log(rawList);

        const menus = []
        for (const i in rawList) {
            if (rawList[i].parentId == 0) {
                const menu = { ...rawList[i] }
                menu.childs = rawList.filter((x) => x.parentId == menu.id)
                menus.push(menu)
            }
        }
        console.log(menus);

        treeMenus.value = menus

        const openedIndices = []
        for (let i = 0; i < menus.length; i++) {
            openedIndices.push(i + '')
        }
        menuOpeneds.value = openedIndices

        let flatTableMenus = []
        for (let i = 0; i < menus.length; i++) {
            flatTableMenus.push(menus[i])
            flatTableMenus = flatTableMenus.concat(menus[i].childs)
        }
        tableMenus.value = flatTableMenus

        list.value = rawList
        total.value = rawList.length
    } catch (error) {
        console.error('获取权限列表失败:', error)
    } finally {
        listLoading.value = false
    }
}

// 下拉搜索框过滤
const querySearch = (queryString, cb) => {
    let results = list.value.filter((x) => x.parentId == 0)
    switch (temp.value.type) {
        case '按钮':
        case 'Api':
            results = list.value.filter((x) => x.type == 0)
            break
        default:
            results = [{ id: 0, name: '根目录', icon: 'el-icon-s-home', parentId: 0 }, ...results]
            break
    }
    cb(results)
}

const handleSelect = (item) => {
    temp.value.parentName = item.name
    temp.value.parentId = item.id
}

const handleIconClick = (ev) => {
    console.log(ev)
}

const resetTemp = () => {
    temp.value = {
        id: undefined,
        name: '',
        code: '',
        remark: '',
        status: '正常',
        type: '',
        parentName: '',
        parentId: 0,
        path: '',
        apiMethod: '',
        icon: ''
    }
}

const handleCreate = (type, rowId) => {
    resetTemp()
    dialogStatus.value = 'create'
    switch (type) {
        case 'button':
            dialogTitle.value = '按钮/功能权限'
            temp.value.type = '按钮'
            temp.value.parentId = rowId
            break
        case 'api':
            dialogTitle.value = 'Api权限'
            temp.value.type = 'Api'
            temp.value.apiMethod = 'Get'
            temp.value.parentId = rowId
            break
        default:
            dialogTitle.value = '菜单'
            temp.value.type = '菜单'
            break
    }

    const parent = list.value.find((x) => x.id == temp.value.parentId)
    temp.value.parentName = !temp.value.parentId ? '根目录' : parent ? parent.name : ''

    dialogFormVisible.value = true
    nextTick(() => {
        dataFormRef.value?.clearValidate()
    })
}

const createData = async () => {
    if (!dataFormRef.value) return
    try {
        const valid = await dataFormRef.value.validate()
        if (!valid) return

        const tempData = { ...temp.value }
        tempData.status = tempData.status === '正常' ? 1 : 0
        switch (tempData.type) {
            case '菜单':
                tempData.type = 0
                break
            case '按钮':
                tempData.type = 1
                break
            case 'Api':
                tempData.type = 2
                break
        }

        await createPermission(tempData)
        getList()
        dialogFormVisible.value = false
        ElNotification({
            title: '成功',
            message: '创建成功',
            type: 'success',
            duration: 2000
        })
    } catch (error) {
        console.error('创建权限失败:', error)
    }
}

const handleUpdate = (row) => {
    temp.value = { ...row }
    temp.value.status = temp.value.status === 1 ? '正常' : '禁用'

    switch (temp.value.type) {
        case 0:
            temp.value.type = '菜单'
            break
        case 1:
            temp.value.type = '按钮'
            break
        case 2:
            temp.value.type = 'Api'
            break
    }

    const parent = list.value.find((x) => x.id == temp.value.parentId)
    temp.value.parentName = temp.value.parentId == 0 ? '根目录' : parent ? parent.name : ''

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
        switch (tempData.type) {
            case '菜单':
                tempData.type = 0
                break
            case '按钮':
                tempData.type = 1
                break
            case 'Api':
                tempData.type = 2
                break
        }

        await updatePermission(tempData)
        getList()
        dialogFormVisible.value = false
        ElNotification({
            title: '成功',
            message: '更新成功',
            type: 'success',
            duration: 2000
        })
    } catch (error) {
        console.error('更新权限失败:', error)
    }
}

const handleDelete = (row, index) => {
    ElMessageBox.confirm('确认删除此权限?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
    })
        .then(async () => {
            try {
                const response = await deletePermission(row.id)
                console.log('删除权限响应:', response)
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
                    getList()
                }
            } catch (error) {
                console.error('删除权限失败', error)
            }
        })
        .catch(() => {
            ElNotification({
                type: 'info',
                message: '已取消删除'
            })
        })
}

// 页面挂载
onMounted(() => {
    getList()
})
</script>

<style scoped>
.app-container {
    padding: 20px;
}
</style>