# Dpz.Core.Web.Dashboard

基于 Blazor WebAssembly 的个人网站管理后台系统。

## 项目简介

这是一个前后端分离的管理后台应用。前端主体使用 Blazor WebAssembly，
通过 REST API 与后端服务通信，支持文章、音频、视频、图片、代码笔记、
动态页面、时间线、碎碎念等内容管理能力。

现代前端资产由 `ClientApp` 统一管理，使用 npm、Vite 和 TypeScript 构建到
Blazor 的 `wwwroot/assets/`。

## 技术栈

- **应用框架**: Blazor WebAssembly (.NET 10.0)
- **前端资产**: npm、Vite、TypeScript
- **认证**: OIDC (OpenID Connect)
- **Markdown 编辑器**: Milkdown Crepe
- **代码编辑器**: CodeMirror 6
- **Markdown 渲染**: Markdig、Prism、Mermaid
- **交互组件**: Web Awesome，当前优先用于 tooltip、popover 等轻量交互
- **媒体查看**: PhotoSwipe
- **图标与字体**: FontAwesome、JetBrains Mono
- **其他依赖**: AngleSharp

## 系统地址

- **后端 API**: `https://api.dpangzi.com`
- **认证服务**: `https://auth.dpangzi.com`
- **主站**: `https://core.dpangzi.com`
- **CDN**: `https://dpangzi.com`

开发环境配置在 `wwwroot/appsettings.Development.json` 中，通常指向本地 API、
认证服务、主站和 CDN 服务。缺少 `BaseAddress`、`CDNBaseAddress` 或 `SourceSite`
会导致应用启动失败。

## 项目结构

```text
src/Dpz.Core.Web.Dashboard/
├── ClientApp/          # npm + Vite + TypeScript 前端资产工程
│   ├── src/
│   │   ├── app.ts      # 全局样式和第三方 CSS 入口
│   │   ├── editors/    # Markdown / CodeMirror 编辑器 interop
│   │   ├── interactions/ # tooltip、popover 等前端交互
│   │   ├── interop/    # Blazor JS interop 模块
│   │   └── styles/     # CSS 源文件
│   ├── package.json
│   ├── tsconfig.json
│   └── vite.config.ts
├── Pages/              # 页面组件
├── Service/            # 服务接口定义
│   └── Impl/           # 服务实现
├── Models/             # 数据模型
├── Helper/             # 辅助工具类
├── Shared/             # 共享组件
│   └── Components/     # 公共组件
├── wwwroot/
│   ├── assets/         # Vite 构建产物，hash 命名，生成目录
│   ├── resources/      # 静态资源
│   ├── appsettings.json
│   └── index.html
├── build.ps1           # 前端资产和 Blazor build 统一入口
└── Dpz.Core.Web.Dashboard.csproj
```

## 环境要求

- .NET 10.0 SDK
- Node.js 和 npm
- 可访问 `src/NuGet.config` 中配置的 NuGet 源。`dpz.core.enumlibrary`
  来自 GitHub Packages，首次还原可能需要对应凭据。

不再需要全局安装 `clean-css-cli`。CSS、第三方前端库、字体和图标均由
`ClientApp` 通过 npm 与 Vite 管理。

## 开发运行

首次运行或依赖缺失时，`build.ps1` 会自动在 `ClientApp` 中执行 `npm install`。

前端资产监听：

```powershell
cd src/Dpz.Core.Web.Dashboard
.\build.ps1 dev
```

Blazor 开发服务需要在另一个终端启动：

```powershell
cd src/Dpz.Core.Web.Dashboard
dotnet run --project .\Dpz.Core.Web.Dashboard.csproj
```

开发服务地址来自 `Properties/launchSettings.json`：

- `https://localhost:5010`
- `http://localhost:5011`

## 构建与检查

生产构建入口：

```powershell
cd src/Dpz.Core.Web.Dashboard
.\build.ps1
```

等价命令：

```powershell
.\build.ps1 prod
.\build.ps1 build
```

生产构建会先清理 `wwwroot/assets/`、`bin/`、`obj/`，再执行 Vite build，
根据 Vite manifest 同步 `wwwroot/index.html` 中的 hash 资源和版本号，
最后执行 `dotnet build`。

常用前端质量命令：

```powershell
.\build.ps1 typecheck
.\build.ps1 lint
.\build.ps1 format-check
.\build.ps1 check
.\build.ps1 format
```

清理构建产物：

```powershell
.\build.ps1 clean
```

## 发布

发布前先执行生产构建，确保 Vite hash 资产和 `index.html` 已同步：

```powershell
cd src/Dpz.Core.Web.Dashboard
.\build.ps1 prod
dotnet publish .\Dpz.Core.Web.Dashboard.csproj -c Release
```

发布产物位于：

```text
src/Dpz.Core.Web.Dashboard/bin/Release/net10.0/publish/wwwroot/
```

将该目录部署到服务器，并让 Web Server 指向发布目录中的 `wwwroot`。
单页应用路由需要 fallback 到 `index.html`。

