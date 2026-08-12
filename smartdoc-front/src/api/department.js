import request from '@/utils/request'

export function queryDepartments(query) {
  return request({
    url: '/department/getlist',
    method: 'get',
    params: query
  })
}

export function getAllDepartments() {
  return request({
    url: '/department/all',
    method: 'get'
  })
}

export function addDepartment(data) {
  return request({
    url: '/department/add',
    method: 'post',
    data: data
  })
}

export function updateDepartment(data) {
  return request({
    url: '/department/update',
    method: 'post',
    data: data
  })
}

export function deleteDepartment(id) {
  return request({
    url: '/department/' + id,
    method: 'delete'
  })
}