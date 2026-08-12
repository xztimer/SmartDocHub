<template>
  <div class="app-container" style="height: 90%;">
    <el-container style="border: 1px solid #eee;">
      <!-- 左侧角色列表 -->
      <el-aside width="250px" style="background-color: rgb(238, 241, 246); margin-bottom: 0; padding-bottom: 20px;">
        <div style="padding-bottom: 10px;">
          <el-row>
            <el-col :span="10" :offset="2"><strong>角色列表</strong></el-col>
          </el-row>
        </div>
        <el-menu default-active="0" @select="roleSelect">
          <template v-for="(role, index) in roles" :key="role.id || index">
            <el-menu-item :index="index + ''">
              <svg-icon icon-class="peoples" style="margin: 0; width: 18px;" />
              <span>{{ role.name }}</span>
            </el-menu-item>
          </template>
        </el-menu>
      </el-aside>

      <!-- 右侧表格展现层 -->
      <el-table v-loading="listLoading" :data="tableMenus" border fit highlight-current-row style="width: 100%;">
        <!-- 菜单名称列 -->
        <el-table-column :label="$t('rolePermission.menuName')" width="200px">
          <template #default="{ row }">
            <div :style="{ 'padding-left': row.parentId == 0 ? '0px' : '15px' }">
              <el-checkbox v-model="checkedList[row.id + '']" :indeterminate="indeterminates[row.id + '']"
                @change="checkChange(row.id)" size="large" style="margin-right: 5px;" />
              <i v-if="row.icon && row.icon.indexOf('el-icon') >= 0" :class="row.icon"
                style="margin: 0; width: 18px;" />
              <svg-icon v-if="row.icon && row.icon.indexOf('el-icon') < 0" :icon-class="row.icon"
                style="margin: 0; width: 18px;" />
              {{ row.name }}
            </div>
          </template>
        </el-table-column>

        <!-- 按钮/功能权限列 -->
        <el-table-column :label="$t('rolePermission.elementName')">
          <template #default="{ row }">
            <el-checkbox v-for="item in getFilteredList(row.id, 1)" :key="item.id"
              v-model="checkedList[row.id + '_1_' + item.id]" @change="checkChange(row.id, 1, item.id)"
              :label="item.name" size="large" border style="margin-right: 10px; margin-bottom: 5px;" />
          </template>
        </el-table-column>

        <!-- API权限列 -->
        <el-table-column :label="$t('rolePermission.apiName')">
          <template #default="{ row }">
            <el-checkbox v-for="item in getFilteredList(row.id, 2)" :key="item.id"
              v-model="checkedList[row.id + '_2_' + item.id]" @change="checkChange(row.id, 2, item.id)"
              :label="item.name" size="large" border style="margin-right: 10px; margin-bottom: 5px;" />
          </template>
        </el-table-column>
      </el-table>
    </el-container>

    <!-- 保存操作区 -->
    <div style="clear: both; text-align: center; padding-top: 20px;">
      <el-button v-permission="'system.rolepermission.edit'" type="primary" size="large" @click="savePermissions">
        保存
      </el-button>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElNotification } from 'element-plus'
import { getAllPermissions } from '@/api/permission'
import permission from '@/directives/permission'
import { getAllRoles, getPermissions, savePermissions as saveRolePermissionsApi } from '@/api/role'
import { useUserStore } from '@/store/user'

const list = ref([])
const listLoading = ref(true)
const tableMenus = ref([])
const roles = ref([])
const rootId = ref(null)

const checkedList = reactive({})
const indeterminates = reactive({})
const userStore = useUserStore()

const getFilteredList = (parentId, typeVal) => {
  if (!list.value) return []
  return list.value.filter(x => x.parentId == parentId && (x.permissionType == typeVal || x.type == typeVal))
}


const checkChange = (rowId, typeVal, childId) => {
  if (!typeVal) {
    const menu = tableMenus.value.find(x => x.id == rowId)
    if (!menu) return

    const isChecked = checkedList[rowId + '']

    if (menu.parentId == 0) {
      const childs = tableMenus.value.filter(x => x.parentId == rowId)
      childs.forEach(child => {
        checkedList[child.id + ''] = isChecked
        checkChange(child.id)
      })
    } else {
      Object.keys(checkedList).forEach(key => {
        if (key.includes('_') && key.split('_')[0] == rowId) {
          checkedList[key] = isChecked
        }
      })
    }
  }
}

