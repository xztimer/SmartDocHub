import request from '@/utils/request'

//获取权限列表
export function getAllPermissions(query) {
  return request({
    url: '/permission/all',
    method: 'get',
    params: query
  })
}

//更新权限
export function updatePermission(data) {
  return request({
    url: '/permission/' + data.id,
    method: 'put',
    data
  })
}

//删除权限
export function deletePermission(id) {
  return request({
    url: '/permission/' + id,
    method: 'delete'
  })
}

//新增权限
export function createPermission(data) {
  return request({
    url: '/permission',
    method: 'post',
    data
  })
}
