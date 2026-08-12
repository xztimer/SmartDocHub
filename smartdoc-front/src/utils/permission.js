import { useUserStore } from '@/store/user'

export default function checkPermission(el, binding) {
  const userStore = useUserStore()
  const { value } = binding
  if (value && value.length > 0) {
    const permissions = userStore.permissions
    const permissionCode = value
    console.log('permissions', permissions)
    const hasPermission = permissions.some((x) => {
      return x.code == permissionCode && x.permissionType == 1
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
