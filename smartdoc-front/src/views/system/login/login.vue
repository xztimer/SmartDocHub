<template>
  <div class="login-container">
    <el-form ref="refLoginForm" :model="loginForm" :rules="loginRules" class="login-form" autocomplete="on"
      label-position="left">
      <div class="title-container">
        <h3 class="title">Login Form</h3>
      </div>

      <!-- 用户名 -->
      <el-form-item prop="username">
        <span class="svg-container">
          <svg-icon name="user" />
        </span>
        <el-input ref="refUsername" v-model="loginForm.username" placeholder="Username" name="username" type="text"
          tabindex="1" autocomplete="on" />
      </el-form-item>

      <!-- 密码 -->
      <el-form-item prop="password">
        <span class="svg-container">
          <svg-icon name="password" />
        </span>
        <el-input ref="refPassword" v-model="loginForm.password" :type="passwordType" placeholder="Password"
          name="password" tabindex="2" autocomplete="on" @keyup.enter="handleLogin" />
        <span class="show-pwd" @click="showPwd">
          <svg-icon :name="passwordType === 'password' ? 'eye' : 'eye-open'" />
        </span>
      </el-form-item>

      <!-- 验证码 -->
      <el-form-item prop="code" class="captcha-item">
        <span class="svg-container">
          <svg-icon name="validCode" />
        </span>
        <el-input v-model="loginForm.code" placeholder="Captcha" name="code" type="text" tabindex="3" autocomplete="off"
          @keyup.enter="handleLogin" />
        <div class="captcha-img" @click="refreshCaptcha">
          <img v-if="captchaUrl" :src="captchaUrl" alt="captcha" />
          <span v-else class="captcha-placeholder">获取验证码</span>
        </div>
      </el-form-item>

      <el-button :loading="loading" type="primary" size="large" style="width: 100%; margin-bottom: 30px"
        @click.prevent="handleLogin">
        Login
      </el-button>

      <div class="tips">
        <span>Username : admin</span>
        <span>Password : any</span>
      </div>
      <div class="tips">
        <span style="margin-right: 18px">Username : editor</span>
        <span>Password : any</span>
      </div>
    </el-form>
  </div>
</template>

<script setup>
import { ref, reactive, nextTick, onMounted } from 'vue'
import { useUserStore } from '@/store/user'
import { useRouter, useRoute } from 'vue-router'
import { login, getCode } from '@/api/auth'
import { getUserInfo, getPermissions } from '@/api/account'

const router = useRouter()
const route = useRoute()

const refLoginForm = ref(null)
const refUsername = ref(null)
const refPassword = ref(null)

// 基础状态定义
const passwordType = ref('password')
const loading = ref(false)
const captchaUrl = ref('')
const redirect = ref(undefined)
const otherQuery = ref({})

// 表单数据定义
const loginForm = reactive({
  username: 'Admin',
  password: 'abc123',
  code: '',
  codekey: '' // 若后端需要 uuid 关联验证码可在此定义
})

// 表单校验逻辑
const validateUsername = (rule, value, callback) => {
  if (!value) {
    callback(new Error('Please enter the correct user name'))
  } else {
    callback()
  }
}

const validatePassword = (rule, value, callback) => {
  if (value.length < 6) {
    callback(new Error('The password can not be less than 6 digits'))
  } else {
    callback()
  }
}

const validateCode = (rule, value, callback) => {
  if (!value) {
    callback(new Error('Please enter the captcha'))
  } else {
    callback()
  }
}

const loginRules = reactive({
  username: [{ required: true, trigger: 'blur', validator: validateUsername }],
  password: [{ required: true, trigger: 'blur', validator: validatePassword }],
  code: [{ required: true, trigger: 'blur', validator: validateCode }]
})

// 解析路由 query 参数
const getOtherQuery = (query) => {
  return Object.keys(query).reduce((acc, cur) => {
    if (cur !== 'redirect') {
      acc[cur] = query[cur]
    }
    return acc
  }, {})
}

