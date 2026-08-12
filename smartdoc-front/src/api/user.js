import request from '@/utils/request'

//获取用户列表
export function queryUsers(query) {
  return request({
    url: '/user/list',
    method: 'get',
    params: query
  })
}

//更新用户
export function updateUser(data) {
  return request({
    url: '/user/' + data.id,
    method: 'put',
    data
  })
}

export function enableUser(id) {
  return request({
    url: '/user/' + id,
    method: 'put'
  })
}

//删除用户
export function deleteUser(id) {
  return request({
    url: '/user/' + id,
    method: 'delete'
  })
}

//新增用户
export function createUser(data) {
  return request({
    url: '/user/add',
    method: 'post',
    data
  })
}

//获取所有用户列表
export function queryAllUsers(query) {
  return request({
    url: '/user/all',
    method: 'get',
    params: query
  })
}
