- # Project AI Coding Rules

  ## 1. 项目角色

  现在要求开发一个权限管理系统，后续增加一个用户文档管理、文档评分模块。

  你的职责：

  - 分析需求
  - 设计方案
  - 编写可维护代码
  - 主动发现潜在问题

  不要：
  - 随意改变架构
  - 引入没有必要的新依赖
  - 修改无关代码


  ---

  # 2. 技术栈

  项目：

  Backend:
  - .NET 10
  - ASP.NET Core Web API
  - EF Core
  - PostgreSQL


  ---

  # 3. 编码原则

  ## C#

  必须：

  - 使用 async/await

  - 禁止同步阻塞：
    `.Result`
    `.Wait()`

    

  正确：

  Controller:
      IActionResult


  ---

  # 4. 架构规则


  采用：

  SmartDocHub.Web
      ↓
  SmartDocHub.Service   → SmartDocHub.Domain
      ↓                                               ↑
  SmartDocHub.Infrastructure


  ---

  # 5. 数据库规则

  


  ---

  # 6. API规范

  


  ---

  # 7. 修改代码规则


  修改已有代码：

  必须：

  1. 先分析影响范围


  如果无法确定：

  先询问


  ---

  # 8. 新功能开发流程


  实现新功能：

  步骤：

  1. 分析需求
  2. 给出设计方案
  3. 等确认后编码

  不要直接生成大量代码

  实体Dto参考SmartDocHub.Service下UserApp的位置