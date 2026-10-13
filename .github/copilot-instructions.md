# Copilot Instructions for Dpz.Core.Web.Management

## 项目定位与架构要点
- Blazor WebAssembly 管理后台，通过 REST API 与 api.dpangzi.com 通信。
- 仓库为多项目布局：应用在 `src/Dpz.Core.Web.Management/`，前端 npm/Vite/TypeScript 工程在 `src/Dpz.Core.Web.Management.Frontend/`（Razor Class Library，Vite 产物输出到自身 `wwwroot/assets/`，由应用项目 `ProjectReference` 合并）。
- 服务层接口与实现分离：Service/ 下定义 IXxxService，Service/Impl/ 下实现；Program.cs 反射自动注册，无需手动 AddScoped。
- HTTP 统一走 IHttpService：处理 401 自动跳转 /session-expired，提供分页与文件上传封装。
- 页面组织按功能模块分文件夹；列表/新增/编辑遵循 List.razor / Publish.razor / Edit.razor 模式。
- 推荐 code-behind：.razor + .razor.cs 分离。

## 开发与构建流程
- 进入目录：src/Dpz.Core.Web.Management
- 开发运行（前端监听 + 应用）：build.ps1 dev
- 生产构建（Vite + 同步 index.html + dotnet build）：build.ps1 prod
- 发布构建：dotnet publish -c Release（Frontend 资产由 MSBuild 自动合并）
- 解决方案与 NuGet.config 位于仓库根；公共 MSBuild 属性在 Directory.Build.props。
- 前端质量检查：build.ps1 check（typecheck + lint + format:check）。CSS/JS 由 npm 与 Vite 管理，不再使用 cleancss。

## 配置与认证
- wwwroot/appsettings.json 配置 BaseAddress 与 OIDC（auth.dpangzi.com）。
- 页面权限：使用 @attribute [Authorize]；App.razor 负责认证路由与 AuthLoginPrompt。

## 代码与样式约定（项目特有）
- 依赖注入：优先构造函数注入；Blazor 组件使用主构造函数（例：public partial class List(IArticleService articleService)）。
- 命名：私有字段以下划线前缀；文件作用域命名空间。
- 代码风格：4 空格缩进，行宽 100，控制语句强制大括号，严格 nullable。
- 分页：_httpService.GetPageAsync<T>("/api/Article", pageIndex, pageSize)，使用返回的 TotalItemCount/CurrentPageIndex/TotalPageCount。

## 前端资源规范
- CSS：BEM 命名；页面样式与页面同名（如 index.css），公共样式以下划线前缀（如 _pagination.css）。
- JS/TS：ES Module；按模块拆分，交互模块按职责放入 `editors/`、`interactions/`、`interop/`、`pages/`。
- 导出给 Blazor 的 interop 模块需在 `vite.config.ts` 注册为 Vite 入口，并在运行时通过 IAssetManifestService.GetAssetPathAsync 解析。

## 常见扩展路径
- 新增模块：Models/ + Service/ + Service/Impl/ + Pages/<Module>/ + Shared/NavMenu.razor.cs 的 Groups 列表。
- 编辑器组件示例：Shared/Components/ 下的 MarkdownEditor / Editor 供页面复用。
