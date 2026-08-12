import { useUserStore } from '@/store/user'

function checkPermission(el, binding) {
  const userStore = useUserStore()
  const { value } = binding
  if (value && value.length > 0) {
    const permissions = userStore.permissions
    const permissionCode = value
    const hasPermission = permissions.some((x) => {
      return x.code == permissionCode && x.type == 1
    })

    if (!hasPermission) {
      el.style.display = 'none'
    } else {
      el.style.display = ''
    }
  } else {
    throw new Error(`need value Like v-permission="'system.user.add'"`)
  }
}

export default {
  mounted(el, binding) {
    checkPermission(el, binding)
  },
  updated(el, binding) {
    checkPermission(el, binding)
  }
}
