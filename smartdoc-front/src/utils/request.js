import axios from 'axios'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useUserStore } from '@/store/user'
import qs from 'qs'

// 业务请求
const request = axios.create({
  baseURL: import.meta.env.VITE_APP_BASE_API // url = base url + request url
  // withCredentials: true, // send cookies when cross-domain requests
  // timeout: 5000 // request timeout
})

// request interceptor
request.interceptors.request.use(
  (config) => {
    // do something before request is sent
    const userStore = useUserStore()

    if (userStore.token) {
      // let each request carry token
      // ['X-Token'] is a custom headers key
      // please modify it according to the actual situation
      config.headers['Authorization'] = 'Bearer ' + userStore.token
    }

    if(config.method === 'get'){
      //如果是get请求，且params是数组类型如arr[]=1&arr[]=2，则转换成arr=1&arr=2
      config.paramsSerializer = function(params) {
          return qs.stringify(params, {arrayFormat: 'repeat'})
      }
    }
    

    return config
  },
  (error) => {
    console.log(error)
    // do something with request error
    return Promise.reject(error)
  }
)

// response interceptor
request.interceptors.response.use(
  /**
   * If you want to get http information such as headers or status
   * Please return  response => response
   */

  /**
   * Determine the request status by custom code
   * Here is just an example
   * You can also judge the status by HTTP Status Code
   */
  (response) => {
    const res = response.data
    return res
  },
  async (error) => {
    console.log(error)

    if (error.response && error.response.status == 400) {
      if (error.response.data.code) {
        ElMessage({
          message: error.response.data.message,
          type: 'error',
          duration: 1000
        })
      } else {
        ElMessage({
          message: error.response.data.title,
          type: 'error',
          duration: 1000
        })
      }
    } else if (error.response && error.response.status == 401) {
      ElMessage({
        message: '请重新登录，认证已过期！',
        type: 'error',
        duration: 2 * 1000
      })
    } else if (error.response && error.response.status == 403) {
      ElMessage({
        message: '操作失败，无此权限！',
        type: 'error',
        duration: 1000
      })
    } else if (error.response && error.response.status == 500) {
      ElMessage({
        message: error.response.data || '服务器异常！',
        type: 'error',
        duration: 1000
      })
    } else if (error.response && error.response.status == 501) {
      ElMessage({
        message: error.response.data,
        type: 'error',
        duration: 1000
      })
    } else {
      ElMessage({
        message: error.message,
        type: 'error',
        duration: 5 * 1000
      })
    }
    return Promise.reject(error)
  }
)

export default request
