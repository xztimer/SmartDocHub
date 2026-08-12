import { defineStore } from 'pinia'
import { asyncRoutes, constantRoutes } from '@/router'

import { useUserStore } from '@/store/user'
import { getPermissions } from '@/api/account'
import { cloneDeep } from 'lodash-es'

/**
 * 检查用户是否有权限访问该路由
 * @param {Array} userPermissions 用户权限列表
 * @param {Object} route 路由对象
 */
function hasPermission(userPermissions, route) {
  if (!route.meta) {
    return true
  }

  // 检查页面权限
  if (route.meta.pageId) {
    if (userPermissions.includes(route.meta.pageId)) {
      return true
    }
  }

  // 检查按钮权限
  if (route.meta.buttonIds) {
    if (
      route.meta.buttonIds.some((button) =>
        userPermissions.includes(button.buttonId)
      )
    ) {
      return true
    }
  }

  return false
}

/**
 * 递归过滤异步路由表
 * @param routes asyncRoutes
 * @param permissionIds 用户权限 {pageIds: [], buttonIds: []}
 */
export function filterAsyncRoutes(routes, permissions) {
  const res = []

  routes.forEach((route) => {
    const tmp = { ...route }
    // 检查是否有子路由有权限
    const hasChildPermission =
      tmp.children && filterAsyncRoutes(tmp.children, permissionIds).length > 0

    // 如果当前路由有权限或者子路由有权限，都应该保留当前路由
    if (hasPermission(permissionIds, tmp) || hasChildPermission) {
      if (tmp.children) {
        tmp.children = filterAsyncRoutes(tmp.children, permissionIds)
      }
      res.push(tmp)
    }
  })

  return res
}

export const usePermissionStore = defineStore('permission', {
  state: () => {
    return {
      routes: [],
      addRoutes: []
    }
  },
  actions: {
    /**
     * @method generateRoutes
     */
    async generateRoutes(permissions) {
      try {
        const res = permissions
        const rawPermissions = res || []

        // 2. 提取菜单 Code 集合 和 按钮 Code 集合
        const menuCodes = new Set()
        const buttonCodes = []

        rawPermissions.forEach((item) => {
          if (item.type === 0 && item.code) {
            menuCodes.add(item.code) // 菜单用于路由匹配
          } else if (item.type === 1 && item.code) {
            buttonCodes.push(item.code) // 按钮用于指令/元素控制
          }
        })

        const accessedRoutes = cloneDeep(asyncRoutes)

        accessedRoutes.forEach((route) => {
          let hasParentAccess = false

          if (route.children && route.children.length > 0) {
            route.children.forEach((child) => {
              const hasAccess = menuCodes.has(child.name)
              child.hidden = !hasAccess

              if (hasAccess) {
                hasParentAccess = true
              }
            })
          }

          // 一级菜单：如果直接匹配到，或者旗下有子菜单有权限，则显示
          const isDirectMenu = menuCodes.has(route.name)
          route.hidden = !(isDirectMenu || hasParentAccess)
        })

        this.addRoutes = accessedRoutes
        this.routes = constantRoutes.concat(accessedRoutes)
        return accessedRoutes
      } catch (error) {
        console.error('Failed to generate routes:', error)
        throw error
      }
    }
  }
})