const getPermissionList = async () => {
  listLoading.value = true
  try {
    const response = await getAllPermissions()
    const rawList = response || []
    list.value = rawList

    const menus = []

    // 清空现有状态
    Object.keys(checkedList).forEach(k => delete checkedList[k])
    Object.keys(indeterminates).forEach(k => delete indeterminates[k])

    for (const item of rawList) {
      if (item.parentId == 0) {
        const menu = { ...item }
        const childs = rawList.filter(x => x.parentId == menu.id)
        menu.childs = childs
        menus.push(menu)

        checkedList[menu.id + ''] = false
        indeterminates[menu.id + ''] = false

        for (const child of childs) {
          const childId = child.id
          checkedList[childId + ''] = false
          indeterminates[childId + ''] = false

          const elementList = rawList.filter(x => x.parentId == childId && (x.permissionType == 1 || x.type == 1))
          elementList.forEach(elem => {
            checkedList[`${childId}_1_${elem.id}`] = false
          })

          const apiList = rawList.filter(x => x.parentId == childId && (x.permissionType == 2 || x.type == 2))
          apiList.forEach(api => {
            checkedList[`${childId}_2_${api.id}`] = false
          })
        }
      }
    }

    const flatTableMenus = []
    for (const m of menus) {
      flatTableMenus.push(m)
      if (m.childs && m.childs.length > 0) {
        flatTableMenus.push(...m.childs)
      }
    }
    tableMenus.value = flatTableMenus
  } catch (error) {
    console.error('获取权限定义列表失败:', error)
  } finally {
    listLoading.value = false
  }
}

const getRolePermissions = async (id) => {
  listLoading.value = true
  try {
    Object.keys(checkedList).forEach(k => {
      checkedList[k] = false
    })

    const permissions = (await getPermissions(id)) || []

    for (const item of permissions) {
      const pType = item.permissionType ?? item.type
      if (item.parentId == 0 || pType == 0) {
        checkedList[item.id + ''] = true
      } else if (pType == 1) {
        checkedList[`${item.parentId}_1_${item.id}`] = true
      } else if (pType == 2) {
        checkedList[`${item.parentId}_2_${item.id}`] = true
      }
    }
  } catch (error) {
    console.error('获取角色权限失败:', error)
  } finally {
    listLoading.value = false
  }
}

const getRoleList = async () => {
  listLoading.value = true
  try {
    const response = await getAllRoles()
    roles.value = response || []
    if (roles.value.length > 0) {
      roleSelect(0)
    }
  } catch (error) {
    console.error('获取角色列表失败:', error)
  } finally {
    listLoading.value = false
  }
}

const roleSelect = (index) => {
  const targetRole = roles.value[index]
  if (targetRole) {
    rootId.value = targetRole.id
    getRolePermissions(rootId.value)
  }
}

const savePermissions = async () => {
  if (!rootId.value) {
    ElNotification({
      title: '提示',
      message: '请选择要操作的角色',
      type: 'warning',
      duration: 2000
    })
    return
  }

  const permissionIds = []
  for (const key in checkedList) {
    if (checkedList[key]) {
      if (key.indexOf('_') > 0) {
        permissionIds.push(parseInt(key.split('_')[2]))
      } else {
        permissionIds.push(parseInt(key.split('_')[0]))
      }
    }
  }

  try {
    const response = await saveRolePermissionsApi(rootId.value, permissionIds)
    if (!response) {
      ElNotification({
        title: '成功',
        message: '更新成功',
        type: 'success',
        duration: 2000
      })
      userStore.clearPermissions()
    } else {
      ElNotification({
        title: '失败',
        message: '更新失败',
        type: 'info',
        duration: 2000
      })
    }
  } catch (error) {
    ElNotification({
      title: '错误',
      message: '系统异常，保存失败',
      type: 'error',
      duration: 2000
    })
  }
}

// 9. 生命周期挂载
onMounted(async () => {
  await getPermissionList()
  await getRoleList()
})
</script>

<style lang="scss" scoped>
.app-container {
  padding: 20px;
}
</style>
