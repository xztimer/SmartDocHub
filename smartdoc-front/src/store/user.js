import { defineStore } from 'pinia'
import { setCookieItem, getCookieItem, removeCookieItem } from '@/utils/storage'
import { resetRouter } from '@/router/index'
import { useTagsViewStore } from '@/store/tagsView'
import { getPermissions } from '@/api/account'

export const useUserStore = defineStore('user', {
  state: () => {
    return {
      token: getCookieItem('token'),
      userInfo: getCookieItem('userInfo'),
      permissions: []
    }
  },
  actions: {
    setToken({ token }) {
      this.token = token

      setCookieItem('token', token)
    },
    setUserInfo({ userInfo }) {
      this.userInfo = userInfo

      setCookieItem('userInfo', userInfo)
    },
    async getUserPermissions() {
      const resPermis = await getPermissions()
      this.permissions = resPermis
      return { permissions: this.permissions }
    },
    logout() {
      const tagsViewStore = useTagsViewStore()
      return new Promise((resolve) => {
        this.token = ''

        this.userInfo = null

        removeCookieItem('token')

        removeCookieItem('userInfo')

        resetRouter()

        tagsViewStore.delAllViews()

        resolve()
      })
    },
    clearPermissions() {
      this.permissions = [] 
    },
    resetToken() {
      return new Promise((resolve) => {
        this.token = ''

        this.userInfo = null

        removeCookieItem('token')

        removeCookieItem('userInfo')

        resolve()
      })
    }
  }
})
