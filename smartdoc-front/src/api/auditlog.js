import request from '@/utils/request'

export function queryAuditLogs(query) {
  return request({
    url: '/auditlog',
    method: 'get',
    params: query
  })
}
