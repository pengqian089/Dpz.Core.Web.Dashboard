# Copilot Instructions for Dpz.Core.Web.Dashboard

## 项目概述

这是一个 **Blazor WebAssembly 管理后台**，用于管理个人博客系统的内容（文章、图片、音频、视频、评论等）。前端运行在浏览器中，通过 REST API 与后端 `api.dpangzi.com` 通信。

## 技术栈

- **框架**: Blazor WebAssembly (.NET 10)
- **UI 组件库**: MudBlazor 7.x（所有 UI 使用 `<Mud*>` 组件）
- **代码编辑器**: BlazorMonaco（Monaco Editor）、Cherry Markdown Editor
- **Markdown 解析**: Markdig
- **认证**: OIDC (OpenID Connect) via `auth.dpangzi.com`

## 项目结构

```
src/Dpz.Core.Web.Dashboard/
├── Service/              # 服务接口定义 (IXxxService.cs)
├── Service/Impl/         # 服务实现 (XxxService.cs)
├── Models/               # API 数据模型
├── Pages/                # Razor 页面（按功能模块分文件夹）
├── Component/            # 可复用组件（Editor, MarkdownEditor 等）
├── Shared/               # 布局组件（MainLayout, NavMenu）
├── Helper/               # 工具类（AppTools, PagedList, FetchException）
└── wwwroot/              # 静态资源（appsettings.json, CSS, JS）
```

## 关键架构模式

### 服务层自动注册
`Program.cs` 使用反射自动注册 `Service/` 下的接口与 `Service/Impl/` 下的实现：
```csharp
// 接口: Dpz.Core.Web.Dashboard.Service.IArticleService
// 实现: Dpz.Core.Web.Dashboard.Service.Impl.ArticleService
// 无需手动 AddScoped，自动扫描注册
```

### HTTP 请求统一封装
所有 API 调用通过 `IHttpService` 进行，提供：
- 自动 401 处理（跳转 `/session-expired`）
- 分页请求：`GetPageAsync<T>(uri, pageIndex, pageSize, params)`
- 文件上传：`PostFileAsync<T>(uri, MultipartFormDataContent)`

### 页面模式
- 列表页：`/article` → `Pages/Article/List.razor`
- 新增页：`/article/publish` → `Pages/Article/Publish.razor`
- 编辑页：`/article/edit/{Id}` → `Pages/Article/Edit.razor`
- Code-behind 模式：`.razor` + `.razor.cs` 分离

### 认证与授权
- 使用 `@attribute [Authorize]` 标记需要登录的页面
- `App.razor` 处理认证路由，未授权显示 `AuthLoginPrompt`
- OIDC 配置在 `wwwroot/appsettings.json`

## 代码约定

### 命名规范
- 私有字段：`_` 前缀 + 驼峰命名（如 `_httpService`）
- 参数/变量：驼峰命名（如 `pageIndex`）
- 类/接口/方法/属性：Pascal 命名（如 `ArticleService`）
- 命名空间：`项目名.目录.子目录`，使用文件作用域命名空间

### 代码风格
- 缩进：4 个空格
- 每行最大长度：100 字符
- `if/for/foreach/while` 即使只有一行也必须使用大括号
- 一个类型一个 `.cs` 文件
- 严格遵循 nullable 语义

### 依赖注入
- **优先使用构造函数注入**，而非 `[Inject]` 属性
- Blazor 组件使用主构造函数：
```csharp
public partial class List(IArticleService articleService, ISnackbar snackbar)
{
    // 直接使用 articleService、snackbar
}
```

### Blazor 页面
- 建议使用 `.razor` + `.razor.cs` 分离模式
- 路由支持多别名：
```razor
@page "/article"
@page "/article/list"
@attribute [Authorize]
```
- 使用 MudBlazor 组件，不使用原生 HTML 表单元素

### 返回值规范
- 单个对象：根据语义返回可空/不可空引用类型
- 集合/数组：无数据时返回空集合，不返回 `null`
- 参数类型尽可能抽象，返回值类型尽可能具体

### 添加新服务
1. 在 `Service/` 创建 `IXxxService.cs` 接口
2. 在 `Service/Impl/` 创建 `XxxService.cs` 实现（使用主构造函数）
3. 自动注册，无需修改 `Program.cs`

### 分页数据
```csharp
var result = await _httpService.GetPageAsync<ArticleModel>("/api/Article", pageIndex, pageSize);
// result.TotalItemCount, result.CurrentPageIndex, result.TotalPageCount
```

### 消息提示
```csharp
Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
Snackbar.Add("操作成功", Severity.Success);
```

## CSS/JS 约定

### CSS 规范
- **BEM 命名**：使用 Block-Element-Modifier 模式
  ```css
  .stat-card { }              /* Block */
  .stat-card__icon { }        /* Element */
  .stat-card__icon--primary { } /* Modifier */
  ```
- **文件组织**：
  - 页面专有样式：`index.css`、`article.css`（与页面同名）
  - 公共样式：以 `_` 前缀命名，如 `_pagination.css`、`_nav-menu.css`
- **主题适配**：使用 MudBlazor CSS 变量支持浅色/深色模式
  ```css
  background: rgba(var(--mud-palette-surface-rgb), 0.94);
  color: var(--mud-palette-text-secondary);
  ```
- **响应式设计**：使用媒体查询适配 PC/平板/手机
  ```css
  @media (max-width: 960px) { /* 平板 */ }
  @media (max-width: 600px) { /* 手机 */ }
  ```
- **浏览器兼容**：支持 Chromium 内核、Firefox

### JS 规范
- **ES 模块**：统一使用 ES Module 语法
  ```javascript
  export function initVideoPlayer(url) { }
  export async function fileUpload(file, callback) { }
  ```
- **文件组织**：按功能/模块分离
  - `app.js` - 通用工具函数
  - `markdown.js` - Markdown 编辑器相关
  - `video.js` - 视频播放器相关
- **命名**：函数使用驼峰命名，常量使用 UPPER_SNAKE_CASE

## 构建与运行

```powershell
cd src/Dpz.Core.Web.Dashboard
dotnet run                    # 开发运行
dotnet publish -c Release     # 发布构建
.\build.ps1                   # 合并 CSS 文件（需要 cleancss）
```

## 配置文件

`wwwroot/appsettings.json` 配置 API 地址和 OIDC：
```json
{
  "BaseAddress": "https://api.dpangzi.com",
  "OIDC": { "ClientId": "manage", "Authority": "https://auth.dpangzi.com" }
}
```
通过 `Program.BaseAddress` 全局访问。

## 常见任务示例

### 新增 CRUD 页面
1. 在 `Models/` 添加数据模型
2. 在 `Service/` 添加 `IXxxService` 接口
3. 在 `Service/Impl/` 添加实现（注入 `IHttpService`）
4. 在 `Pages/Xxx/` 创建 `List.razor`, `Publish.razor`, `Edit.razor`
5. 在 `Shared/NavMenu.razor` 添加导航链接

### Markdown 编辑器集成
```razor
<MarkdownEditor @ref="_editor" Markdown="@Model.Markdown" UploadAction="/api/Xxx/upload" />
@code {
    private MarkdownEditor _editor;
    var content = await _editor.GetValueAsync();
}
```
