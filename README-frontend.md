# nekohub-maui 前端（Posts 功能）

本应用是一个 .NET MAUI 跨平台前端（Android/iOS/MacCatalyst/Windows），采用 MVVM 并通过 REST API 对接 Blog Core 后端，完成 Posts 的列表、详情、创建、编辑、删除、搜索/刷新、发布/取消发布等功能。

## 运行与调试

1. 启动后端（Blog Core API）
   - 默认基础地址：http://localhost:5249
   - 主要端点：/api/posts、/api/posts/paged、/api/posts/{id} 等

2. 运行前端（VS/VS Code/Rider）
   - 选择目标平台（Android 模拟器、iOS 模拟器、Windows、MacCatalyst）
   - 直接启动调试

3. BaseUrl/端口
   - Android 模拟器：使用 10.0.2.2 指向宿主机 localhost（已在 Services/AppConfig.cs 中处理）
   - iOS 模拟器、Windows、MacCatalyst：使用 localhost
   - 默认端口为 5249，如后端端口变更，请修改 Services/AppConfig.cs 中的 defaultPort 常量，或改造为读取配置/环境变量

## 功能说明

- Posts 列表（Pages/PostsPage）：
  - 搜索：按标题/内容关键字模糊匹配
  - 筛选：全部/已发布/草稿
  - 排序：更新时间/创建时间/标题（升/降）
  - 分页：使用“加载更多”
  - 下拉刷新、空态、错误态与重试
- 详情（Pages/PostDetailPage）：
  - 查看标题、内容、状态、更新时间
  - 发布/取消发布、编辑、删除（删除前带确认）
- 编辑（Pages/EditPostPage）：
  - 新建/编辑统一界面
  - 基于后端 ValidationProblemDetails 的表单校验提示（标题、内容）
  - 保存过程禁用按钮并显示进度

## 技术实现

- MVVM：ViewModels/* + Pages/*，基础类 ViewModels/BaseViewModel 实现 INotifyPropertyChanged
- 服务层：Services/IPostsApi 与 PostsApiService
  - HttpClient Timeout 15s，支持取消令牌
  - GET 采用指数退避的重试（最多 3 次）
  - System.Text.Json（camelCase），映射 RFC7807/ValidationProblemDetails
  - ILogger 记录关键步骤与异常
- 依赖注入：MauiProgram 中注册 HttpClient、服务、ViewModel 与页面
- 导航：Shell 路由（AppShell 注册），通过 QueryProperty 传参
- 可用性：统一错误提示、关键控件设置 AutomationProperties（可按需再完善）

## 常见问题与 CORS

- 移动端模拟器与宿主机的 localhost 不同：Android 使用 10.0.2.2
- 若部署真机，需要保证手机与后端服务可网络互通（WIFI 同网段，或通过反向代理暴露）
- 如遇到 CORS 问题（主要在 Web 前端），MAUI 原生应用无需浏览器 CORS；仅需确保网络可达

## 扩展建议

- 将 BaseUrl 改为可配置（Preferences、环境变量、编译常量）
- 引入 CommunityToolkit.Mvvm 精简样板代码
- 增加单元测试项目，使用 HttpMessageHandler stub 覆盖超时/4xx/5xx
- 增加 UI 测试（.NET MAUI UITest）