## 配置说明

`wwwroot/appsettings.json`：

```json
{
    "BaseAddress": "https://api.dpangzi.com",
    "SourceSite": "https://core.dpangzi.com",
    "CDNBaseAddress": "https://dpangzi.com",
    "OIDC": {
        "ClientId": "manage",
        "Authority": "https://auth.dpangzi.com",
        "ResponseType": "code",
        "ResponseMode": "query"
    }
}
```

- `BaseAddress`: 后端 API 地址
- `SourceSite`: 主站地址
- `CDNBaseAddress`: CDN 地址
- `OIDC.Authority`: 认证服务器地址

## 开发约定

### C# 与 Blazor

- 使用文件作用域命名空间。
- 私有字段使用 `_camelCase`。
- 4 空格缩进，单行最大长度 100。
- `if`、`for`、`foreach`、`while` 等代码块必须使用大括号。
- 不写行尾注释。
- 严格遵循 nullable 语义。
- 优先使用主构造函数或构造函数依赖注入，避免在组件中过度使用 `[Inject]`。
- Blazor 页面和组件推荐使用 `.razor` + `.razor.cs` 代码隔离。

更完整约定见 `src/EncodingConventions.md`。

### 服务注册

项目使用反射自动注册服务：

- 接口放在 `Service/` 命名空间下。
- 实现放在 `Service/Impl/` 命名空间下。
- 一个接口应只有一个实现，注册逻辑会选择第一个匹配实现。
- API 请求统一通过 `IHttpService`，由它处理认证、401 跳转和分页封装。

### 页面组织

功能模块通常遵循：

- `List.razor`: 列表页
- `Publish.razor`: 新增页
- `Edit.razor`: 编辑页

新增模块需要在 `Shared/NavMenu.razor` 中加入入口。

## 前端开发约定

- 前端源码放在 `src/Dpz.Core.Web.Dashboard/ClientApp/`。
- TypeScript 优先，复杂交互使用 class 组织，按职责拆分模块。
- 导出给 Blazor 的 interop 函数保持稳定、窄接口。
- TypeScript 缩进 4 个空格，单行最大长度 100。
- `if`、`for`、`while` 等代码块必须使用大括号。
- 不写行尾注释。
- 第三方前端库优先通过 npm 管理，不在 `index.html` 中直接挂 CDN。
- Vite 构建产物使用 hash 文件名，Blazor 通过 manifest 解析动态模块。
- 复杂 DOM 生命周期、事件绑定、MutationObserver 等逻辑应由 class 封装。
- 交互模块按职责放入 `editors/`、`interactions/`、`interop/` 等目录。
- 支持 Chromium 内核浏览器和 Firefox。
- 响应式 UI 需要兼顾手机端、平板端和 PC 端。

### CSS

- CSS 源文件放在 `ClientApp/src/styles/`。
- `styles/app.css` 是样式入口，负责明确 import 顺序。
- 使用 BEM 命名，避免 `.card`、`.title` 这类过宽泛选择器。
- 共享样式以 `_` 开头，例如 `_layout.css`、`_variables.css`。
- 页面样式使用页面或模块前缀，例如 `article-list.css`。
- 后台系统以深色模式视觉为主，新增颜色优先复用 CSS 变量。
- 不使用 Blazor CSS isolation 承载全局页面样式。
- FontAwesome、Prism、Milkdown Crepe、PhotoSwipe、JetBrains Mono 等样式由 npm 导入。

更完整样式约定见 `src/CSS_MANAGEMENT.md`。

### 交互组件

Web Awesome 作为渐进式 Web Component 交互层使用，当前优先用于 tooltip、
popover 等轻量交互。Dialog、Toast、Confirm 等涉及 Blazor 组件内容、
业务状态和 `Task` 返回值的交互，在服务层迁移设计完成前继续使用
`IAppDialogService`。

Web Awesome 组件应按需导入具体模块，例如：

```ts
import "@awesome.me/webawesome/dist/components/tooltip/tooltip.js";
```

不要为了单个交互导入整包。

### 编辑器

- Markdown 编辑器使用 Milkdown Crepe。
- 代码和 HTML 编辑器使用 CodeMirror 6。
- Markdown 代码块支持 Prism 高亮。
- Markdown 编辑器中的 Mermaid 代码块支持预览。
- Markdown 图片上传通过 Blazor interop 调用后端上传流程。
- 编辑器内部动态生成的按钮必须保证 `type="button"`，避免位于表单中时触发 submit。

## 功能模块

- **文章管理**: Markdown 发布、编辑、删除和图片上传
- **视频管理**: 视频上传、元数据编辑、弹幕管理
- **音频管理**: 音频文件管理
- **图片管理**: 图片上传与展示
- **代码笔记**: 多语言代码片段管理与预览
- **动态页面**: 自定义页面内容管理
- **时间线**: 时间线事件管理
- **碎碎念**: 短内容发布
- **评论管理**: 评论审核与管理
- **友链管理**: 友情链接维护
- **数据统计**: 访问量与数据汇总

## 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。