// 切换密码显示隐藏
const showPwd = () => {
  passwordType.value = passwordType.value === 'password' ? '' : 'password'
  nextTick(() => {
    refPassword.value?.focus()
  })
}

// 获取/刷新验证码
const refreshCaptcha = async () => {
  const res = await getCode()
  captchaUrl.value = res.image
  loginForm.codekey = res.codeKey
}

// 登录处理
const handleLogin = () => {
  refLoginForm.value?.validate(async (valid) => {
    if (!valid) return

    loading.value = true
    try {
      const userStore = useUserStore()
      const { token } = await login(loginForm)
      userStore.setToken({ token })

      const res = await getUserInfo()
      console.log(res);
      
      userStore.setUserInfo({ userInfo: res })

      router.push({ path: redirect.value || '/', query: otherQuery.value })
    } catch (error) {
      refreshCaptcha()
    } finally {
      loading.value = false
    }
  })
}

onMounted(() => {
  const { query } = route

  if (!loginForm.username) {
    refUsername.value?.focus()
  } else if (!loginForm.password) {
    refPassword.value?.focus()
  }

  if (query) {
    redirect.value = query.redirect
    otherQuery.value = getOtherQuery(query)
  }

  // 初始化加载验证码
  refreshCaptcha()
})
</script>

<style lang="scss">
$bg: #283443;
$light_gray: #fff;
$cursor: #fff;

@supports (-webkit-mask: none) and (not (cater-color: $cursor)) {
  .login-container .el-input input {
    color: $cursor;
  }
}

/* reset element-ui css */
.login-container {
  .el-input {
    flex: 1;
    display: inline-block;
    height: 47px;

    .el-input__wrapper {
      width: 100%;
      padding: 0;
      background-color: transparent;
      box-shadow: none;

      input {
        background: transparent;
        border: 0px;
        appearance: none;
        border-radius: 0px;
        padding: 12px 5px 12px 15px;
        color: $light_gray;
        height: 47px;
        caret-color: $cursor;

        &:-webkit-autofill {
          box-shadow: 0 0 0px 1000px $bg inset !important;
          -webkit-text-fill-color: $cursor !important;
        }
      }
    }
  }

  .el-form-item {
    border: 1px solid rgba(255, 255, 255, 0.1);
    background: rgba(0, 0, 0, 0.1);
    border-radius: 5px;
    color: #454545;
  }
}
</style>

<style lang="scss" scoped>
$bg: #2d3a4b;
$dark_gray: #889aa4;
$light_gray: #eee;

.login-container {
  min-height: 100%;
  width: 100%;
  background-color: $bg;
  overflow: hidden;

  .login-form {
    position: relative;
    width: 520px;
    max-width: 100%;
    padding: 160px 35px 0;
    box-sizing: border-box;
    margin: 0 auto;
    overflow: hidden;
  }

  .svg-container {
    padding: 6px 5px 6px 15px;
    color: $dark_gray;
    vertical-align: middle;
    display: inline-block;
  }

  .title-container {
    position: relative;

    .title {
      font-size: 26px;
      color: $light_gray;
      margin: 0px auto 40px auto;
      text-align: center;
      font-weight: bold;
    }
  }

  .show-pwd {
    position: absolute;
    right: 10px;
    top: 7px;
    font-size: 16px;
    color: $dark_gray;
    cursor: pointer;
    user-select: none;
  }

  /* 验证码样式调整 */
  .captcha-item {
    :deep(.el-form-item__content) {
      display: flex;
      align-items: center;
    }

    .captcha-img {
      width: 110px;
      height: 40px;
      margin-right: 5px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      background: rgba(255, 255, 255, 0.2);
      border-radius: 4px;
      overflow: hidden;

      img {
        width: 100%;
        height: 100%;
        object-fit: cover;
      }

      .captcha-placeholder {
        font-size: 12px;
        color: $light_gray;
      }
    }
  }

  .tips {
    font-size: 14px;
    color: #fff;
    margin-bottom: 10px;

    span {
      &:first-of-type {
        margin-right: 16px;
      }
    }
  }
}
</style>