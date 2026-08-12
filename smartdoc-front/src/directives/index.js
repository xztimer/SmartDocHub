import permission from './permission'

const directives = {
  permission
}
export default {
  install(app) {
    Object.entries(directives).forEach(([name, directive]) => {
      app.directive(name, directive)
    })
  }
}
