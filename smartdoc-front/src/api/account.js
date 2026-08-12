import request from '@/utils/request'

export function getUserInfo() {
  return request({
    url: '/accounts',
    method: 'get'
  })
}

export function getPermissions() {
  return request({
    url: '/accounts/permissions',
    method: 'get'
  })
}

export function updatePassword(data) {
  return request({
    url: '/accounts/password',
    method: 'post',
    data
  })
}
